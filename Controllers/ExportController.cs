using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ExcelExportService _export;

        public ExportController(ExcelExportService export)
        {
            _export = export;
        }

        // general + analista pueden exportar
        [Authorize]
        [HttpGet("excel")]
        public async Task<IActionResult> ExportarExcelActual()
        {
            var year = DateTime.Now.Year;
            var archivo = await _export.ExportarExcelCompleto(year);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_SLA_{year}.xlsx"
            );
        }

        // general + analista pueden exportar (con año específico)
        [Authorize]
        [HttpGet("excel/{year}")]
        public async Task<IActionResult> ExportarExcel(int year)
        {
            var archivo = await _export.ExportarExcelCompleto(year);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_SLA_{year}.xlsx"
            );
        }
    }
}
