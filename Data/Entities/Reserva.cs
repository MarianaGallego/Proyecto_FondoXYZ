namespace FondoXYZ.Data.Entities;

public class Reserva
{
    public int ReservaId { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public int AsociadoId { get; set; }
    public int SitioId { get; set; }
    public string TipoReserva { get; set; } = string.Empty;
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int NumeroPersonas { get; set; }
    public int NumeroUnidadesSolicitadas { get; set; }
    public int NumeroNoches { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalServicios { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Borrador";
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public Asociado Asociado { get; set; } = null!;
    public Sitio Sitio { get; set; } = null!;
    public ICollection<ReservaUnidad> Unidades { get; set; } = [];
    public ICollection<ReservaAcompanante> Acompanantes { get; set; } = [];
    public ICollection<ReservaServicio> Servicios { get; set; } = [];
    public ICollection<AuditoriaTarifa> AuditoriaTarifas { get; set; } = [];
    public ICollection<Pago> Pagos { get; set; } = [];
}

public class ReservaUnidad
{
    public int ReservaUnidadId { get; set; }
    public int ReservaId { get; set; }
    public int UnidadAlojamientoId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal PrecioNoche { get; set; }
    public decimal Subtotal { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public UnidadAlojamiento UnidadAlojamiento { get; set; } = null!;
}

public class ReservaAcompanante
{
    public int ReservaAcompananteId { get; set; }
    public int ReservaId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public int Orden { get; set; }
    public decimal TarifaAplicada { get; set; }

    public Reserva Reserva { get; set; } = null!;
}

public class ReservaServicio
{
    public int ReservaServicioId { get; set; }
    public int ReservaId { get; set; }
    public int TipoServicioId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public TipoServicio TipoServicio { get; set; } = null!;
}

public class AuditoriaTarifa
{
    public int AuditoriaTarifaId { get; set; }
    public int ReservaId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public int? TarifaId { get; set; }

    public Reserva Reserva { get; set; } = null!;
}
