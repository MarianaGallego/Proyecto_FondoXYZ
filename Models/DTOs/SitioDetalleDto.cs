namespace FondoXYZ.Models.DTOs;

public class SitioDetalleDto
{
    public int SitioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public int CupoMaximo { get; set; }
    public IReadOnlyList<ServicioSitioDto> Servicios { get; set; } = [];
    public IReadOnlyList<BloqueAlojamientoDetalleDto> BloquesAlojamiento { get; set; } = [];
    public IReadOnlyList<UnidadAlojamientoDto> UnidadesAlojamiento { get; set; } = [];
}

public class ServicioSitioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
}

public class BloqueAlojamientoDetalleDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadMaxima { get; set; }
    public IReadOnlyList<UnidadAlojamientoDto> Unidades { get; set; } = [];
}

public class UnidadAlojamientoDto
{
    public int UnidadAlojamientoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int NumeroHabitacionesInternas { get; set; }
    public int CapacidadMaxima { get; set; }
}
