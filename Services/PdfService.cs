using DamslaApi.Data;
using DamslaApi.Dtos.Sla;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DamslaApi.Services.PdfTemplates;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DamslaApi.Services
{
    public class PdfService
    {
        private readonly DamslaDbContext _db;
        private readonly SlaService _sla;

        public PdfService(DamslaDbContext db, SlaService sla)
        {
            _db = db;
            _sla = sla;
        }

        public async Task<byte[]> GenerarReporteCompleto()
        {
            var resumen = await _sla.GetResumenPorRol();
            var indicadores = await _sla.GetIndicadores();

            var ms = new MemoryStream();

            // Configure QuestPDF license mode (Community is free for development)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = new SlaReportDocument(resumen, indicadores);
            document.GeneratePdf(ms);
            ms.Position = 0;
            return ms.ToArray();
        }
    }
}
