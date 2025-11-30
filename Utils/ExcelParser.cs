using OfficeOpenXml;

namespace DamslaApi.Utils
{
    public class ExcelParser
    {
        public static List<Dictionary<string, string>> ReadExcel(Stream stream)
        {
            // Configurar licencia para EPPlus (no comercial)
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var result = new List<Dictionary<string, string>>();

            int rows = worksheet.Dimension.Rows;
            int cols = worksheet.Dimension.Columns;

            // Leer encabezados
            var headers = new List<string>();
            for (int c = 1; c <= cols; c++)
                headers.Add(worksheet.Cells[1, c].Text.Trim());

            // Leer filas
            for (int r = 2; r <= rows; r++)
            {
                var row = new Dictionary<string, string>();

                for (int c = 1; c <= cols; c++)
                    row[headers[c - 1]] = worksheet.Cells[r, c].Text.Trim();

                result.Add(row);
            }

            return result;
        }
    }
}