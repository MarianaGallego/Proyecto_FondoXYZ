namespace FondoXYZ.Models.DTOs;

public class TarifaConsultaRequest
{
    public int? SitioId { get; set; }
    public int? UnidadAlojamientoId { get; set; }
    public int? TemporadaId { get; set; }
    public int? NumeroPersonas { get; set; }
}

public class TarifaConsultaResponseDto
{
    public int? SitioId { get; set; }
    public int? UnidadAlojamientoId { get; set; }
    public int? TemporadaId { get; set; }
    public int? NumeroPersonas { get; set; }
    public int TotalTarifas { get; set; }
    public IReadOnlyList<TarifaDto> Tarifas { get; set; } = [];
}

public class TarifaDto
{
    public int TarifaId { get; set; }
    public string TipoConcepto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal? PrecioPersonaAdicional { get; set; }
    public int PersonasMin { get; set; }
    public int PersonasMax { get; set; }
    public byte? DiasSemana { get; set; }
    public bool ExcluirFestivos { get; set; }
    public bool ExcluirSemanaEscolar { get; set; }
    public bool ExcluirTemporadaAlta { get; set; }
    public DateOnly? VigenciaDesde { get; set; }
    public DateOnly? VigenciaHasta { get; set; }
    public int? SitioId { get; set; }
    public string? SitioCodigo { get; set; }
    public string? SitioNombre { get; set; }
    public int? CategoriaTarifaId { get; set; }
    public string? CategoriaTarifaCodigo { get; set; }
    public string? CategoriaTarifaNombre { get; set; }
    public int? TemporadaId { get; set; }
    public string? TemporadaCodigo { get; set; }
    public string? TemporadaNombre { get; set; }
    public bool? EsTemporadaAlta { get; set; }
    public int? UnidadAlojamientoId { get; set; }
}
