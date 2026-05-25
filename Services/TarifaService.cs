using System.Data;
using System.Data.Common;
using FondoXYZ.Data;
using FondoXYZ.Data.Models;
using FondoXYZ.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FondoXYZ.Services;

public class TarifaService : ITarifaService
{
    private readonly FondoXYZDbContext _context;

    public TarifaService(FondoXYZDbContext context)
    {
        _context = context;
    }

    public async Task<TarifaConsultaResponseDto> ConsultarTarifasAsync(
        int? sitioId = null,
        int? unidadAlojamientoId = null,
        int? temporadaId = null,
        int? numeroPersonas = null,
        CancellationToken cancellationToken = default)
    {
        ValidarParametros(sitioId, unidadAlojamientoId, temporadaId, numeroPersonas);

        var tarifas = await EjecutarProcedimientoAsync(
            sitioId,
            unidadAlojamientoId,
            temporadaId,
            numeroPersonas,
            cancellationToken);

        return new TarifaConsultaResponseDto
        {
            SitioId = sitioId,
            UnidadAlojamientoId = unidadAlojamientoId,
            TemporadaId = temporadaId,
            NumeroPersonas = numeroPersonas,
            TotalTarifas = tarifas.Count,
            Tarifas = tarifas
        };
    }

    public async Task<CalcularTarifaResponseDto> CalcularTarifaAsync(
        CalcularTarifaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarParametrosCalculo(request);

        return await EjecutarCalculoProcedimientoAsync(request, cancellationToken);
    }

