namespace FondoXYZ.Models.DTOs;

public class DisponibilidadResponseDto
{
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int? SitioId { get; set; }
    public int? NumeroPersonas { get; set; }
    public int TotalUnidadesDisponibles { get; set; }
    public IReadOnlyList<UnidadDisponibleDto> Unidades { get; set; } = [];
}

public class UnidadDisponibleDto
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

public class UnidadAlojamientoDisponibilidadResponseDto
{
    public int UnidadAlojamientoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int SitioId { get; set; }
    public string SitioNombre { get; set; } = string.Empty;
    public DateOnly FechaEntrada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int? NumeroPersonas { get; set; }
    public int CapacidadMaxima { get; set; }
    public int NumeroHabitacionesInternas { get; set; }
    public bool Disponible { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
