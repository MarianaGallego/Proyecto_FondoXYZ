using FondoXYZ.Models.DTOs;

namespace FondoXYZ.Services;

public interface ITarifaService
{
    Task<TarifaConsultaResponseDto> ConsultarTarifasAsync(
        int? sitioId = null,
        int? unidadAlojamientoId = null,
        int? temporadaId = null,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default);

    Task<CalcularTarifaResponseDto> CalcularTarifaAsync(
        CalcularTarifaRequest request,
        CancellationToken cancellationToken = default);
}
