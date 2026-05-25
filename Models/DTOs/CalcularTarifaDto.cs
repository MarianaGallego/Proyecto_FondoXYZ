namespace FondoXYZ.Models.DTOs;

public class CalcularTarifaRequest
{
    public int SitioId { get; set; }
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int NumeroPersonas { get; set; }
    public int NumeroUnidades { get; set; } = 1;
    public int? UnidadAlojamientoId { get; set; }
    public int? NumeroHabitacionesInternas { get; set; }
    public int? TemporadaId { get; set; }
}

public class CalcularTarifaResponseDto
{
    public int SitioId { get; set; }
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int NumeroNoches { get; set; }
    public int NumeroPersonas { get; set; }
    public int NumeroUnidades { get; set; }
    public int PersonasPorUnidad { get; set; }
    public int CategoriaTarifaId { get; set; }
    public string CategoriaTarifaNombre { get; set; } = string.Empty;
    public int? UnidadAlojamientoId { get; set; }
    public int? NumeroHabitacionesInternas { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<CalculoTarifaDetalleDto> Detalle { get; set; } = [];
}

public class CalculoTarifaDetalleDto
{
    public DateOnly Fecha { get; set; }
    public int UnidadNum { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public int? TarifaId { get; set; }
}
