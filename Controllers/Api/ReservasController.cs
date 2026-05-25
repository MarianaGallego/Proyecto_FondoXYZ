using FondoXYZ.Models.DTOs;
using FondoXYZ.Services;
using Microsoft.AspNetCore.Mvc;

namespace FondoXYZ.Controllers.Api;

[ApiController]
[Route("api/reservas")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly IPagoService _pagoService;

    public ReservasController(IReservaService reservaService, IPagoService pagoService)
    {
        _reservaService = reservaService;
        _pagoService = pagoService;
    }

    /// <summary>
    /// Crea una reserva de alojamiento o visita de día con unidades, acompañantes y servicios adicionales.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearReservaRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var reserva = await _reservaService.CrearReservaAsync(request, cancellationToken);
            return Created($"/api/reservas/{reserva.ReservaId}", reserva);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Inicia o registra el pago en línea de una reserva.
    /// Un pago aprobado confirma la reserva.
    /// </summary>
    [HttpPost("{reservaId:int}/pagos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPago(
        int reservaId,
        [FromBody] RegistrarPagoRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var pago = await _pagoService.RegistrarPagoAsync(reservaId, request, cancellationToken);
            return Created($"/api/reservas/{reservaId}/pagos/{pago.PagoId}", pago);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
