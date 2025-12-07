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
            try
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

            // 2. Obtener estadísticas globales del sistema (todas las solicitudes)
            var todasSolicitudes = await _db.Solicitudes
                .Include(s => s.TipoSla)
                .ToListAsync();

            // Sólo procesadas para cálculos SLA
            var procesadasSistema = todasSolicitudes.Where(s => s.FechaIngreso.HasValue).ToList();
            int totalSistema = procesadasSistema.Count;

            // Fechas de referencia para ventanas de tiempo
            var hoy = DateTime.UtcNow.Date;
            var inicioMes = hoy.AddMonths(-1);
            var inicioTrimestre = hoy.AddMonths(-3);
            var inicioSemana = hoy.AddDays(-7);

            const int limiteSla1 = 35;
            const int limiteSla2 = 20;

            // Función local para métricas SLA1/SLA2
            (int total, int cumple1, int incumple1, int cumple2, int incumple2, double avgDias) Calcular(IEnumerable<dynamic> items)
            {
                var lista = items.ToList();
                int total = lista.Count;
                int cumple1 = lista.Count(s => s.Dias < limiteSla1);
                int incumple1 = lista.Count(s => s.Dias >= limiteSla1);
                int cumple2 = lista.Count(s => s.Dias < limiteSla2);
                int incumple2 = lista.Count(s => s.Dias >= limiteSla2);
                double avgDias = total > 0 ? Math.Round(lista.Average(s => (double)s.Dias), 2) : 0;
                return (total, cumple1, incumple1, cumple2, incumple2, avgDias);
            }

            var enriquecidas = procesadasSistema.Select(s => new
            {
                Rol = string.IsNullOrWhiteSpace(s.Rol) ? "(Sin Rol)" : s.Rol,
                TipoSla = s.TipoSla.Descripcion,
                FechaSolicitud = s.FechaSolicitud,
                FechaIngreso = s.FechaIngreso!.Value,
                Dias = (s.FechaIngreso!.Value - s.FechaSolicitud).Days
            }).ToList();

            var metricasGlobales = Calcular(enriquecidas);
            var metricasMes = Calcular(enriquecidas.Where(s => s.FechaSolicitud >= inicioMes));
            var metricasSemana = Calcular(enriquecidas.Where(s => s.FechaSolicitud >= inicioSemana));
            var metricasTrimestre = Calcular(enriquecidas.Where(s => s.FechaSolicitud >= inicioTrimestre));

            var porRol = enriquecidas
                .GroupBy(s => s.Rol)
                .Select(g => new
                {
                    Rol = g.Key,
                    Total = g.Count(),
                    IncumpleSla1 = g.Count(x => x.Dias >= limiteSla1),
                    CumpleSla1 = g.Count(x => x.Dias < limiteSla1),
                    PromDias = Math.Round(g.Average(x => (double)x.Dias), 2)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Tendencia simple SLA1 últimos 3 meses (tasa cumplimiento mensual)
            var mensual = enriquecidas
                .GroupBy(s => new { s.FechaSolicitud.Year, s.FechaSolicitud.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Periodo = $"{g.Key.Year}-{g.Key.Month:00}",
                    Cumplimiento = g.Count(x => x.Dias < limiteSla1),
                    Total = g.Count()
                })
                .Where(x => x.Total > 0)
                .ToList();

            double tendenciaSla1 = 0;
            if (mensual.Count >= 2)
            {
                var primero = mensual.First();
                var ultimo = mensual.Last();
                double p1 = Math.Round((double)primero.Cumplimiento / primero.Total * 100, 2);
                double p2 = Math.Round((double)ultimo.Cumplimiento / ultimo.Total * 100, 2);
                tendenciaSla1 = Math.Round(p2 - p1, 2); // cambio porcentual simple
            }

            // Tabla reducida: últimas 20 solicitudes (para trazabilidad breve)
            var tablaReducida = string.Join("\n", todasSolicitudes
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(20)
                .Select((s, i) =>
                {
                    var rol = string.IsNullOrWhiteSpace(s.Rol) ? "(Sin Rol)" : s.Rol;
                    var fechaIngStr = s.FechaIngreso.HasValue ? s.FechaIngreso.Value.ToString("yyyy-MM-dd") : "pendiente";
                    var dias = s.FechaIngreso.HasValue ? (s.FechaIngreso.Value - s.FechaSolicitud).Days : 0;
                    var resumen = s.FechaIngreso.HasValue
                        ? $"SLA1_{(dias < limiteSla1 ? "OK" : "FAIL")}; SLA2_{(dias < limiteSla2 ? "OK" : "FAIL")}" 
                        : "pendiente";
                    return $"  {i + 1}. Rol: {rol} | Fecha_Solicitud: {s.FechaSolicitud:yyyy-MM-dd} | TipoSLA: {s.TipoSla.Descripcion} | Fecha_Ingreso: {fechaIngStr} | Días: {dias} | {resumen}";
                }));

            var incumpleSla1 = enriquecidas.Where(x => x.Dias >= limiteSla1).ToList();
            var incumpleSla2 = enriquecidas.Where(x => x.Dias >= limiteSla2).ToList();
            var ejemplosCumplen = enriquecidas.Where(x => x.Dias < limiteSla1)
                .OrderBy(x => x.FechaSolicitud)
                .Take(5)
                .Select((x, i) => $"  {i + 1}. Rol {x.Rol}, Fecha {x.FechaSolicitud:yyyy-MM-dd}, Días {x.Dias}");

            var tendenciaLista = mensual
                .TakeLast(3)
                .Select(m =>
                {
                    var pct = Math.Round((double)m.Cumplimiento / m.Total * 100, 2);
                    return $"  {m.Periodo}: {pct}%";
                });

            // 4. Crear contexto ULTRA compacto (sin tabla, sin ejemplos)
            string contexto = $@"
Eres un asistente de SLA. Responde de forma directa, conversacional y amigable. 
NO repitas frases como ""Según la información"", ""en el contexto"", etc.
Habla naturalmente. Sé breve y conciso.

HOY: {hoy:yyyy-MM-dd}
ÚLTIMOS 7 DÍAS: {inicioSemana:yyyy-MM-dd} a {hoy:yyyy-MM-dd}
ÚLTIMO MES: {inicioMes:yyyy-MM-dd} a {hoy:yyyy-MM-dd}
ÚLTIMO TRIMESTRE: {inicioTrimestre:yyyy-MM-dd} a {hoy:yyyy-MM-dd}

SLA1 < {limiteSla1} días | SLA2 < {limiteSla2} días

GLOBALES: {metricasGlobales.total} total | SLA1: {metricasGlobales.cumple1}✓/{metricasGlobales.incumple1}✗ | SLA2: {metricasGlobales.cumple2}✓/{metricasGlobales.incumple2}✗ | Prom: {metricasGlobales.avgDias}d

SEMANA: SLA1 {metricasSemana.cumple1}✓/{metricasSemana.incumple1}✗ | {metricasSemana.avgDias}d
MES: SLA1 {metricasMes.cumple1}✓/{metricasMes.incumple1}✗ | SLA2 {metricasMes.cumple2}✓/{metricasMes.incumple2}✗ | {metricasMes.avgDias}d
TRIMESTRE: SLA1 {metricasTrimestre.cumple1}✓/{metricasTrimestre.incumple1}✗

TENDENCIA (últimos meses):
{string.Join(", ", tendenciaLista.Select(l => l.Trim()))}

ROLES:
{string.Join(" | ", porRol.Take(10).Select(r => $"{r.Rol}: {r.Total}({r.CumpleSla1}✓/{r.IncumpleSla1}✗) {r.PromDias}d"))}

CRÍTICOS:
SLA1 >=35d: {incumpleSla1.Count}
SLA2 >=20d: {incumpleSla2.Count}

PREGUNTA: {request.Message}
";

            // 3. Crear payload para GROQ
            var payload = new
            {
                model = _config["GROQ_MODEL"] ?? "llama-3.1-405b",
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

                if (!response.IsSuccessStatusCode)
                {
                    var msg = responseText.Length > 500 ? responseText[..500] + "..." : responseText;
                    return new ChatResponseDto { Reply = $"Error del modelo ({response.StatusCode}): {msg}" };
                }

                using var doc = JsonDocument.Parse(responseText);

                string reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No se obtuvo respuesta.";

                return new ChatResponseDto { Reply = reply };
            }
            catch (Exception ex)
            {
                return new ChatResponseDto { Reply = $"Error procesando la solicitud: {ex.Message}" };
            }
        }
    }
}
