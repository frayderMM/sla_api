using Microsoft.AspNetCore.Mvc;
using DamslaApi.Services;
using DamslaApi.Dtos.Solicitudes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudesController : ControllerBase
    {
        private readonly SolicitudesService _service;

        public SolicitudesController(SolicitudesService service)
        {
            _service = service;
        }

        // -----------------------------------------------------------
        // GET: api/solicitudes  (GENERAL + ANALISTA)
        // -----------------------------------------------------------
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            return Ok(data);
        }

        // -----------------------------------------------------------
        // GET: api/solicitudes/{id} (GENERAL + ANALISTA)
        // -----------------------------------------------------------
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var solicitud = await _service.GetById(id);
            if (solicitud == null) return NotFound();

            return Ok(solicitud);
        }

        // -----------------------------------------------------------
        // POST: api/solicitudes  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSolicitudDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var created = await _service.Create(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // -----------------------------------------------------------
        // PUT: api/solicitudes/{id}  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSolicitudDto dto)
        {
            var ok = await _service.Update(id, dto);
            if (!ok) return NotFound();

            return Ok(new { message = "Solicitud actualizada correctamente" });
        }

        // -----------------------------------------------------------
        // DELETE: api/solicitudes/{id}  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.Delete(id);
            if (!ok) return NotFound();

            return Ok(new { message = "Solicitud eliminada correctamente" });
        }
    }
}