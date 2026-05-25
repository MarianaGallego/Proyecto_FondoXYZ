namespace FondoXYZ.Data.Entities;

public class Region
{
    public int RegionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public ICollection<Sitio> Sitios { get; set; } = [];
}
