namespace FondoXYZ.Models.DTOs;

public class CrearReservaRequest
{
    public int AsociadoId { get; set; }
    public int SitioId { get; set; }
    public string TipoReserva { get; set; } = "Alojamiento";
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int NumeroPersonas { get; set; }
    public int? TemporadaId { get; set; }
    public string? Observaciones { get; set; }
    public IList<int> UnidadesAlojamientoIds { get; set; } = [];
    public IList<AcompananteRequest> Acompanantes { get; set; } = [];
    public IList<ServicioAdicionalRequest> ServiciosAdicionales { get; set; } = [];
}

public class AcompananteRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
}

public class ServicioAdicionalRequest
{
    public int TipoServicioId { get; set; }
    public int Cantidad { get; set; } = 1;
}

public class ReservaCreadaResponseDto
{
    public int ReservaId { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public string TipoReserva { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int SitioId { get; set; }
    public string SitioNombre { get; set; } = string.Empty;
    public int AsociadoId { get; set; }
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int NumeroNoches { get; set; }
    public int NumeroPersonas { get; set; }
    public int NumeroUnidadesSolicitadas { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalServicios { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<ReservaUnidadResponseDto> Unidades { get; set; } = [];
    public IReadOnlyList<ReservaAcompananteResponseDto> Acompanantes { get; set; } = [];
    public IReadOnlyList<ReservaServicioResponseDto> Servicios { get; set; } = [];
}

public class ReservaUnidadResponseDto
{
    public int UnidadAlojamientoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioNoche { get; set; }
    public decimal Subtotal { get; set; }
}

public class ReservaAcompananteResponseDto
{
    public int Orden { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public decimal TarifaAplicada { get; set; }
}

public class ReservaServicioResponseDto
{
    public int TipoServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
