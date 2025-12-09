using Microsoft.EntityFrameworkCore;
using DamslaApi.Data;
using DamslaApi.Dtos.DashboardPrincipal;

namespace DamslaApi.Services;

public class DashboardPrincipalService
{
    private readonly DamslaDbContext _context;

    public DashboardPrincipalService(DamslaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ENDPOINT 1 - Indicadores principales (Cumple / No Cumple)
    /// </summary>
    public async Task<IndicadoresDto?> ObtenerIndicadores(string codigoTipoSla)
    {
        var tipoSla = await _context.TiposSla
            .FirstOrDefaultAsync(t => t.Codigo == codigoTipoSla);

        if (tipoSla == null) return null;

        var solicitudes = await _context.Solicitudes
            .Where(s => s.TipoSlaId == tipoSla.Id && s.FechaIngreso.HasValue)
            .ToListAsync();

        if (!solicitudes.Any())
        {
            return new IndicadoresDto
            {
                TipoSla = codigoTipoSla,
                Total = 0,
                Cumple = 0,
                NoCumple = 0,
                PorcentajeCumplimiento = 0,
                PromedioDias = 0
            };
        }

        var total = solicitudes.Count;
        var cumple = solicitudes.Count(s =>
        {
            var dias = (s.FechaIngreso.Value - s.FechaSolicitud).Days;
            return dias <= tipoSla.TiempoRespuesta;
        });
        var noCumple = total - cumple;

        var promedioDias = solicitudes.Average(s => (s.FechaIngreso.Value - s.FechaSolicitud).Days);

        return new IndicadoresDto
        {
            TipoSla = codigoTipoSla,
            Total = total,
            Cumple = cumple,
            NoCumple = noCumple,
            PorcentajeCumplimiento = Math.Round((double)cumple / total * 100, 1),
            PromedioDias = Math.Round(promedioDias, 1)
        };
    }

    /// <summary>
    /// ENDPOINT 2 - Indicadores históricos para gráfica
    /// </summary>
    public async Task<HistoricoDto?> ObtenerHistorico(string codigoTipoSla)
    {
        var tipoSla = await _context.TiposSla
            .FirstOrDefaultAsync(t => t.Codigo == codigoTipoSla);

        if (tipoSla == null) return null;

        var solicitudes = await _context.Solicitudes
            .Where(s => s.TipoSlaId == tipoSla.Id && s.FechaIngreso.HasValue)
            .ToListAsync();

        // Agrupar por mes y calcular porcentaje de cumplimiento
        var porMes = solicitudes
            .GroupBy(s => new
            {
                Año = s.FechaSolicitud.Year,
                Mes = s.FechaSolicitud.Month
            })
            .Select(g => new
            {
                g.Key.Año,
                g.Key.Mes,
                Total = g.Count(),
                Cumple = g.Count(s => (s.FechaIngreso.Value - s.FechaSolicitud).Days <= tipoSla.TiempoRespuesta)
            })
            .OrderBy(x => x.Año)
            .ThenBy(x => x.Mes)
            .ToList();

        var historico = porMes
            .Select(m => new PuntoHistoricoDto
            {
                Periodo = $"{m.Año}-{m.Mes:D2}",
                Porcentaje = m.Total > 0 ? Math.Round((double)m.Cumple / m.Total * 100, 1) : 0
            })
            .Where(p => p.Porcentaje > 0) // Filtrar meses con 0% de cumplimiento
            .ToList();

        return new HistoricoDto
        {
            TipoSla = codigoTipoSla,
            Historico = historico
        };
    }

    /// <summary>
    /// ENDPOINT 3 - Regresión lineal completa con proyección
    /// </summary>
    public async Task<RegresionDto?> ObtenerRegresionLineal(string codigoTipoSla)
    {
        var tipoSla = await _context.TiposSla
            .FirstOrDefaultAsync(t => t.Codigo == codigoTipoSla);

        if (tipoSla == null) return null;

        var solicitudes = await _context.Solicitudes
            .Where(s => s.TipoSlaId == tipoSla.Id && s.FechaIngreso.HasValue)
            .ToListAsync();

        if (!solicitudes.Any())
        {
            return new RegresionDto
            {
                TipoSla = codigoTipoSla,
                Regresion = new RegresionLinealDto { Pendiente = 0, Intercepto = 0, R2 = 0 },
                Historico = new List<PuntoRegresionDto>(),
                Proyeccion = new ProyeccionDto
                {
                    PeriodoSiguiente = DateTime.Now.AddMonths(1).ToString("yyyy-MM"),
                    Valor = 0,
                    NivelRiesgo = "Bajo"
                },
                PromedioDias = 0,
                Recomendacion = "No hay datos suficientes para realizar predicciones."
            };
        }

        // Calcular histórico por mes
        var porMes = solicitudes
            .GroupBy(s => new
            {
                Año = s.FechaSolicitud.Year,
                Mes = s.FechaSolicitud.Month
            })
            .Select(g => new
            {
                g.Key.Año,
                g.Key.Mes,
                Total = g.Count(),
                Cumple = g.Count(s => (s.FechaIngreso.Value - s.FechaSolicitud).Days <= tipoSla.TiempoRespuesta)
            })
            .OrderBy(x => x.Año)
            .ThenBy(x => x.Mes)
            .ToList();

        // Convertir a puntos X, Y para regresión
        var puntos = porMes.Select((m, index) => new
        {
            X = index + 1,
            Y = m.Total > 0 ? (double)m.Cumple / m.Total * 100 : 0
        }).ToList();

        if (puntos.Count < 2)
        {
            return new RegresionDto
            {
                TipoSla = codigoTipoSla,
                Regresion = new RegresionLinealDto { Pendiente = 0, Intercepto = 0, R2 = 0 },
                Historico = puntos.Select(p => new PuntoRegresionDto { X = p.X, Y = Math.Round(p.Y, 1) }).ToList(),
                Proyeccion = new ProyeccionDto
                {
                    PeriodoSiguiente = DateTime.Now.AddMonths(1).ToString("yyyy-MM"),
                    Valor = puntos.FirstOrDefault()?.Y ?? 0,
                    NivelRiesgo = "Medio"
                },
                PromedioDias = Math.Round(solicitudes.Average(s => (s.FechaIngreso.Value - s.FechaSolicitud).Days), 1),
                Recomendacion = "Datos insuficientes para regresión lineal precisa."
            };
        }

        // Calcular regresión lineal: y = mx + b
        var n = puntos.Count;
        var sumX = puntos.Sum(p => p.X);
        var sumY = puntos.Sum(p => p.Y);
        var sumXY = puntos.Sum(p => p.X * p.Y);
        var sumX2 = puntos.Sum(p => p.X * p.X);

        var pendiente = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercepto = (sumY - pendiente * sumX) / n;

        // Calcular R²
        var yMedia = sumY / n;
        var ssTot = puntos.Sum(p => Math.Pow(p.Y - yMedia, 2));
        var ssRes = puntos.Sum(p => Math.Pow(p.Y - (pendiente * p.X + intercepto), 2));
        var r2 = ssTot > 0 ? 1 - (ssRes / ssTot) : 0;

        // Proyección para el siguiente periodo
        var siguienteX = puntos.Count + 1;
        var valorProyectado = pendiente * siguienteX + intercepto;

        // Determinar nivel de riesgo
        string nivelRiesgo;
        if (valorProyectado >= 70) nivelRiesgo = "Bajo";
        else if (valorProyectado >= 50) nivelRiesgo = "Medio";
        else nivelRiesgo = "Alto";

        // Generar recomendación
        string recomendacion;
        if (pendiente < -5)
            recomendacion = "El cumplimiento está disminuyendo rápidamente. Revisar procesos urgentemente.";
        else if (pendiente < 0)
            recomendacion = "El cumplimiento está disminuyendo. Revisar procesos de contratación.";
        else if (pendiente > 5)
            recomendacion = "Excelente mejora en el cumplimiento. Continuar con las buenas prácticas.";
        else
            recomendacion = "El cumplimiento se mantiene estable. Monitorear periódicamente.";

        var ultimoPeriodo = porMes.Any() ? porMes.Last() : null;
        var proximoPeriodo = ultimoPeriodo != null
            ? new DateTime(ultimoPeriodo.Año, ultimoPeriodo.Mes, 1).AddMonths(1).ToString("yyyy-MM")
            : DateTime.Now.AddMonths(1).ToString("yyyy-MM");

        return new RegresionDto
        {
            TipoSla = codigoTipoSla,
            Regresion = new RegresionLinealDto
            {
                Pendiente = Math.Round(pendiente, 2),
                Intercepto = Math.Round(intercepto, 2),
                R2 = Math.Round(r2, 2)
            },
            Historico = puntos.Select(p => new PuntoRegresionDto
            {
                X = p.X,
                Y = Math.Round(p.Y, 1)
            }).ToList(),
            Proyeccion = new ProyeccionDto
            {
                PeriodoSiguiente = proximoPeriodo,
                Valor = Math.Round(valorProyectado, 1),
                NivelRiesgo = nivelRiesgo
            },
            PromedioDias = Math.Round(solicitudes.Average(s => (s.FechaIngreso.Value - s.FechaSolicitud).Days), 1),
            Recomendacion = recomendacion
        };
    }
}
