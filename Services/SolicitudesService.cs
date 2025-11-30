using DamslaApi.Data;
using DamslaApi.Dtos.Solicitudes;
using DamslaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class SolicitudesService
    {
        private readonly DamslaDbContext _db;

        public SolicitudesService(DamslaDbContext db)
        {
            _db = db;
        }

        // LISTAR
        public async Task<List<Solicitud>> GetAll()
        {
            return await _db.Solicitudes
                .Include(x => x.TipoSla)
                .Include(x => x.Usuario)
                .ToListAsync();
        }

        // OBTENER POR ID
        public async Task<Solicitud?> GetById(int id)
        {
            return await _db.Solicitudes
                .Include(x => x.TipoSla)
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // CREAR
        public async Task<Solicitud> Create(CreateSolicitudDto dto, int userId)
        {
            var nueva = new Solicitud
            {
                Rol = dto.Rol,
                FechaSolicitud = dto.FechaSolicitud,
                FechaIngreso = dto.FechaIngreso,
                TipoSlaId = dto.TipoSlaId,
                UsuarioId = userId,
                CreadoPor = userId
            };

            _db.Solicitudes.Add(nueva);
            await _db.SaveChangesAsync();
            
            // Recargar con relaciones
            return await GetById(nueva.Id) ?? nueva;
        }

        // EDITAR
        public async Task<bool> Update(int id, UpdateSolicitudDto dto)
        {
            var solicitud = await _db.Solicitudes.FindAsync(id);
            if (solicitud == null) return false;

            solicitud.Rol = dto.Rol;
            solicitud.FechaSolicitud = dto.FechaSolicitud;
            solicitud.FechaIngreso = dto.FechaIngreso;
            solicitud.TipoSlaId = dto.TipoSlaId;

            await _db.SaveChangesAsync();
            return true;
        }

        // BORRAR
        public async Task<bool> Delete(int id)
        {
            var solicitud = await _db.Solicitudes.FindAsync(id);
            if (solicitud == null) return false;

            _db.Solicitudes.Remove(solicitud);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}