    private async Task<CalcularTarifaResponseDto> EjecutarCalculoProcedimientoAsync(
        CalcularTarifaRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.SP_CalcularTarifaReserva
                @SitioId,
                @FechaEntrada,
                @FechaSalida,
                @NumeroPersonas,
                @NumeroUnidades,
                @UnidadAlojamientoId,
                @NumeroHabitacionesInternas,
                @TemporadaId
            """;

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var transaccionActual = _context.Database.CurrentTransaction?.GetDbTransaction();
        if (transaccionActual is not null)
        {
            command.Transaction = (SqlTransaction)transaccionActual;
        }

        command.Parameters.Add(new SqlParameter("@SitioId", SqlDbType.Int) { Value = request.SitioId });
        command.Parameters.Add(new SqlParameter("@FechaEntrada", SqlDbType.Date)
        {
            Value = request.FechaEntrada.ToDateTime(TimeOnly.MinValue)
        });
        command.Parameters.Add(new SqlParameter("@FechaSalida", SqlDbType.Date)
        {
            Value = request.FechaSalida.ToDateTime(TimeOnly.MinValue)
        });
        command.Parameters.Add(new SqlParameter("@NumeroPersonas", SqlDbType.Int) { Value = request.NumeroPersonas });
        command.Parameters.Add(new SqlParameter("@NumeroUnidades", SqlDbType.Int) { Value = request.NumeroUnidades });
        command.Parameters.Add(new SqlParameter("@UnidadAlojamientoId", SqlDbType.Int)
        {
            Value = request.UnidadAlojamientoId ?? (object)DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@NumeroHabitacionesInternas", SqlDbType.Int)
        {
            Value = request.NumeroHabitacionesInternas ?? (object)DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@TemporadaId", SqlDbType.Int)
        {
            Value = request.TemporadaId ?? (object)DBNull.Value
        });

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("El procedimiento no devolvió el resumen del cálculo.");
            }

            var respuesta = new CalcularTarifaResponseDto
            {
                SitioId = reader.GetInt32(reader.GetOrdinal("SitioId")),
                FechaEntrada = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("FechaEntrada"))),
                FechaSalida = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("FechaSalida"))),
                NumeroNoches = reader.GetInt32(reader.GetOrdinal("NumeroNoches")),
                NumeroPersonas = reader.GetInt32(reader.GetOrdinal("NumeroPersonas")),
                NumeroUnidades = reader.GetInt32(reader.GetOrdinal("NumeroUnidades")),
                PersonasPorUnidad = reader.GetInt32(reader.GetOrdinal("PersonasPorUnidad")),
                CategoriaTarifaId = reader.GetInt32(reader.GetOrdinal("CategoriaTarifaId")),
                CategoriaTarifaNombre = reader.GetString(reader.GetOrdinal("CategoriaTarifaNombre")),
                UnidadAlojamientoId = GetNullableInt(reader, "UnidadAlojamientoId"),
                NumeroHabitacionesInternas = GetNullableInt(reader, "NumeroHabitacionesInternas"),
                Total = reader.GetDecimal(reader.GetOrdinal("Total"))
            };

            var detalle = new List<CalculoTarifaDetalleDto>();

            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    detalle.Add(new CalculoTarifaDetalleDto
                    {
                        Fecha = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("Fecha"))),
                        UnidadNum = reader.GetInt32(reader.GetOrdinal("UnidadNum")),
                        Concepto = reader.GetString(reader.GetOrdinal("Concepto")),
                        Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                        ValorUnitario = reader.GetDecimal(reader.GetOrdinal("ValorUnitario")),
                        Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                        TarifaId = GetNullableInt(reader, "TarifaId")
                    });
                }
            }

            respuesta.Detalle = detalle;
            return respuesta;
        }
        catch (SqlException ex) when (ex.Class >= 16)
        {
            throw new ArgumentException(ObtenerMensajeSql(ex), ex);
        }
    }

    private static int? GetNullableInt(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static void ValidarParametrosCalculo(CalcularTarifaRequest request)
    {
        if (request.SitioId < 1)
        {
            throw new ArgumentException("El sitioId debe ser mayor o igual a 1.");
        }

        if (request.FechaEntrada == default)
        {
            throw new ArgumentException("La fechaEntrada es obligatoria.");
        }

        if (request.FechaSalida == default)
        {
            throw new ArgumentException("La fechaSalida es obligatoria.");
        }

        if (request.FechaSalida <= request.FechaEntrada)
        {
            throw new ArgumentException("La fecha de salida debe ser posterior a la fecha de entrada.");
        }

        if (request.NumeroPersonas < 1)
        {
            throw new ArgumentException("El número de personas debe ser mayor o igual a 1.");
        }

        if (request.NumeroUnidades < 1)
        {
            throw new ArgumentException("El número de unidades debe ser mayor o igual a 1.");
        }

        if (!request.UnidadAlojamientoId.HasValue && !request.NumeroHabitacionesInternas.HasValue)
        {
            throw new ArgumentException("Indique la unidad de alojamiento o el número de habitaciones internas.");
        }

        if (request.UnidadAlojamientoId.HasValue && request.UnidadAlojamientoId.Value < 1)
        {
            throw new ArgumentException("El unidadAlojamientoId debe ser mayor o igual a 1.");
        }

        if (request.NumeroHabitacionesInternas.HasValue &&
            request.NumeroHabitacionesInternas.Value is not (1 or 2))
        {
            throw new ArgumentException("El número de habitaciones internas debe ser 1 o 2.");
        }

        if (request.TemporadaId.HasValue && request.TemporadaId.Value < 1)
        {
            throw new ArgumentException("El temporadaId debe ser mayor o igual a 1.");
        }
    }

    private async Task<IReadOnlyList<TarifaDto>> EjecutarProcedimientoAsync(
        int? sitioId,
        int? unidadAlojamientoId,
        int? temporadaId,
        int? numeroPersonas,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.SP_ConsultarTarifas
                @SitioId,
                @UnidadAlojamientoId,
                @TemporadaId,
                @NumeroPersonas
            """;

        var parametros = new[]
        {
            new SqlParameter("@SitioId", SqlDbType.Int)
            {
                Value = sitioId ?? (object)DBNull.Value
            },
            new SqlParameter("@UnidadAlojamientoId", SqlDbType.Int)
            {
                Value = unidadAlojamientoId ?? (object)DBNull.Value
            },
            new SqlParameter("@TemporadaId", SqlDbType.Int)
            {
                Value = temporadaId ?? (object)DBNull.Value
            },
            new SqlParameter("@NumeroPersonas", SqlDbType.Int)
            {
                Value = numeroPersonas ?? (object)DBNull.Value
            }
        };

        try
        {
            var resultados = await _context.Database
                .SqlQueryRaw<TarifaSpResult>(sql, parametros)
                .ToListAsync(cancellationToken);

            return resultados.Select(MapearTarifa).ToList();
        }
        catch (SqlException ex) when (ex.Class >= 16)
        {
            throw new ArgumentException(ObtenerMensajeSql(ex), ex);
        }
    }

