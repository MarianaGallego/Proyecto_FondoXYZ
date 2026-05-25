using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface IPagoService
{
    Task<PagoReservaResponseDto> RegistrarPagoAsync(
        int reservaId,
        RegistrarPagoRequest request,
        CancellationToken cancellationToken = default);
}
