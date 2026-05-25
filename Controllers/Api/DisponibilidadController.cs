using FondoXYZ.Services;
using Microsoft.AspNetCore.Mvc;

namespace FondoXYZ.Controllers.Api;

[ApiController]
[Route("api/disponibilidad")]
public class DisponibilidadController : ControllerBase
{
    private readonly IDisponibilidadService _disponibilidadService;

    public DisponibilidadController(IDisponibilidadService disponibilidadService)
    {
        _disponibilidadService = disponibilidadService;
    }

    /// <summary>
    /// Consulta unidades de alojamiento disponibles en un rango de fechas.
    /// Excluye unidades ocupadas por reservas activas y bloqueos de disponibilidad.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Consultar(
        [FromQuery] DateOnly fechaEntrada,
        [FromQuery] DateOnly fechaSalida,
        [FromQuery] int? sitioId,
        [FromQuery] int? numeroPersonas,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _disponibilidadService.ConsultarDisponibilidadAsync(
                fechaEntrada,
                fechaSalida,
                sitioId,
                numeroPersonas,
                cancellationToken);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Consulta si una unidad de alojamiento específica está disponible en un rango de fechas.
    /// </summary>
    [HttpGet("alojamientos/{unidadAlojamientoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarAlojamiento(
        int unidadAlojamientoId,
        [FromQuery] DateOnly fechaEntrada,
        [FromQuery] DateOnly fechaSalida,
        [FromQuery] int? numeroPersonas,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _disponibilidadService.ConsultarDisponibilidadUnidadAsync(
                unidadAlojamientoId,
                fechaEntrada,
                fechaSalida,
                numeroPersonas,
                cancellationToken);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
