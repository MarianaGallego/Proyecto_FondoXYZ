namespace FondoXYZ.Data.Entities;

public class UnidadAlojamiento
{
    public int UnidadAlojamientoId { get; set; }
    public int SitioId { get; set; }
    public int? BloqueAlojamientoId { get; set; }
    public int CategoriaTarifaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int NumeroHabitacionesInternas { get; set; }
    public int CapacidadMaxima { get; set; }
    public bool Activo { get; set; }

    public Sitio Sitio { get; set; } = null!;
    public BloqueAlojamiento? Bloque { get; set; }
}
