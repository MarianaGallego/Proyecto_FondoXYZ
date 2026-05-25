namespace FondoXYZ.Data.Entities;

public class Asociado
{
    public int AsociadoId { get; set; }
    public string NumeroAsociado { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Clave { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = [];
}
