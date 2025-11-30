using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertasController : ControllerBase
    {
        private readonly AlertasService _alertas;

        public AlertasController(AlertasService alertas)
        {
            _alertas = alertas;
        }

        // Ver alertas recientes
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAlertas()
        {
            var data = await _alertas.ObtenerAlertas();
            return Ok(data);
        }

        // Forzar ejecución manual (solo analista)
        [Authorize(Roles = "analista")]
        [HttpPost("procesar")]
        public async Task<IActionResult> Procesar()
        {
            await _alertas.GenerarAlertas();
            return Ok(new { message = "Alertas procesadas manualmente" });
        }

        // Marcar alerta como atendida (solo analista)
        [Authorize(Roles = "analista")]
        [HttpPut("{id}/atender")]
        public async Task<IActionResult> AtenderAlerta(int id)
        {
            var resultado = await _alertas.MarcarComoAtendida(id);
            
            if (!resultado)
                return NotFound(new { message = "Alerta no encontrada" });

            return Ok(new { message = "Alerta marcada como atendida" });
        }
    }
}
