using Microsoft.AspNetCore.Mvc;
using DamslaApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlaController : ControllerBase
    {
        private readonly SlaService _sla;

        public SlaController(SlaService sla)
        {
            _sla = sla;
        }

        // ------------------------------------------------------
        // GET: api/sla/indicadores
        // Lista completa de solicitudes con resultado SLA
        // ------------------------------------------------------
        [Authorize] // general + analista
        [HttpGet("indicadores")]
        public async Task<IActionResult> GetIndicadores()
        {
            var data = await _sla.GetIndicadores();
            return Ok(data);
        }

        // ------------------------------------------------------
        // GET: api/sla/resumen
        // KPI por rol (para dashboard)
        // ------------------------------------------------------
        [Authorize] // general + analista
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            var resumen = await _sla.GetResumenPorRol();
            return Ok(resumen);
        }

        // ------------------------------------------------------
        // GET: api/sla/metrics_glob
        // Métricas globales (total, cumplen, no cumplen, porcentajes)
        // ------------------------------------------------------
        [Authorize]
        [HttpGet("metrics_glob")]
        public async Task<IActionResult> GetMetricsGlob()
        {
            var metrics = await _sla.GetMetricsGlob();
            return Ok(metrics);
        }

        // ------------------------------------------------------
        // GET: api/sla/metricsxsla
        // Métricas por tipo de SLA
        // ------------------------------------------------------
        [Authorize]
        [HttpGet("metricsxsla")]
        public async Task<IActionResult> GetMetricsByTipo()
        {
            var metrics = await _sla.GetMetricsByTipo();
            return Ok(metrics);
        }
    }
}