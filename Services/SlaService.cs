using DamslaApi.Models;
using DamslaApi.Data;
using DamslaApi.Dtos.Sla;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class SlaService
    {
        private readonly DamslaDbContext _db;

        public SlaService(DamslaDbContext db)
        {
            _db = db;
        }

        // --------------------------------------------------
        // Cálculo individual SLA1 / SLA2
        // --------------------------------------------------
        public string CalcularSla(DateTime solicitud, DateTime ingreso, string tipo)
        {
            int dias = (ingreso - solicitud).Days;

            if (tipo == "SLA1")
                return dias < 35 ? "Cumple SLA1" : "No Cumple SLA1";

            if (tipo == "SLA2")
                return dias < 20 ? "Cumple SLA2" : "No Cumple SLA2";

            return "NA";
        }

        // --------------------------------------------------
        // Lista completa de indicadores por solicitud
        // --------------------------------------------------
        public async Task<List<IndicadorSlaDto>> GetIndicadores()
        {
            var solicitudes = await _db.Solicitudes.Include(s => s.TipoSla).ToListAsync();

            return solicitudes.Select(s => new IndicadorSlaDto
            {
                Id = s.Id,
                Rol = s.Rol,
                TipoSla = s.TipoSla.Codigo,
                FechaSolicitud = s.FechaSolicitud,
                FechaIngreso = s.FechaIngreso ?? DateTime.UtcNow,
                Dias = (s.FechaIngreso.HasValue ? (s.FechaIngreso.Value - s.FechaSolicitud).Days : (DateTime.UtcNow - s.FechaSolicitud).Days),
                Resultado = CalcularSla(s.FechaSolicitud, s.FechaIngreso ?? DateTime.UtcNow, s.TipoSla.Codigo)
            }).ToList();
        }

        // --------------------------------------------------
        // Resumen por rol (KPI)
        // --------------------------------------------------
        public async Task<List<SlaResumenDto>> GetResumenPorRol()
        {
            var datos = await GetIndicadores();

            var agrupado = datos
                .GroupBy(x => x.Rol)
                .Select(g => new SlaResumenDto
                {
                    Rol = g.Key,
                    TotalSolicitudes = g.Count(),
                    Cumplen = g.Count(x => x.Resultado.Contains("Cumple")),
                    NoCumplen = g.Count(x => x.Resultado.Contains("No")),
                    PorcentajeCumplimiento = g.Any() 
                        ? Math.Round((double)g.Count(x => x.Resultado.Contains("Cumple")) / g.Count() * 100, 2)
                        : 0,
                    IndicadorColor = g.Any()
                        ? (g.Count(x => x.Resultado.Contains("Cumple")) == g.Count() ? "green"
                            : g.Count(x => x.Resultado.Contains("Cumple")) == 0 ? "gray" : "red")
                        : "gray"
                })
                .ToList();

            return agrupado;
        }

        // --------------------------------------------------
        // Métricas globales: total, cumplen, no cumplen, porcentajes
        // --------------------------------------------------
        public async Task<Dtos.Sla.MetricsGlobDto> GetMetricsGlob()
        {
            var indicadores = await GetIndicadores();

            var total = indicadores.Count;
            var cumplen = indicadores.Count(x => x.Resultado.Contains("Cumple"));
            var noCumplen = indicadores.Count(x => x.Resultado.Contains("No"));

            double porcentajeCumplen = total > 0 ? Math.Round((double)cumplen / total * 100, 2) : 0;
            double porcentajeNoCumplen = total > 0 ? Math.Round((double)noCumplen / total * 100, 2) : 0;

            // Calcular retrasos (en días)
            var retrasos = indicadores.Select(i => i.Dias).ToList();
            int retrasoMax = retrasos.Any() ? retrasos.Max() : 0;
            double retrasoPromedio = retrasos.Any() ? Math.Round(retrasos.Average(), 2) : 0;

            return new Dtos.Sla.MetricsGlobDto
            {
                TotalSolicitudes = total,
                Cumplen = cumplen,
                NoCumplen = noCumplen,
                PorcentajeCumplidos = porcentajeCumplen,
                PorcentajeNoCumplidos = porcentajeNoCumplen,
                RetrasoMaximoDias = retrasoMax,
                RetrasoPromedioDias = retrasoPromedio
            };
        }

        // --------------------------------------------------
        // Métricas agrupadas por TipoSla
        // --------------------------------------------------
        public async Task<List<Dtos.Sla.MetricsByTipoDto>> GetMetricsByTipo()
        {
            var indicadores = await GetIndicadores();

            var agrupado = indicadores
                .GroupBy(x => x.TipoSla)
                .Select(g =>
                {
                    var total = g.Count();
                    var cumplen = g.Count(x => x.Resultado.Contains("Cumple"));
                    var noCumplen = g.Count(x => x.Resultado.Contains("No"));
                    var porcentajeCumplen = total > 0 ? Math.Round((double)cumplen / total * 100, 2) : 0;
                    var porcentajeNoCumplen = total > 0 ? Math.Round((double)noCumplen / total * 100, 2) : 0;

                    // Calcular retrasos para este tipo
                    var retrasosTipo = g.Select(x => x.Dias).ToList();
                    int retrasoMaxTipo = retrasosTipo.Any() ? retrasosTipo.Max() : 0;
                    double retrasoPromTipo = retrasosTipo.Any() ? Math.Round(retrasosTipo.Average(), 2) : 0;

                    return new Dtos.Sla.MetricsByTipoDto
                    {
                        TipoSla = g.Key,
                        TotalSolicitudes = total,
                        Cumplen = cumplen,
                        NoCumplen = noCumplen,
                        PorcentajeCumplidos = porcentajeCumplen,
                        PorcentajeNoCumplidos = porcentajeNoCumplen,
                        RetrasoMaximoDias = retrasoMaxTipo,
                        RetrasoPromedioDias = retrasoPromTipo
                    };
                })
                .ToList();

            return agrupado;
        }
    }
}
