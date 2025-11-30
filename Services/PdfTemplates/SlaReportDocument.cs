using DamslaApi.Dtos.Sla;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace DamslaApi.Services.PdfTemplates
{
    public class SlaReportDocument : IDocument
    {
        private readonly List<SlaResumenDto> _resumen;
        private readonly List<IndicadorSlaDto> _indicadores;

        // Colores del estilo basado en tu Excel
        private readonly string AzulHeader = "#014F86";
        private readonly string CelesteFondo = "#E7F5FF";
        private readonly string VerdeOk = "#2ECC71";
        private readonly string RojoBad = "#E63946";
        private readonly string GrisNa = "#B0B0B0";

        public SlaReportDocument(List<SlaResumenDto> resumen, List<IndicadorSlaDto> indicadores)
        {
            _resumen = resumen ?? new List<SlaResumenDto>();
            _indicadores = indicadores ?? new List<IndicadorSlaDto>();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // HEADER
                page.Header().Element(ComposeHeader);

                // CONTENIDO
                page.Content().Element(contentContainer => contentContainer.PaddingVertical(10).Column(content =>
                {
                    // Avoid nested Element usage: pad the item and call the section directly
                    content.Item().PaddingBottom(15).Element(ComposeKpiSection);

                    content.Item().Element(ComposeDetailsSection);
                }));

                // FOOTER (simplified to avoid nested handlers)
                page.Footer().AlignCenter().Text($"DAM SLA – {DateTime.Now:yyyy-MM-dd}").SemiBold().FontSize(9);
            });
        }

        // ====================================================================================
        // HEADER ESTILO EXCEL — BARRA AZUL
        // ====================================================================================
        void ComposeHeader(IContainer header)
        {
            // Use a single chained call to avoid assigning multiple child elements to the same container
            header.Background(Colors.Blue.Darken2).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Reporte Indicadores SLA").FontColor(Colors.White).FontSize(20).SemiBold();
                });

                row.ConstantItem(120).Column(col =>
                {
                    col.Item().AlignRight().Text("Confidencial").FontColor(Colors.Grey.Lighten2).FontSize(10);
                });
            });
        }

        // ====================================================================================
        // TABLA DE KPIs — MISMO ESTILO DE TU IMAGEN
        // ====================================================================================
        void ComposeKpiSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(6).Text("KPIs Generales por Rol").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Rol
                        columns.RelativeColumn();  // Total
                        columns.RelativeColumn();  // Cumplen
                        columns.RelativeColumn();  // No Cumplen
                        columns.RelativeColumn();  // Porcentaje
                        columns.RelativeColumn();  // Semáforo
                    });

                    // HEADER
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Rol").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("Total").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("Cumplen").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("No Cumplen").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("% SLA").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter().Text("Indicador").FontColor(Colors.White).SemiBold();
                    });

                    foreach (var r in _resumen)
                    {
                        // Texto y color de 'No Cumplen'
                        var noCumplenText = r.NoCumplen.ToString();
                        // Si hay no cumplen, usar rojo; si no hay, usar verde (y gris si no hay solicitudes)
                        var noCumplenColor = r.NoCumplen > 0 ? Colors.Red.Medium : Colors.Green.Medium;

                        // Semáforo: seguir el mismo criterio que la columna 'No Cumplen'
                        var semColor = r.TotalSolicitudes == 0 ? Colors.Grey.Medium : (r.NoCumplen > 0 ? Colors.Red.Medium : Colors.Green.Medium);

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(r.Rol ?? string.Empty);
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(r.TotalSolicitudes.ToString());
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(r.Cumplen.ToString());

                        // 'No Cumplen' con color condicional
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(noCumplenText).FontColor(noCumplenColor);

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text($"{r.PorcentajeCumplimiento:0.##}%");

                        // Punto indicador: se pinta usando el mismo color lógico (semColor)
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(t => t.Span("●").FontSize(12).FontColor(semColor));
                    }
                });
            });
        }

        // ====================================================================================
        // TABLA DETALLE COMPLETO — MISMO ESTILO DE EXCEL
        // ====================================================================================
        void ComposeDetailsSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(6).Text("Detalle Completo de Solicitudes SLA").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();   // ID
                        cols.RelativeColumn(2); // Rol
                        cols.RelativeColumn();   // Tipo SLA
                        cols.RelativeColumn();   // Fecha Solicitud
                        cols.RelativeColumn();   // Fecha Ingreso
                        cols.RelativeColumn();   // Resultado
                    });

                    // HEADER
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("ID").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Rol").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Tipo SLA").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Fecha Solicitud").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Fecha Ingreso").FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Resultado").FontColor(Colors.White).SemiBold();
                    });

                    foreach (var s in _indicadores)
                    {
                        // Determinar color del resultado: rojo si explícitamente indica no cumplimiento
                        var resultadoColor = Colors.Black;
                        if (!string.IsNullOrWhiteSpace(s.Resultado))
                        {
                            var res = s.Resultado.Trim().ToLowerInvariant();
                            // Considerar varios formatos que indican no cumplimiento
                            if (res.StartsWith("no") || res.Contains("no cumple") || res.Contains("no-cumple") || res.Contains("no_cumple") || res.Contains("no cumple"))
                                resultadoColor = Colors.Red.Medium;
                            else
                                resultadoColor = Colors.Green.Medium;
                        }

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(s.Id.ToString());
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(s.Rol ?? string.Empty);
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(s.TipoSla ?? string.Empty);
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(s.FechaSolicitud.ToString("yyyy-MM-dd"));
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).AlignCenter().Text(s.FechaIngreso.ToString("yyyy-MM-dd"));

                        // Aplicar color al texto del resultado usando Text lambda
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(t => t.Span(s.Resultado ?? string.Empty).SemiBold().FontColor(resultadoColor));
                    }
                });
            });
        }
    }
}
