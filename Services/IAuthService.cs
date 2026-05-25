using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
