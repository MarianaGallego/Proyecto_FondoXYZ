using System.Data;
using FondoXYZ.Data;
using FondoXYZ.Data.Models;
using FondoXYZ.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Services;

public class DisponibilidadService : IDisponibilidadService
{
    private readonly FondoXYZDbContext _context;

    public DisponibilidadService(FondoXYZDbContext context)
    {
        _context = context;
    }

    public async Task<DisponibilidadResponseDto> ConsultarDisponibilidadAsync(
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? sitioId = null,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default)
    {
        ValidarParametros(fechaEntrada, fechaSalida, sitioId, numeroPersonas);

        if (sitioId.HasValue)
        {
            var sitioExiste = await _context.Sitios
                .AnyAsync(s => s.SitioId == sitioId.Value && s.Activo, cancellationToken);

            if (!sitioExiste)
            {
                throw new ArgumentException($"No se encontró un sitio activo con id {sitioId.Value}.");
            }
        }

        var unidades = await EjecutarProcedimientoAsync(
            fechaEntrada,
            fechaSalida,
            sitioId,
            numeroPersonas,
            cancellationToken);

        return new DisponibilidadResponseDto
        {
            FechaEntrada = fechaEntrada,
            FechaSalida = fechaSalida,
            SitioId = sitioId,
            NumeroPersonas = numeroPersonas,
            TotalUnidadesDisponibles = unidades.Count,
            Unidades = unidades
        };
    }

    public async Task<UnidadAlojamientoDisponibilidadResponseDto> ConsultarDisponibilidadUnidadAsync(
        int unidadAlojamientoId,
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default)
    {
        if (unidadAlojamientoId < 1)
        {
            throw new ArgumentException("El unidadAlojamientoId debe ser mayor o igual a 1.");
        }

        ValidarParametros(fechaEntrada, fechaSalida, sitioId: null, numeroPersonas);

        var unidad = await _context.UnidadesAlojamiento
            .AsNoTracking()
            .Include(u => u.Sitio)
            .FirstOrDefaultAsync(
                u => u.UnidadAlojamientoId == unidadAlojamientoId && u.Activo && u.Sitio.Activo,
                cancellationToken)
            ?? throw new ArgumentException(
                $"No se encontró una unidad de alojamiento activa con id {unidadAlojamientoId}.");

        if (numeroPersonas.HasValue && numeroPersonas.Value > unidad.CapacidadMaxima)
        {
            return new UnidadAlojamientoDisponibilidadResponseDto
            {
                UnidadAlojamientoId = unidad.UnidadAlojamientoId,
                Codigo = unidad.Codigo,
                Nombre = unidad.Nombre,
                SitioId = unidad.SitioId,
                SitioNombre = unidad.Sitio.Nombre,
                FechaEntrada = fechaEntrada,
                FechaSalida = fechaSalida,
                NumeroPersonas = numeroPersonas,
                CapacidadMaxima = unidad.CapacidadMaxima,
                NumeroHabitacionesInternas = unidad.NumeroHabitacionesInternas,
                Disponible = false,
                Mensaje = $"La unidad admite máximo {unidad.CapacidadMaxima} persona(s)."
            };
        }

        var unidadesDisponibles = await EjecutarProcedimientoAsync(
            fechaEntrada,
            fechaSalida,
            unidad.SitioId,
            numeroPersonas,
            cancellationToken);

        var disponible = unidadesDisponibles.Any(u => u.UnidadAlojamientoId == unidadAlojamientoId);

        return new UnidadAlojamientoDisponibilidadResponseDto
        {
            UnidadAlojamientoId = unidad.UnidadAlojamientoId,
            Codigo = unidad.Codigo,
            Nombre = unidad.Nombre,
            SitioId = unidad.SitioId,
            SitioNombre = unidad.Sitio.Nombre,
            FechaEntrada = fechaEntrada,
            FechaSalida = fechaSalida,
            NumeroPersonas = numeroPersonas,
            CapacidadMaxima = unidad.CapacidadMaxima,
            NumeroHabitacionesInternas = unidad.NumeroHabitacionesInternas,
            Disponible = disponible,
            Mensaje = disponible
                ? "La unidad está disponible en el rango de fechas indicado."
                : "La unidad no está disponible en el rango de fechas indicado."
        };
    }

    private async Task<IReadOnlyList<UnidadDisponibleDto>> EjecutarProcedimientoAsync(
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? sitioId,
        int? numeroPersonas,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.SP_ConsultarHabitacionesDisponibles
                @FechaEntrada,
                @FechaSalida,
                @SitioId,
                @NumeroPersonas
            """;

        var parametros = new[]
        {
            new SqlParameter("@FechaEntrada", SqlDbType.Date)
            {
                Value = fechaEntrada.ToDateTime(TimeOnly.MinValue)
            },
            new SqlParameter("@FechaSalida", SqlDbType.Date)
            {
                Value = fechaSalida.ToDateTime(TimeOnly.MinValue)
            },
            new SqlParameter("@SitioId", SqlDbType.Int)
            {
                Value = sitioId ?? (object)DBNull.Value
            },
            new SqlParameter("@NumeroPersonas", SqlDbType.Int)
            {
                Value = numeroPersonas ?? (object)DBNull.Value
            }
        };

        try
        {
            var resultados = await _context.Database
                .SqlQueryRaw<UnidadDisponibleSpResult>(sql, parametros)
                .ToListAsync(cancellationToken);

            return resultados.Select(MapearUnidad).ToList();
        }
        catch (SqlException ex) when (ex.Class >= 16)
        {
            throw new ArgumentException(ObtenerMensajeSql(ex), ex);
        }
    }

    private static UnidadDisponibleDto MapearUnidad(UnidadDisponibleSpResult unidad) =>
        new()
        {
            UnidadAlojamientoId = unidad.UnidadAlojamientoId,
            Codigo = unidad.Codigo,
            Nombre = unidad.Nombre,
            CapacidadMaxima = unidad.CapacidadMaxima,
            NumeroHabitacionesInternas = unidad.NumeroHabitacionesInternas,
            SitioId = unidad.SitioId,
            SitioCodigo = unidad.SitioCodigo,
            SitioNombre = unidad.SitioNombre,
            Ciudad = unidad.Ciudad,
            TipoSitio = unidad.TipoSitio == "SedeRecreativa" ? "Sede Recreativa" : "Apartamento",
            CategoriaTarifaCodigo = unidad.CategoriaTarifaCodigo,
            CategoriaTarifaNombre = unidad.CategoriaTarifaNombre
        };

    private static void ValidarParametros(
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        int? sitioId,
        int? numeroPersonas)
    {
        if (fechaEntrada == default)
        {
            throw new ArgumentException("El parámetro fechaEntrada es obligatorio.");
        }

        if (fechaSalida == default)
        {
            throw new ArgumentException("El parámetro fechaSalida es obligatorio.");
        }

        if (fechaSalida <= fechaEntrada)
        {
            throw new ArgumentException("La fecha de salida debe ser posterior a la fecha de entrada.");
        }

        if (sitioId.HasValue && sitioId.Value < 1)
        {
            throw new ArgumentException("El sitioId debe ser mayor o igual a 1.");
        }

        if (numeroPersonas.HasValue && numeroPersonas.Value < 1)
        {
            throw new ArgumentException("El número de personas debe ser mayor o igual a 1.");
        }
    }

    private static string ObtenerMensajeSql(SqlException ex) =>
        ex.Errors.Count > 0 ? ex.Errors[0].Message : ex.Message;
}
