using DamslaApi.Data;
using DamslaApi.Dtos.TiposSla;
using DamslaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class TiposSlaService
    {
        private readonly DamslaDbContext _db;

        public TiposSlaService(DamslaDbContext db)
        {
            _db = db;
        }

        // LISTAR TODOS
        public async Task<List<TipoSla>> GetAll()
        {
            return await _db.TiposSla
                .OrderBy(t => t.Codigo)
                .ToListAsync();
        }

        // OBTENER POR ID
        public async Task<TipoSla?> GetById(int id)
        {
            return await _db.TiposSla.FindAsync(id);
        }

        // OBTENER POR CODIGO
        public async Task<TipoSla?> GetByCodigo(string codigo)
        {
            return await _db.TiposSla
                .FirstOrDefaultAsync(t => t.Codigo == codigo);
        }

        // CREAR
        public async Task<TipoSla> Create(CreateTipoSlaDto dto)
        {
            // Validar que el código no exista
            var existe = await _db.TiposSla.AnyAsync(t => t.Codigo == dto.Codigo);
            if (existe)
            {
                throw new InvalidOperationException($"Ya existe un Tipo SLA con código '{dto.Codigo}'");
            }

            var nuevo = new TipoSla
            {
                Codigo = dto.Codigo,
                Descripcion = dto.Descripcion,
                TiempoRespuesta = dto.TiempoRespuesta
            };

            _db.TiposSla.Add(nuevo);
            await _db.SaveChangesAsync();

            return nuevo;
        }

        // ACTUALIZAR
        public async Task<bool> Update(int id, UpdateTipoSlaDto dto)
        {
            var tipoSla = await _db.TiposSla.FindAsync(id);
            if (tipoSla == null) return false;

            // Validar que el código no esté duplicado
            var existe = await _db.TiposSla
                .AnyAsync(t => t.Codigo == dto.Codigo && t.Id != id);
            if (existe)
            {
                throw new InvalidOperationException($"Ya existe un Tipo SLA con código '{dto.Codigo}'");
            }

            tipoSla.Codigo = dto.Codigo;
            tipoSla.Descripcion = dto.Descripcion;
            tipoSla.TiempoRespuesta = dto.TiempoRespuesta;

            await _db.SaveChangesAsync();
            return true;
        }

        // ELIMINAR
        public async Task<bool> Delete(int id)
        {
            var tipoSla = await _db.TiposSla.FindAsync(id);
            if (tipoSla == null) return false;

            // Validar que no tenga solicitudes asociadas
            var tieneSolicitudes = await _db.Solicitudes.AnyAsync(s => s.TipoSlaId == id);
            if (tieneSolicitudes)
            {
                throw new InvalidOperationException("No se puede eliminar un Tipo SLA que tiene solicitudes asociadas");
            }

            _db.TiposSla.Remove(tipoSla);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
