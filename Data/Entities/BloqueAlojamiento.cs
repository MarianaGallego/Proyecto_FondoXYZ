namespace FondoXYZ.Data.Entities;

public class BloqueAlojamiento
{
    public int BloqueAlojamientoId { get; set; }
    public int SitioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadMaxima { get; set; }

    public Sitio Sitio { get; set; } = null!;
    public ICollection<UnidadAlojamiento> Unidades { get; set; } = [];
}