    private static TarifaDto MapearTarifa(TarifaSpResult tarifa) =>
        new()
        {
            TarifaId = tarifa.TarifaId,
            TipoConcepto = tarifa.TipoConcepto,
            Precio = tarifa.Precio,
            PrecioPersonaAdicional = tarifa.PrecioPersonaAdicional,
            PersonasMin = tarifa.PersonasMin,
            PersonasMax = tarifa.PersonasMax,
            DiasSemana = tarifa.DiasSemana,
            ExcluirFestivos = tarifa.ExcluirFestivos,
            ExcluirSemanaEscolar = tarifa.ExcluirSemanaEscolar,
            ExcluirTemporadaAlta = tarifa.ExcluirTemporadaAlta,
            VigenciaDesde = ConvertirFecha(tarifa.VigenciaDesde),
            VigenciaHasta = ConvertirFecha(tarifa.VigenciaHasta),
            SitioId = tarifa.SitioId,
            SitioCodigo = tarifa.SitioCodigo,
            SitioNombre = tarifa.SitioNombre,
            CategoriaTarifaId = tarifa.CategoriaTarifaId,
            CategoriaTarifaCodigo = tarifa.CategoriaTarifaCodigo,
            CategoriaTarifaNombre = tarifa.CategoriaTarifaNombre,
            TemporadaId = tarifa.TemporadaId,
            TemporadaCodigo = tarifa.TemporadaCodigo,
            TemporadaNombre = tarifa.TemporadaNombre,
            EsTemporadaAlta = tarifa.EsTemporadaAlta,
            UnidadAlojamientoId = tarifa.UnidadAlojamientoId
        };

    private static DateOnly? ConvertirFecha(DateTime? fecha) =>
        fecha.HasValue ? DateOnly.FromDateTime(fecha.Value) : null;

    private static void ValidarParametros(
        int? sitioId,
        int? unidadAlojamientoId,
        int? temporadaId,
        int? numeroPersonas)
    {
        if (!sitioId.HasValue && !unidadAlojamientoId.HasValue)
        {
            throw new ArgumentException("Debe indicar el sitio o la unidad de alojamiento.");
        }

        if (sitioId.HasValue && sitioId.Value < 1)
        {
            throw new ArgumentException("El sitioId debe ser mayor o igual a 1.");
        }

        if (unidadAlojamientoId.HasValue && unidadAlojamientoId.Value < 1)
        {
            throw new ArgumentException("El unidadAlojamientoId debe ser mayor o igual a 1.");
        }

        if (temporadaId.HasValue && temporadaId.Value < 1)
        {
            throw new ArgumentException("El temporadaId debe ser mayor o igual a 1.");
        }

        if (numeroPersonas.HasValue && numeroPersonas.Value < 1)
        {
            throw new ArgumentException("El número de personas debe ser mayor o igual a 1.");
        }
    }

    private static string ObtenerMensajeSql(SqlException ex) =>
        ex.Errors.Count > 0 ? ex.Errors[0].Message : ex.Message;
}
