using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface IDisponibilidadService
{
    Task<DisponibilidadResponseDto> ConsultarDisponibilidadAsync(
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? sitioId = null,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default);

    Task<UnidadAlojamientoDisponibilidadResponseDto> ConsultarDisponibilidadUnidadAsync(
        int unidadAlojamientoId,
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default);
}
