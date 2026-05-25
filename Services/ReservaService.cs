using FondoXYZ.Data;
using FondoXYZ.Data.Entities;
using FondoXYZ.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Services;

public class ReservaService : IReservaService
{
    private const string TipoAlojamiento = "Alojamiento";
    private const string TipoVisitaDia = "VisitaDia";
    private const string EstadoPendientePago = "PendientePago";

    private readonly FondoXYZDbContext _context;
    private readonly ITarifaService _tarifaService;
    private readonly IDisponibilidadService _disponibilidadService;

    public ReservaService(
        FondoXYZDbContext context,
        ITarifaService tarifaService,
        IDisponibilidadService disponibilidadService)
    {
        _context = context;
        _tarifaService = tarifaService;
        _disponibilidadService = disponibilidadService;
    }

    public async Task<ReservaCreadaResponseDto> CrearReservaAsync(
        CrearReservaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitud(request);

        var asociado = await _context.Asociados
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AsociadoId == request.AsociadoId && a.Activo, cancellationToken)
            ?? throw new ArgumentException($"No se encontró un asociado activo con id {request.AsociadoId}.");

        var sitio = await _context.Sitios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SitioId == request.SitioId && s.Activo, cancellationToken)
            ?? throw new ArgumentException($"No se encontró un sitio activo con id {request.SitioId}.");

        var tipoReserva = NormalizarTipoReserva(request.TipoReserva);

        var reserva = new Reserva
        {
            AsociadoId = asociado.AsociadoId,
            SitioId = sitio.SitioId,
            TipoReserva = tipoReserva,
            FechaEntrada = request.FechaEntrada,
            FechaSalida = request.FechaSalida,
            NumeroPersonas = request.NumeroPersonas,
            Observaciones = request.Observaciones,
            Estado = EstadoPendientePago,
            FechaCreacion = DateTime.Now
        };

        var auditoriaLineas = new List<AuditoriaTarifa>();

        decimal subtotal = tipoReserva == TipoAlojamiento
            ? await ProcesarAlojamientoAsync(request, sitio, reserva, auditoriaLineas, cancellationToken)
            : await ProcesarVisitaDiaAsync(request, sitio, reserva, auditoriaLineas, cancellationToken);

        var totalServicios = await ProcesarServiciosAdicionalesAsync(
            request, sitio.SitioId, reserva, auditoriaLineas, cancellationToken);

        reserva.Subtotal = subtotal;
        reserva.TotalServicios = totalServicios;
        reserva.Total = subtotal + totalServicios;

        await using var transaccion = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            reserva.CodigoReserva = await GenerarCodigoReservaAsync(cancellationToken);

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var linea in auditoriaLineas)
            {
                linea.ReservaId = reserva.ReservaId;
            }

            _context.AuditoriaTarifas.AddRange(auditoriaLineas);
            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            return await MapearRespuestaAsync(reserva.ReservaId, cancellationToken);
        }
        catch
        {
            await RevertirTransaccionSiActivaAsync(transaccion, cancellationToken);
            throw;
        }
    }

    private static async Task RevertirTransaccionSiActivaAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaccion,
        CancellationToken cancellationToken)
    {
        try
        {
            await transaccion.RollbackAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // La transacción ya fue confirmada o revertida por otro componente.
        }
    }

    private async Task<decimal> ProcesarAlojamientoAsync(
        CrearReservaRequest request,
        Sitio sitio,
        Reserva reserva,
        List<AuditoriaTarifa> auditoriaLineas,
        CancellationToken cancellationToken)
    {
        if (request.UnidadesAlojamientoIds.Count == 0)
        {
            throw new ArgumentException("Debe indicar al menos una unidad de alojamiento.");
        }

        if (request.FechaSalida <= request.FechaEntrada)
        {
            throw new ArgumentException("La fecha de salida debe ser posterior a la fecha de entrada.");
        }

        var unidadIds = request.UnidadesAlojamientoIds.Distinct().ToList();

        var unidades = await _context.UnidadesAlojamiento
            .AsNoTracking()
            .Where(u => unidadIds.Contains(u.UnidadAlojamientoId))
            .OrderBy(u => u.UnidadAlojamientoId)
            .ToListAsync(cancellationToken);

        if (unidades.Count != unidadIds.Count)
        {
            throw new ArgumentException("Una o más unidades de alojamiento no existen.");
        }

        if (unidades.Any(u => !u.Activo || u.SitioId != request.SitioId))
        {
            throw new ArgumentException("Todas las unidades deben estar activas y pertenecer al sitio indicado.");
        }

        var capacidadTotal = unidades.Sum(u => u.CapacidadMaxima);
        if (request.NumeroPersonas > capacidadTotal)
        {
            throw new ArgumentException(
                $"El número de personas ({request.NumeroPersonas}) supera la capacidad máxima combinada " +
                $"de las unidades seleccionadas ({capacidadTotal}).");
        }

        var personasPorUnidad = (request.NumeroPersonas + unidades.Count - 1) / unidades.Count;
        var unidadInsuficiente = unidades.FirstOrDefault(u => u.CapacidadMaxima < personasPorUnidad);
        if (unidadInsuficiente is not null)
        {
            throw new ArgumentException(
                $"La unidad {unidadInsuficiente.Codigo} admite máximo {unidadInsuficiente.CapacidadMaxima} persona(s), " +
                $"pero repartir {request.NumeroPersonas} personas en {unidades.Count} unidad(es) " +
                $"requiere al menos {personasPorUnidad} por unidad.");
        }

        await VerificarDisponibilidadAsync(
            request.SitioId,
            request.FechaEntrada,
            request.FechaSalida,
            unidadIds,
            cancellationToken);

        var numeroNoches = request.FechaSalida.DayNumber - request.FechaEntrada.DayNumber;
        reserva.NumeroNoches = numeroNoches;
        reserva.NumeroUnidadesSolicitadas = unidades.Count;

        decimal subtotalTotal = 0;

        foreach (var grupo in unidades.GroupBy(u => u.CategoriaTarifaId))
        {
            var unidadesGrupo = grupo.OrderBy(u => u.UnidadAlojamientoId).ToList();
            var personasGrupo = DistribuirPersonas(
                request.NumeroPersonas,
                unidades.Count,
                unidadesGrupo.Count);

            var calculo = await _tarifaService.CalcularTarifaAsync(
                new CalcularTarifaRequest
                {
                    SitioId = request.SitioId,
                    FechaEntrada = request.FechaEntrada,
                    FechaSalida = request.FechaSalida,
                    NumeroPersonas = personasGrupo,
                    NumeroUnidades = unidadesGrupo.Count,
                    UnidadAlojamientoId = unidadesGrupo[0].UnidadAlojamientoId,
                    TemporadaId = request.TemporadaId
                },
                cancellationToken);

            subtotalTotal += calculo.Total;

            auditoriaLineas.AddRange(calculo.Detalle.Select(linea => new AuditoriaTarifa
            {
                Fecha = linea.Fecha,
                Concepto = linea.Concepto,
                Cantidad = linea.Cantidad,
                ValorUnitario = linea.ValorUnitario,
                Subtotal = linea.Subtotal,
                TarifaId = linea.TarifaId
            }));

            var subtotalesPorUnidad = calculo.Detalle
                .GroupBy(d => d.UnidadNum)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Subtotal));

            for (var i = 0; i < unidadesGrupo.Count; i++)
            {
                var subtotalUnidad = subtotalesPorUnidad.GetValueOrDefault(
                    i + 1,
                    calculo.Total / unidadesGrupo.Count);

                reserva.Unidades.Add(new ReservaUnidad
                {
                    UnidadAlojamientoId = unidadesGrupo[i].UnidadAlojamientoId,
                    FechaInicio = request.FechaEntrada,
                    FechaFin = request.FechaSalida,
                    Subtotal = Math.Round(subtotalUnidad, 2),
                    PrecioNoche = numeroNoches > 0
                        ? Math.Round(subtotalUnidad / numeroNoches, 2)
                        : subtotalUnidad
                });
            }
        }

        return subtotalTotal;
    }

    private async Task<decimal> ProcesarVisitaDiaAsync(
        CrearReservaRequest request,
        Sitio sitio,
        Reserva reserva,
        List<AuditoriaTarifa> auditoriaLineas,
        CancellationToken cancellationToken)
    {
        if (sitio.TipoSitio != "SedeRecreativa")
        {
            throw new ArgumentException("La visita de día solo aplica a sedes recreativas.");
        }

        if (request.FechaSalida != request.FechaEntrada)
        {
            throw new ArgumentException("En visita de día la fecha de entrada y salida deben ser la misma.");
        }

        if (request.NumeroPersonas is < 5 or > 10)
        {
            throw new ArgumentException("La visita de día admite entre 5 y 10 personas en total.");
        }

        var tarifaVisita = await _context.Tarifas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Activo && t.TipoConcepto == "VisitaDiaAcompanante",
                cancellationToken)
            ?? throw new ArgumentException("No se encontró tarifa configurada para visita de día.");

        var acompanantesPagos = request.NumeroPersonas - 4;

        if (request.Acompanantes.Count != acompanantesPagos)
        {
            throw new ArgumentException(
                $"Debe registrar {acompanantesPagos} acompañante(s) de pago (del 5° al {request.NumeroPersonas}°).");
        }

        reserva.NumeroNoches = 0;
        reserva.NumeroUnidadesSolicitadas = 0;

        decimal subtotal = 0;

        for (var i = 0; i < request.Acompanantes.Count; i++)
        {
            var acompanante = request.Acompanantes[i];
            var orden = i + 5;

            reserva.Acompanantes.Add(new ReservaAcompanante
            {
                Nombres = acompanante.Nombres,
                Apellidos = acompanante.Apellidos,
                TipoDocumento = acompanante.TipoDocumento,
                NumeroDocumento = acompanante.NumeroDocumento,
                Orden = orden,
                TarifaAplicada = tarifaVisita.Precio
            });

            subtotal += tarifaVisita.Precio;

            auditoriaLineas.Add(new AuditoriaTarifa
            {
                Fecha = request.FechaEntrada,
                Concepto = $"Visita día acompañante ({orden}°)",
                Cantidad = 1,
                ValorUnitario = tarifaVisita.Precio,
                Subtotal = tarifaVisita.Precio,
                TarifaId = tarifaVisita.TarifaId
            });
        }

        return subtotal;
    }

    private async Task<decimal> ProcesarServiciosAdicionalesAsync(
        CrearReservaRequest request,
        int sitioId,
        Reserva reserva,
        List<AuditoriaTarifa> auditoriaLineas,
        CancellationToken cancellationToken)
    {
        decimal totalServicios = 0;

        foreach (var servicioRequest in request.ServiciosAdicionales)
        {
            if (servicioRequest.Cantidad < 1)
            {
                throw new ArgumentException("La cantidad de cada servicio adicional debe ser mayor o igual a 1.");
            }

            var tipoServicio = await _context.TiposServicio
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.TipoServicioId == servicioRequest.TipoServicioId && t.Activo,
                    cancellationToken)
                ?? throw new ArgumentException(
                    $"No se encontró un servicio activo con id {servicioRequest.TipoServicioId}.");

            var tarifaServicio = await _context.Tarifas
                .AsNoTracking()
                .Where(t => t.Activo
                    && t.TipoConcepto == "ServicioAdicional"
                    && (t.SitioId == sitioId || t.SitioId == null))
                .OrderByDescending(t => t.SitioId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ArgumentException(
                    $"No se encontró tarifa para el servicio {tipoServicio.Nombre}.");

            var subtotal = tarifaServicio.Precio * servicioRequest.Cantidad;
            totalServicios += subtotal;

            reserva.Servicios.Add(new ReservaServicio
            {
                TipoServicioId = tipoServicio.TipoServicioId,
                Cantidad = servicioRequest.Cantidad,
                PrecioUnitario = tarifaServicio.Precio,
                Subtotal = subtotal
            });

            auditoriaLineas.Add(new AuditoriaTarifa
            {
                Fecha = request.FechaEntrada,
                Concepto = tipoServicio.Nombre,
                Cantidad = servicioRequest.Cantidad,
                ValorUnitario = tarifaServicio.Precio,
                Subtotal = subtotal,
                TarifaId = tarifaServicio.TarifaId
            });
        }

        return totalServicios;
    }

    private async Task VerificarDisponibilidadAsync(
        int sitioId,
        DateOnly fechaEntrada,
        DateOnly fechaSalida,
        IList<int> unidadIds,
        CancellationToken cancellationToken)
    {
        // No filtrar por numeroPersonas: el SP exige capacidad >= total en cada unidad,
        // pero en reservas multi-unidad la capacidad se valida por unidad por separado.
        var disponibilidad = await _disponibilidadService.ConsultarDisponibilidadAsync(
            fechaEntrada,
            fechaSalida,
            sitioId,
            numeroPersonas: null,
            cancellationToken);

        var disponibles = disponibilidad.Unidades
            .Select(u => u.UnidadAlojamientoId)
            .ToHashSet();

        var noDisponibles = unidadIds.Where(id => !disponibles.Contains(id)).ToList();
        if (noDisponibles.Count > 0)
        {
            throw new ArgumentException(
                $"Las unidades [{string.Join(", ", noDisponibles)}] no están disponibles en las fechas indicadas.");
        }
    }

    private async Task<string> GenerarCodigoReservaAsync(CancellationToken cancellationToken)
    {
        var hoy = DateTime.Today;
        var conteo = await _context.Reservas.CountAsync(
            r => r.FechaCreacion >= hoy && r.FechaCreacion < hoy.AddDays(1),
            cancellationToken);

        return $"RES-{hoy:yyyyMMdd}-{(conteo + 1):D4}";
    }

    private async Task<ReservaCreadaResponseDto> MapearRespuestaAsync(
        int reservaId,
        CancellationToken cancellationToken)
    {
        var reserva = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Sitio)
            .Include(r => r.Unidades).ThenInclude(u => u.UnidadAlojamiento)
            .Include(r => r.Acompanantes)
            .Include(r => r.Servicios).ThenInclude(s => s.TipoServicio)
            .FirstAsync(r => r.ReservaId == reservaId, cancellationToken);

        return new ReservaCreadaResponseDto
        {
            ReservaId = reserva.ReservaId,
            CodigoReserva = reserva.CodigoReserva,
            TipoReserva = reserva.TipoReserva,
            Estado = reserva.Estado,
            SitioId = reserva.SitioId,
            SitioNombre = reserva.Sitio.Nombre,
            AsociadoId = reserva.AsociadoId,
            FechaEntrada = reserva.FechaEntrada,
            FechaSalida = reserva.FechaSalida,
            NumeroNoches = reserva.NumeroNoches,
            NumeroPersonas = reserva.NumeroPersonas,
            NumeroUnidadesSolicitadas = reserva.NumeroUnidadesSolicitadas,
            Subtotal = reserva.Subtotal,
            TotalServicios = reserva.TotalServicios,
            Total = reserva.Total,
            Unidades = reserva.Unidades.Select(u => new ReservaUnidadResponseDto
            {
                UnidadAlojamientoId = u.UnidadAlojamientoId,
                Codigo = u.UnidadAlojamiento.Codigo,
                Nombre = u.UnidadAlojamiento.Nombre,
                PrecioNoche = u.PrecioNoche,
                Subtotal = u.Subtotal
            }).ToList(),
            Acompanantes = reserva.Acompanantes
                .OrderBy(a => a.Orden)
                .Select(a => new ReservaAcompananteResponseDto
                {
                    Orden = a.Orden,
                    Nombres = a.Nombres,
                    Apellidos = a.Apellidos,
                    TarifaAplicada = a.TarifaAplicada
                }).ToList(),
            Servicios = reserva.Servicios.Select(s => new ReservaServicioResponseDto
            {
                TipoServicioId = s.TipoServicioId,
                Nombre = s.TipoServicio.Nombre,
                Cantidad = s.Cantidad,
                PrecioUnitario = s.PrecioUnitario,
                Subtotal = s.Subtotal
            }).ToList()
        };
    }

    private static int DistribuirPersonas(int totalPersonas, int totalUnidades, int unidadesGrupo) =>
        (totalPersonas * unidadesGrupo + totalUnidades - 1) / totalUnidades;

    private static string NormalizarTipoReserva(string tipoReserva)
    {
        if (string.Equals(tipoReserva, TipoVisitaDia, StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipoReserva, "VisitaDia", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipoReserva, "Visita Dia", StringComparison.OrdinalIgnoreCase))
        {
            return TipoVisitaDia;
        }

        if (string.Equals(tipoReserva, TipoAlojamiento, StringComparison.OrdinalIgnoreCase))
        {
            return TipoAlojamiento;
        }

        throw new ArgumentException("El tipo de reserva debe ser 'Alojamiento' o 'VisitaDia'.");
    }

    private static void ValidarSolicitud(CrearReservaRequest request)
    {
        if (request.AsociadoId < 1)
        {
            throw new ArgumentException("El asociadoId es obligatorio.");
        }

        if (request.SitioId < 1)
        {
            throw new ArgumentException("El sitioId es obligatorio.");
        }

        if (request.FechaEntrada == default)
        {
            throw new ArgumentException("La fechaEntrada es obligatoria.");
        }

        if (request.FechaSalida == default)
        {
            throw new ArgumentException("La fechaSalida es obligatoria.");
        }

        if (request.NumeroPersonas < 1)
        {
            throw new ArgumentException("El número de personas debe ser mayor o igual a 1.");
        }

        if (string.IsNullOrWhiteSpace(request.TipoReserva))
        {
            throw new ArgumentException("El tipo de reserva es obligatorio.");
        }
    }
}
