using FondoXYZ.Data;
using FondoXYZ.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Services;

public class SitioService : ISitioService
{
    private const int LongitudDescripcionResumida = 200;
    private readonly FondoXYZDbContext _context;

    public SitioService(FondoXYZDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SitioListadoDto>> ListarSitiosAsync(
        CancellationToken cancellationToken = default)
    {
        var sitios = await _context.Sitios
            .AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Nombre)
            .Select(s => new SitioListadoDto
            {
                Nombre = s.Nombre,
                Ciudad = s.Ciudad,
                Tipo = s.TipoSitio == "SedeRecreativa" ? "Sede Recreativa" : "Apartamento",
                CupoMaximo = s.CapacidadMaximaTotal,
                DescripcionResumida = s.Descripcion ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        foreach (var sitio in sitios)
        {
            sitio.DescripcionResumida = TruncarDescripcion(sitio.DescripcionResumida);
        }

        return sitios;
    }

    public async Task<SitioDetalleDto?> ObtenerSitioPorIdAsync(
        int sitioId,
        CancellationToken cancellationToken = default)
    {
        var sitio = await _context.Sitios
            .AsNoTracking()
            .Include(s => s.Region)
            .Include(s => s.Servicios)
            .Include(s => s.BloquesAlojamiento)
                .ThenInclude(b => b.Unidades.Where(u => u.Activo))
            .Include(s => s.UnidadesAlojamiento.Where(u => u.Activo && u.BloqueAlojamientoId == null))
            .FirstOrDefaultAsync(s => s.SitioId == sitioId && s.Activo, cancellationToken);

        if (sitio is null)
        {
            return null;
        }

        return new SitioDetalleDto
        {
            SitioId = sitio.SitioId,
            Codigo = sitio.Codigo,
            Nombre = sitio.Nombre,
            Ciudad = sitio.Ciudad,
            Tipo = MapearTipoSitio(sitio.TipoSitio),
            Region = sitio.Region.Nombre,
            Descripcion = sitio.Descripcion ?? string.Empty,
            Ubicacion = sitio.Ubicacion,
            CupoMaximo = sitio.CapacidadMaximaTotal,
            Servicios = sitio.Servicios
                .OrderBy(sv => sv.Nombre)
                .Select(sv => new ServicioSitioDto
                {
                    Nombre = sv.Nombre,
                    Descripcion = sv.Descripcion,
                    Categoria = sv.Categoria
                })
                .ToList(),
            BloquesAlojamiento = sitio.BloquesAlojamiento
                .OrderBy(b => b.Nombre)
                .Select(b => new BloqueAlojamientoDetalleDto
                {
                    Nombre = b.Nombre,
                    Descripcion = b.Descripcion,
                    CapacidadMaxima = b.CapacidadMaxima,
                    Unidades = b.Unidades
                        .OrderBy(u => u.Codigo)
                        .Select(MapearUnidad)
                        .ToList()
                })
                .ToList(),
            UnidadesAlojamiento = sitio.UnidadesAlojamiento
                .OrderBy(u => u.Codigo)
                .Select(MapearUnidad)
                .ToList()
        };
    }

    private static UnidadAlojamientoDto MapearUnidad(Data.Entities.UnidadAlojamiento unidad) =>
        new()
        {
            UnidadAlojamientoId = unidad.UnidadAlojamientoId,
            Codigo = unidad.Codigo,
            Nombre = unidad.Nombre,
            Descripcion = unidad.Descripcion,
            NumeroHabitacionesInternas = unidad.NumeroHabitacionesInternas,
            CapacidadMaxima = unidad.CapacidadMaxima
        };

    private static string MapearTipoSitio(string tipoSitio) =>
        tipoSitio == "SedeRecreativa" ? "Sede Recreativa" : "Apartamento";

    private static string TruncarDescripcion(string descripcion)
    {
        if (descripcion.Length <= LongitudDescripcionResumida)
        {
            return descripcion;
        }

        return descripcion[..LongitudDescripcionResumida].TrimEnd() + "...";
    }
}
