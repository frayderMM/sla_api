using DamslaApi.Data;
using DamslaApi.Models;
using DamslaApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class ExcelService
    {
        private readonly DamslaDbContext _db;

        public ExcelService(DamslaDbContext db)
        {
            _db = db;
        }

        public async Task<List<string>> ImportarExcel(Stream fileStream, int userId)
        {
            var errores = new List<string>();
            var rows = ExcelParser.ReadExcel(fileStream);

            foreach (var row in rows)
            {
                try
                {
                    string rol = row["Rol"];
                    string tipoSla = row["TipoSla"];
                    DateTime fechaSolicitud = DateTime.Parse(row["FechaSolicitud"]);
                    DateTime? fechaIngreso = string.IsNullOrWhiteSpace(row["FechaIngreso"]) 
                        ? null 
                        : DateTime.Parse(row["FechaIngreso"]);

                    // Convertir a UTC para PostgreSQL
                    fechaSolicitud = DateTime.SpecifyKind(fechaSolicitud, DateTimeKind.Utc);
                    if (fechaIngreso.HasValue)
                    {
                        fechaIngreso = DateTime.SpecifyKind(fechaIngreso.Value, DateTimeKind.Utc);
                    }

                    // Validar Tipo SLA
                    var tipo = await _db.TiposSla.FirstOrDefaultAsync(t => t.Codigo == tipoSla);
                    if (tipo == null)
                    {
                        errores.Add($"Tipo SLA inválido en fila: {tipoSla}");
                        continue;
                    }

                    var solicitud = new Solicitud
                    {
                        Rol = rol,
                        FechaSolicitud = fechaSolicitud,
                        FechaIngreso = fechaIngreso,
                        TipoSlaId = tipo.Id,
                        UsuarioId = userId,
                        CreadoPor = userId
                    };

                    _db.Solicitudes.Add(solicitud);
                }
                catch (Exception ex)
                {
                    errores.Add($"Error procesando fila: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();
            return errores;
        }
    }
}