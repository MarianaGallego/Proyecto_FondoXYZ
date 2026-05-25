using FondoXYZ.Services;
using Microsoft.AspNetCore.Mvc;

namespace FondoXYZ.Controllers.Api;

[ApiController]
[Route("api/sitios")]
public class SitiosController : ControllerBase
{
    private readonly ISitioService _sitioService;

    public SitiosController(ISitioService sitioService)
    {
        _sitioService = sitioService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var sitios = await _sitioService.ListarSitiosAsync(cancellationToken);
        return Ok(sitios);
    }

    [HttpGet("{sitioId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int sitioId, CancellationToken cancellationToken)
    {
        var sitio = await _sitioService.ObtenerSitioPorIdAsync(sitioId, cancellationToken);

        if (sitio is null)
        {
            return NotFound(new { mensaje = $"No se encontró el sitio con id {sitioId}." });
        }

        return Ok(sitio);
    }
}
