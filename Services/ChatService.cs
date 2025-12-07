using System.Net.Http;
using System.Text;
using System.Text.Json;
using DamslaApi.Data;
using DamslaApi.Dtos.Chat;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class ChatService
    {
        private readonly DamslaDbContext _db;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ChatService(
            DamslaDbContext db,
            IHttpClientFactory httpFactory,
            IConfiguration config)
        {
            _db = db;
            _http = httpFactory.CreateClient();
            _config = config;
        }

        public async Task<ChatResponseDto> ProcessMessageAsync(ChatRequestDto request)
        {
            // 1. Extraer TODAS las solicitudes del usuario con detalles completos
            var solicitudesUsuario = await _db.Solicitudes
                .Include(s => s.TipoSla)
                .Where(s => s.UsuarioId == request.UserId)
                .ToListAsync();

            // Calcular métricas del usuario
            var procesadas = solicitudesUsuario.Where(s => s.FechaIngreso.HasValue).ToList();
            int totalUsuario = procesadas.Count;

            int cumplenUsuario = procesadas.Count(s =>
            {
                var dias = (s.FechaIngreso!.Value - s.FechaSolicitud).Days;
                return dias <= s.TipoSla.TiempoRespuesta;
            });

            int incumplenUsuario = totalUsuario - cumplenUsuario;
            double porcentajeCumplimiento = totalUsuario > 0 ? Math.Round((double)cumplenUsuario / totalUsuario * 100, 2) : 0;

            // 2. Obtener estadísticas globales del sistema
            var todasSolicitudes = await _db.Solicitudes
                .Include(s => s.TipoSla)
                .Where(s => s.FechaIngreso.HasValue)
                .ToListAsync();

            int totalSistema = todasSolicitudes.Count;
            
            var porRol = todasSolicitudes
                .GroupBy(s => s.Rol)
                .Select(g => new
                {
                    Rol = string.IsNullOrWhiteSpace(g.Key) ? "(Sin Rol)" : g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var porTipoSla = todasSolicitudes
                .GroupBy(s => s.TipoSla.Descripcion)
                .Select(g => new
                {
                    TipoSla = g.Key,
                    Total = g.Count()
                })
                .ToList();

            // 3. Detalle de las solicitudes del usuario (últimas 10)
            var ultimasSolicitudes = procesadas
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(10)
                .Select(s => new
                {
                    Rol = s.Rol,
                    TipoSla = s.TipoSla.Descripcion,
                    FechaSolicitud = s.FechaSolicitud.ToString("yyyy-MM-dd"),
                    FechaIngreso = s.FechaIngreso?.ToString("yyyy-MM-dd"),
                    Dias = s.FechaIngreso.HasValue ? (s.FechaIngreso.Value - s.FechaSolicitud).Days : 0,
                    Cumple = s.FechaIngreso.HasValue && (s.FechaIngreso.Value - s.FechaSolicitud).Days <= s.TipoSla.TiempoRespuesta
                })
                .ToList();

            // 4. Crear el contexto enriquecido para el modelo LLM
            string contexto = $@"
=== INFORMACIÓN DEL SISTEMA SLA ===

Estadísticas Globales:
- Total de solicitudes procesadas en el sistema: {totalSistema}
- Solicitudes pendientes sin procesar: {await _db.Solicitudes.CountAsync() - totalSistema}

Top 5 Roles con más solicitudes:
{string.Join("\n", porRol.Select(r => $"  • {r.Rol}: {r.Total} solicitudes"))}

Distribución por Tipo de SLA:
{string.Join("\n", porTipoSla.Select(t => $"  • {t.TipoSla}: {t.Total} solicitudes"))}

=== INFORMACIÓN DEL USUARIO (ID: {request.UserId}) ===

Resumen de Desempeño:
- Total de solicitudes: {totalUsuario}
- Cumplieron SLA: {cumplenUsuario}
- Incumplieron SLA: {incumplenUsuario}
- Porcentaje de cumplimiento: {porcentajeCumplimiento}%

Últimas 10 Solicitudes del Usuario:
{string.Join("\n", ultimasSolicitudes.Select((s, i) => $"  {i + 1}. Rol: {s.Rol} | Tipo: {s.TipoSla} | Solicitado: {s.FechaSolicitud} | Ingresado: {s.FechaIngreso} | Días: {s.Dias} | {(s.Cumple ? "✓ CUMPLE" : "✗ NO CUMPLE")}"))}

=== PREGUNTA DEL USUARIO ===
""{request.Message}""
";

            // 3. Crear payload para GROQ
            var payload = new
            {
                model = _config["GROQ_MODEL"] ?? "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "system", content = "Eres un asistente inteligente del sistema de SLA. NO inventes datos. Usa solo el contexto." },
                    new { role = "user", content = contexto }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 4. Headers
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {_config["GROQ_API_KEY"]}"
            );

            // 5. Llamada a Groq
            var response = await _http.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                content
            );

            var responseText = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseText);

            string reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No se obtuvo respuesta.";

            return new ChatResponseDto { Reply = reply };
        }
    }
}
