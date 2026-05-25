using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface IAsociadoService
{
    Task<IReadOnlyList<AsociadoDto>> ListarAsociadosAsync(
        bool incluirInactivos = false,
        CancellationToken cancellationToken = default);

    Task<AsociadoDto?> ObtenerAsociadoPorIdAsync(
        int asociadoId,
        CancellationToken cancellationToken = default);

    Task<AsociadoDto> CrearAsociadoAsync(
        CrearAsociadoRequest request,
        CancellationToken cancellationToken = default);

    Task<AsociadoDto?> ActualizarAsociadoAsync(
        int asociadoId,
        ActualizarAsociadoRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> EliminarAsociadoAsync(
        int asociadoId,
        CancellationToken cancellationToken = default);
}
