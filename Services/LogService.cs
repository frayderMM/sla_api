using DamslaApi.Data;
using DamslaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class LogService
    {
        private readonly DamslaDbContext _db;

        public LogService(DamslaDbContext db)
        {
            _db = db;
        }

        public async Task RegistrarLog(
            int? usuarioId,
            string metodo,
            string endpoint,
            string accion,
            string ip,
            string userAgent)
        {
            var log = new LogAcceso
            {
                UsuarioId = usuarioId,
                Metodo = metodo,
                Endpoint = endpoint,
                Accion = accion,
                Ip = ip,
                UserAgent = userAgent,
                Fecha = DateTime.UtcNow
            };

            _db.LogAcceso.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
