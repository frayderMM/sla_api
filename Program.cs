using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DamslaApi.Data;
using DamslaApi.Services;
using DamslaApi.Utils;
using DamslaApi.Models;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno (GROQ_API_KEY, GROQ_MODEL)
builder.Configuration.AddEnvironmentVariables();

// Configurar DbContext con PostgreSQL (Aurora) - Variable de entorno para Render
var connectionString = builder.Configuration.GetValue<string>("CONN_STR") ?? 
                       builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DamslaDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Registrar servicios
builder.Services.AddScoped<SlaService>();
builder.Services.AddScoped<SolicitudesService>();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PrediccionService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddScoped<AlertasService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<DashboardPrincipalService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ChatService>();

// Registrar tarea en segundo plano para alertas automáticas
builder.Services.AddHostedService<BackgroundAlertTask>();

// Configurar CORS - Permitir todo para Render y móviles
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Agregar controladores y Swagger
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = "rol"  // Mapear el claim "rol" del JWT a los roles de ASP.NET
        };
    });

builder.Services.AddAuthorization();

// Configurar límites de carga de archivos
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 30_000_000; // 30 MB
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configurar Kestrel para aceptar archivos grandes
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30_000_000; // 30 MB
});

var app = builder.Build();

// Poblar datos iniciales
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DamslaDbContext>();
    
    // Verificar si ya existen datos
    if (!db.Roles.Any())
    {
        // Crear roles
        db.Roles.AddRange(
            new Rol { Nombre = "analista" },
            new Rol { Nombre = "general" }
        );
        db.SaveChanges();
    }

    if (!db.TiposSla.Any())
    {
        // Crear tipos SLA
        db.TiposSla.AddRange(
            new TipoSla { Codigo = "SLA1", Descripcion = "Atención máxima 35 días" },
            new TipoSla { Codigo = "SLA2", Descripcion = "Atención máxima 20 días" }
        );
        db.SaveChanges();
    }

    if (!db.Usuarios.Any())
    {
        // Crear usuarios por defecto
        var analistaRol = db.Roles.First(r => r.Nombre == "analista");
        var generalRol = db.Roles.First(r => r.Nombre == "general");

        db.Usuarios.AddRange(
            new Usuario 
            { 
                Nombre = "Analista TCS",
                Email = "analista@tcs.com",
                Password_Hash = BCrypt.Net.BCrypt.HashPassword("Analista123!"),
                Rol = analistaRol
            },
            new Usuario 
            { 
                Nombre = "Usuario General",
                Email = "general@tcs.com",
                Password_Hash = BCrypt.Net.BCrypt.HashPassword("General123!"),
                Rol = generalRol
            }
        );
        db.SaveChanges();
    }
}

// Configurar pipeline HTTP
// Swagger habilitado en todos los entornos para Render
app.UseSwagger();
app.UseSwaggerUI();

// No usar HTTPS redirection en Render (Render maneja SSL)
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Habilitar buffering para streams (necesario para Render)
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint raíz con información de la API
app.MapGet("/", () =>
    Results.Json(new {
        api = "DAM SLA API",
        status = "running",
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        swagger = "/swagger/index.html",
        timestamp = DateTime.UtcNow
    })
);

app.Run();
