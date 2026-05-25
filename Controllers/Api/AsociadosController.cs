using FondoXYZ.Models.DTOs;
using FondoXYZ.Services;
using Microsoft.AspNetCore.Mvc;

namespace FondoXYZ.Controllers.Api;

[ApiController]
[Route("api/asociados")]
public class AsociadosController : ControllerBase
{
    private readonly IAsociadoService _asociadoService;

    public AsociadosController(IAsociadoService asociadoService)
    {
        _asociadoService = asociadoService;
    }

    /// <summary>
    /// Lista los asociados registrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] bool incluirInactivos = false,
        CancellationToken cancellationToken = default)
    {
        var asociados = await _asociadoService.ListarAsociadosAsync(incluirInactivos, cancellationToken);
        return Ok(asociados);
    }

    /// <summary>
    /// Obtiene un asociado por su identificador.
    /// </summary>
    [HttpGet("{asociadoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerPorId(int asociadoId, CancellationToken cancellationToken)
    {
        try
        {
            var asociado = await _asociadoService.ObtenerAsociadoPorIdAsync(asociadoId, cancellationToken);

            if (asociado is null)
            {
                return NotFound(new { mensaje = $"No se encontró el asociado con id {asociadoId}." });
            }

            return Ok(asociado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Crea un asociado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearAsociadoRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var asociado = await _asociadoService.CrearAsociadoAsync(request, cancellationToken);
            return Created($"/api/asociados/{asociado.AsociadoId}", asociado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un asociado.
    /// </summary>
    [HttpPut("{asociadoId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar(
        int asociadoId,
        [FromBody] ActualizarAsociadoRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la solicitud en JSON es obligatorio." });
        }

        try
        {
            var asociado = await _asociadoService.ActualizarAsociadoAsync(
                asociadoId,
                request,
                cancellationToken);

            if (asociado is null)
            {
                return NotFound(new { mensaje = $"No se encontró el asociado con id {asociadoId}." });
            }

            return Ok(asociado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Inactiva un asociado.
    /// </summary>
    [HttpDelete("{asociadoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Eliminar(int asociadoId, CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await _asociadoService.EliminarAsociadoAsync(asociadoId, cancellationToken);

            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontró un asociado activo con id {asociadoId}." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
