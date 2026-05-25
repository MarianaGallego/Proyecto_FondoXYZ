namespace FondoXYZ.Data.Entities;

public class ServicioSitio
{
    public int ServicioSitioId { get; set; }
    public int SitioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }

    public Sitio Sitio { get; set; } = null!;
}
