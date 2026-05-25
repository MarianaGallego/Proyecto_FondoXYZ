namespace FondoXYZ.Data.Models;

/// <summary>
/// Resultado del procedimiento SP_ConsultarHabitacionesDisponibles.
/// </summary>
public class UnidadDisponibleSpResult
{
    public int UnidadAlojamientoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }
    public int NumeroHabitacionesInternas { get; set; }
    public int SitioId { get; set; }
    public string SitioCodigo { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string TipoSitio { get; set; } = string.Empty;
    public string CategoriaTarifaCodigo { get; set; } = string.Empty;
    public string CategoriaTarifaNombre { get; set; } = string.Empty;
}
