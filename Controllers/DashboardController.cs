using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;
using DamslaApi.Dtos.Dashboard;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboard;
        private readonly PrediccionService _prediccion;

        public DashboardController(DashboardService dashboard, PrediccionService prediccion)
        {
            _dashboard = dashboard;
            _prediccion = prediccion;
        }

        // -------------------------------------------------------
        // GET: Dashboard mensual por Rol
        // -------------------------------------------------------
        [Authorize]
        [HttpGet("mensual/{year}")]
        public async Task<IActionResult> Mensual(int year)
        {
            var data = await _dashboard.GetDashboardMensual(year);
            return Ok(data);
        }

        // -------------------------------------------------------
        // GET: Predicción por Rol
        // -------------------------------------------------------
        [Authorize]
        [HttpGet("prediccion/{year}/{rol}")]
        public async Task<IActionResult> Prediccion(int year, string rol)
        {
            var data = await _dashboard.GetDashboardMensual(year);
            var filtrado = data.Where(x => x.Rol == rol);

            if (!filtrado.Any())
                return NotFound("No hay datos para ese rol");

            var pred = _prediccion.Predecir(filtrado);
            return Ok(pred);
        }
    }
}
