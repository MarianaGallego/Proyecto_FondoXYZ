namespace FondoXYZ.Data.Entities;

public class Sitio
{
    public int SitioId { get; set; }
    public int RegionId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoSitio { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int CapacidadMaximaTotal { get; set; }
    public bool Activo { get; set; }

    public Region Region { get; set; } = null!;
    public ICollection<ServicioSitio> Servicios { get; set; } = [];
    public ICollection<BloqueAlojamiento> BloquesAlojamiento { get; set; } = [];
    public ICollection<UnidadAlojamiento> UnidadesAlojamiento { get; set; } = [];
}
