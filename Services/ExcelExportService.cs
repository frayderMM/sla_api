using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using DamslaApi.Data;
using System.Drawing;

namespace DamslaApi.Services
{
    public class ExcelExportService
    {
        private readonly SlaService _sla;
        private readonly DashboardService _dashboard;
        private readonly DamslaDbContext _db;

        public ExcelExportService(SlaService sla, DashboardService dashboard, DamslaDbContext db)
        {
            _sla = sla;
            _dashboard = dashboard;
            _db = db;
        }

        public async Task<byte[]> ExportarExcelCompleto(int year)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // ================================================================
            // HOJA 1: KPIs por Rol
            // ================================================================
            var wsKpi = package.Workbook.Worksheets.Add("KPIs");

            // Logos (si existen)
            try
            {
                var esanLogoPath = new FileInfo("wwwroot/logos/esan.png");
                var tcsLogoPath = new FileInfo("wwwroot/logos/tcs.png");

                if (esanLogoPath.Exists)
                {
                    var pic1 = wsKpi.Drawings.AddPicture("ESAN", esanLogoPath);
                    pic1.SetPosition(0, 0, 0, 0);
                    pic1.SetSize(100, 100);
                }

                if (tcsLogoPath.Exists)
                {
                    var pic2 = wsKpi.Drawings.AddPicture("TCS", tcsLogoPath);
                    pic2.SetPosition(0, 0, 3, 0);
                    pic2.SetSize(100, 100);
                }
            }
            catch { /* Si falta el logo, no cae el proceso */ }

            // Título
            wsKpi.Cells["A6"].Value = $"KPIs de SLA – {year}";
            wsKpi.Cells["A6"].Style.Font.Bold = true;
            wsKpi.Cells["A6"].Style.Font.Size = 16;

            // Encabezados
            wsKpi.Cells["A8"].Value = "Rol";
            wsKpi.Cells["B8"].Value = "Total";
            wsKpi.Cells["C8"].Value = "Cumplen";
            wsKpi.Cells["D8"].Value = "No Cumplen";
            wsKpi.Cells["E8"].Value = "% Cumplimiento";

            using (var range = wsKpi.Cells["A8:E8"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            var resumen = await _dashboard.GetDashboardMensual(year);

            int row = 9;
            foreach (var r in resumen)
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
