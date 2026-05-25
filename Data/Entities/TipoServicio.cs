namespace FondoXYZ.Data.Entities;

public class TipoServicio
{
    public int TipoServicioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsPorUnidad { get; set; }
    public bool Activo { get; set; }
}
