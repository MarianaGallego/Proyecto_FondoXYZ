using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface ISitioService
{
    Task<IReadOnlyList<SitioListadoDto>> ListarSitiosAsync(CancellationToken cancellationToken = default);

    Task<SitioDetalleDto?> ObtenerSitioPorIdAsync(int sitioId, CancellationToken cancellationToken = default);
}
