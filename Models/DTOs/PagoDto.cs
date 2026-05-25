namespace FondoXYZ.Models.DTOs;

public class RegistrarPagoRequest
{
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = "Iniciado";
}

public class PagoReservaResponseDto
{
    public int PagoId { get; set; }
    public int ReservaId { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }
}
