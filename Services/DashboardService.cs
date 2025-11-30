using DamslaApi.Data;
using DamslaApi.Dtos.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class DashboardService
    {
        private readonly DamslaDbContext _db;
        private readonly SlaService _sla;

        public DashboardService(DamslaDbContext db, SlaService sla)
        {
            _db = db;
            _sla = sla;
        }

        public async Task<List<DashboardMensualDto>> GetDashboardMensual(int year)
        {
            var indicadores = await _sla.GetIndicadores();

            var agrupado = indicadores
                .Where(x => x.FechaIngreso.Year == year)
                .GroupBy(x => new
                {
                    Mes = x.FechaIngreso.Month,
                    x.Rol
                })
                .Select(g => new DashboardMensualDto
                {
                    Mes = $"{g.Key.Mes:00}-{year}",
                    Rol = g.Key.Rol,
                    Total = g.Count(),
                    Cumplen = g.Count(x => x.Resultado.Contains("Cumple")),
                    NoCumplen = g.Count(x => !x.Resultado.Contains("Cumple")),
                    Porcentaje = Math.Round((double)g.Count(x => x.Resultado.Contains("Cumple")) / g.Count() * 100, 2),
                    Color = g.Count(x => x.Resultado.Contains("Cumple")) == g.Count() ? "green"
                            : g.Count(x => x.Resultado.Contains("Cumple")) == 0 ? "gray"
                            : "red"
                })
                .OrderBy(x => x.Mes)
                .ToList();

            return agrupado;
        }
    }
}
