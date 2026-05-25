namespace FondoXYZ.Data.Entities;

public class Tarifa
{
    public int TarifaId { get; set; }
    public int? SitioId { get; set; }
    public int? CategoriaTarifaId { get; set; }
    public int? TemporadaId { get; set; }
    public string TipoConcepto { get; set; } = string.Empty;
    public int PersonasMin { get; set; }
    public int PersonasMax { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioPersonaAdicional { get; set; }
    public bool Activo { get; set; }
}
