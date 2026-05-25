using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface IReservaService
{
    Task<ReservaCreadaResponseDto> CrearReservaAsync(
        CrearReservaRequest request,
        CancellationToken cancellationToken = default);
}
