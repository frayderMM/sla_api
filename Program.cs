using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DamslaApi.Data;
using DamslaApi.Services;
using DamslaApi.Utils;
using DamslaApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con PostgreSQL (Aurora)
builder.Services.AddDbContext<DamslaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
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

// Registrar tarea en segundo plano para alertas automáticas
builder.Services.AddHostedService<BackgroundAlertTask>();

// Configurar CORS para aplicaciones móviles
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
