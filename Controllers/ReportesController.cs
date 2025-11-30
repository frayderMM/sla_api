using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly PdfService _pdfService;

        public ReportesController(PdfService pdfService)
        {
            _pdfService = pdfService;
        }

        // Solo usuarios con rol general o analista pueden ver reportes
        [Authorize]
        [HttpGet("pdf")]
        public async Task<IActionResult> GenerarPdf()
        {
            var archivo = await _pdfService.GenerarReporteCompleto();

            return File(
                archivo,
                "application/pdf",
                "Reporte_SLA.pdf"
            );
        }
    }
}
