using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;
using DamslaApi.Dtos.TiposSla;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposSlaController : ControllerBase
    {
        private readonly TiposSlaService _service;

        public TiposSlaController(TiposSlaService service)
        {
            _service = service;
        }

        // -----------------------------------------------------------
        // GET: api/tiposSla  (TODOS)
        // -----------------------------------------------------------
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _service.GetAll();
            return Ok(tipos);
        }

        // -----------------------------------------------------------
        // GET: api/tiposSla/{id}  (TODOS)
        // -----------------------------------------------------------
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tipo = await _service.GetById(id);
            if (tipo == null) return NotFound(new { message = "Tipo SLA no encontrado" });

            return Ok(tipo);
        }

        // -----------------------------------------------------------
        // GET: api/tiposSla/codigo/{codigo}  (TODOS)
        // -----------------------------------------------------------
        [Authorize]
        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var tipo = await _service.GetByCodigo(codigo);
            if (tipo == null) return NotFound(new { message = $"Tipo SLA '{codigo}' no encontrado" });

            return Ok(tipo);
        }

        // -----------------------------------------------------------
        // POST: api/tiposSla  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTipoSlaDto dto)
        {
            try
            {
                var created = await _service.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -----------------------------------------------------------
        // PUT: api/tiposSla/{id}  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTipoSlaDto dto)
        {
            try
            {
                var ok = await _service.Update(id, dto);
                if (!ok) return NotFound(new { message = "Tipo SLA no encontrado" });

                return Ok(new { message = "Tipo SLA actualizado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -----------------------------------------------------------
        // DELETE: api/tiposSla/{id}  (SOLO ANALISTA)
        // -----------------------------------------------------------
        [Authorize(Roles = "analista")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.Delete(id);
                if (!ok) return NotFound(new { message = "Tipo SLA no encontrado" });

                return Ok(new { message = "Tipo SLA eliminado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
