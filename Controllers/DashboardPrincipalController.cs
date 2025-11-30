using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DamslaApi.Services;
using DamslaApi.Dtos.DashboardPrincipal;

namespace DamslaApi.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardPrincipalController : ControllerBase
{
    private readonly DashboardPrincipalService _service;

    public DashboardPrincipalController(DashboardPrincipalService service)
    {
        _service = service;
    }

    /// <summary>
    /// ENDPOINT 1 - Indicadores principales (Cumple / No Cumple)
    /// </summary>
    [HttpGet("indicadores")]
    public async Task<ActionResult<IndicadoresDto>> GetIndicadores([FromQuery] string tipoSla)
    {
        if (string.IsNullOrWhiteSpace(tipoSla))
            return BadRequest(new { mensaje = "El parámetro 'tipoSla' es obligatorio" });

        var resultado = await _service.ObtenerIndicadores(tipoSla);
        
        if (resultado == null)
            return NotFound(new { mensaje = $"No se encontró el tipo SLA '{tipoSla}'" });

        return Ok(resultado);
    }

    /// <summary>
    /// ENDPOINT 2 - Indicadores históricos para gráfica de tendencia
    /// </summary>
    [HttpGet("historico")]
    public async Task<ActionResult<HistoricoDto>> GetHistorico([FromQuery] string tipoSla)
    {
        if (string.IsNullOrWhiteSpace(tipoSla))
            return BadRequest(new { mensaje = "El parámetro 'tipoSla' es obligatorio" });

        var resultado = await _service.ObtenerHistorico(tipoSla);
        
        if (resultado == null)
            return NotFound(new { mensaje = $"No se encontró el tipo SLA '{tipoSla}'" });

        return Ok(resultado);
    }

    /// <summary>
    /// ENDPOINT 3 - Datos completos de regresión lineal con proyección
    /// </summary>
    [HttpGet("regresion")]
    public async Task<ActionResult<RegresionDto>> GetRegresion([FromQuery] string tipoSla)
    {
        if (string.IsNullOrWhiteSpace(tipoSla))
            return BadRequest(new { mensaje = "El parámetro 'tipoSla' es obligatorio" });

        var resultado = await _service.ObtenerRegresionLineal(tipoSla);
        
        if (resultado == null)
            return NotFound(new { mensaje = $"No se encontró el tipo SLA '{tipoSla}'" });

        return Ok(resultado);
    }
}
