using FondoXYZ.Models.DTOs;
using FondoXYZ.Services;
using Microsoft.AspNetCore.Mvc;

namespace FondoXYZ.Controllers.Api;

[ApiController]
[Route("api/tarifas")]
public class TarifasController : ControllerBase
{
    private readonly ITarifaService _tarifaService;

    public TarifasController(ITarifaService tarifaService)
    {
        _tarifaService = tarifaService;
    }

    /// <summary>
    /// Consulta tarifas vigentes según sitio, unidad de alojamiento, temporada y número de personas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Consultar(
        [FromBody] TarifaConsultaRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var resultado = await _tarifaService.ConsultarTarifasAsync(
                request.SitioId,
                request.UnidadAlojamientoId,
                request.TemporadaId,
                request.NumeroPersonas,
                cancellationToken);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Calcula el total a cancelar por noches, unidades, personas y temporada.
    /// Incluye tarifa especial lun-jue y personas adicionales cuando aplican.
    /// </summary>
    [HttpPost("calcular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Calcular(
        [FromBody] CalcularTarifaRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var resultado = await _tarifaService.CalcularTarifaAsync(request, cancellationToken);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
