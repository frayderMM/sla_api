using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposSlaController : ControllerBase
    {
        private readonly DamslaDbContext _db;

        public TiposSlaController(DamslaDbContext db)
        {
            _db = db;
        }

        // GET: api/tiposSla
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _db.TiposSla
                .Select(t => new {
                    t.Id,
                    t.Codigo,
                    t.Descripcion,
                    t.Nombre,
                    t.TiempoRespuesta
                })
                .ToListAsync();

            return Ok(tipos);
        }
    }
}
