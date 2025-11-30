using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly DamslaDbContext _db;

        public LogsController(DamslaDbContext db)
        {
            _db = db;
        }

        // Solo rol "analista" puede ver auditoría
        [Authorize(Roles = "analista")]
        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _db.LogAcceso
                .OrderByDescending(l => l.Fecha)
                .Take(200)
                .ToListAsync();

            return Ok(logs);
        }

        [Authorize(Roles = "analista")]
        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> GetPorUsuario(int id)
        {
            var logs = await _db.LogAcceso
                .Where(l => l.UsuarioId == id)
                .OrderByDescending(l => l.Fecha)
                .ToListAsync();

            return Ok(logs);
        }
    }
}
