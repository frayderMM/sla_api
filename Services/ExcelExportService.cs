using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using DamslaApi.Data;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class ExcelExportService
    {
        private readonly SlaService _sla;
        private readonly DashboardService _dashboard;
        private readonly DamslaDbContext _db;

        static ExcelExportService()
        {
            // Configurar licencia para EPPlus 8+ (no comercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public ExcelExportService(SlaService sla, DashboardService dashboard, DamslaDbContext db)
        {
            _sla = sla;
            _dashboard = dashboard;
            _db = db;
        }

        public async Task<byte[]> ExportarExcelCompleto(int year)
        {
            using var package = new ExcelPackage();

            // ================================================================
            // HOJA 1: KPIs por Rol - TODOS los roles de TODAS las fechas
            // ================================================================
            var wsKpi = package.Workbook.Worksheets.Add("KPIs");

            // Encabezados en A1
            wsKpi.Cells["A1"].Value = "Rol";
            wsKpi.Cells["B1"].Value = "Total";
            wsKpi.Cells["C1"].Value = "Cumplen";
            wsKpi.Cells["D1"].Value = "No Cumplen";
            wsKpi.Cells["E1"].Value = "% Cumplimiento";

            using (var range = wsKpi.Cells["A1:E1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Consulta DIRECTA a la BD sin filtros de año
            var todasSolicitudes = await _db.Solicitudes
                .Include(s => s.TipoSla)
                .Where(s => s.FechaIngreso.HasValue) // Solo las que tienen fecha de ingreso
                .ToListAsync();

            var resumenPorRol = todasSolicitudes
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Rol) ? "(Sin Rol)" : x.Rol)
                .Select(g => new
                {
                    Rol = g.Key,
                    Total = g.Count(),
                    Cumplen = g.Count(x => {
                        var dias = (x.FechaIngreso!.Value - x.FechaSolicitud).Days;
                        return dias <= x.TipoSla.TiempoRespuesta;
                    }),
                    NoCumplen = g.Count(x => {
                        var dias = (x.FechaIngreso!.Value - x.FechaSolicitud).Days;
                        return dias > x.TipoSla.TiempoRespuesta;
                    }),
                    Porcentaje = g.Count() > 0 ? Math.Round((double)g.Count(x => {
                        var dias = (x.FechaIngreso!.Value - x.FechaSolicitud).Days;
                        return dias <= x.TipoSla.TiempoRespuesta;
                    }) / g.Count() * 100, 2) : 0
                })
                .OrderBy(x => x.Rol)
                .ToList();

            int row = 2;
            foreach (var r in resumenPorRol)
            {
                wsKpi.Cells[row, 1].Value = r.Rol;
                wsKpi.Cells[row, 2].Value = r.Total;
                wsKpi.Cells[row, 3].Value = r.Cumplen;
                wsKpi.Cells[row, 4].Value = r.NoCumplen;
                wsKpi.Cells[row, 5].Value = r.Porcentaje;

                row++;
            }

            wsKpi.Cells.AutoFitColumns();

            // ================================================================
            // HOJA 2: Solicitudes SLA (detalle completo)
            // ================================================================
            var wsDetalle = package.Workbook.Worksheets.Add("Solicitudes");

            wsDetalle.Cells["A1"].Value = "ID";
            wsDetalle.Cells["B1"].Value = "Rol";
            wsDetalle.Cells["C1"].Value = "Tipo SLA";
            wsDetalle.Cells["D1"].Value = "Fecha Solicitud";
            wsDetalle.Cells["E1"].Value = "Fecha Ingreso";
            wsDetalle.Cells["F1"].Value = "Dias";
            wsDetalle.Cells["G1"].Value = "Resultado";

            using (var range = wsDetalle.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            var indicadores = await _sla.GetIndicadores();
            int row2 = 2;

            foreach (var s in indicadores)
            {
                wsDetalle.Cells[row2, 1].Value = s.Id;
                wsDetalle.Cells[row2, 2].Value = s.Rol;
                wsDetalle.Cells[row2, 3].Value = s.TipoSla;
                wsDetalle.Cells[row2, 4].Value = s.FechaSolicitud.ToString("yyyy-MM-dd");
                wsDetalle.Cells[row2, 5].Value = s.FechaIngreso.ToString("yyyy-MM-dd");
                wsDetalle.Cells[row2, 6].Value = s.Dias;
                wsDetalle.Cells[row2, 7].Value = s.Resultado;

                row2++;
            }

            wsDetalle.Cells.AutoFitColumns();

            // ================================================================
            // EXPORTAR
            // ================================================================
            return package.GetAsByteArray();
        }
    }
}
