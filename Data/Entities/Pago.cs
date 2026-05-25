namespace FondoXYZ.Data.Entities;

public class Pago
{
    public int PagoId { get; set; }
    public int ReservaId { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Reserva Reserva { get; set; } = null!;
}
