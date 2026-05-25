using FondoXYZ.Data;
using FondoXYZ.Data.Entities;
using FondoXYZ.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Services;

public class PagoService : IPagoService
{
    private const string EstadoPagoIniciado = "Iniciado";
    private const string EstadoPagoAprobado = "Aprobado";
    private const string EstadoPagoRechazado = "Rechazado";
    private const string EstadoPagoReembolsado = "Reembolsado";

    private const string EstadoReservaPendientePago = "PendientePago";
    private const string EstadoReservaConfirmada = "Confirmada";

    private readonly FondoXYZDbContext _context;

    public PagoService(FondoXYZDbContext context)
    {
        _context = context;
    }

    public async Task<PagoReservaResponseDto> RegistrarPagoAsync(
        int reservaId,
        RegistrarPagoRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitud(reservaId, request);

        var estadoPago = NormalizarEstadoPago(request.EstadoPago);

        var reserva = await _context.Reservas
            .Include(r => r.Pagos)
            .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken)
            ?? throw new ArgumentException($"No se encontró la reserva con id {reservaId}.");

        ValidarReservaParaPago(reserva, request.Monto, estadoPago);

        await using var transaccion = await _context.Database.BeginTransactionAsync(cancellationToken);

        var pago = new Pago
        {
            ReservaId = reserva.ReservaId,
            Monto = request.Monto,
            MetodoPago = request.MetodoPago.Trim(),
            Estado = estadoPago,
            FechaCreacion = DateTime.Now,
            FechaPago = estadoPago is EstadoPagoAprobado or EstadoPagoRechazado or EstadoPagoReembolsado
                ? DateTime.Now
                : null
        };

        reserva.Estado = ObtenerNuevoEstadoReserva(reserva.Estado, estadoPago);
        reserva.FechaConfirmacion = estadoPago == EstadoPagoAprobado
            ? DateTime.Now
            : reserva.FechaConfirmacion;

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        return new PagoReservaResponseDto
        {
            PagoId = pago.PagoId,
            ReservaId = reserva.ReservaId,
            CodigoReserva = reserva.CodigoReserva,
            Monto = pago.Monto,
            MetodoPago = pago.MetodoPago,
            EstadoPago = pago.Estado,
            EstadoReserva = reserva.Estado,
            FechaPago = pago.FechaPago,
            FechaCreacion = pago.FechaCreacion
        };
    }

    private static void ValidarSolicitud(int reservaId, RegistrarPagoRequest request)
    {
        if (reservaId < 1)
        {
            throw new ArgumentException("El reservaId debe ser mayor o igual a 1.");
        }

        if (request.Monto <= 0)
        {
            throw new ArgumentException("El monto del pago debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(request.MetodoPago))
        {
            throw new ArgumentException("El método de pago es obligatorio.");
        }
    }

    private static void ValidarReservaParaPago(Reserva reserva, decimal monto, string estadoPago)
    {
        if (reserva.Estado is "Cancelada" or "Expirada")
        {
            throw new ArgumentException($"No se puede pagar una reserva en estado {reserva.Estado}.");
        }

        if (estadoPago == EstadoPagoAprobado && reserva.Estado == EstadoReservaConfirmada)
        {
            throw new ArgumentException("La reserva ya se encuentra confirmada.");
        }

        var tienePagoAprobado = reserva.Pagos.Any(p => p.Estado == EstadoPagoAprobado);
        if (tienePagoAprobado && estadoPago != EstadoPagoReembolsado)
        {
            throw new ArgumentException("La reserva ya tiene un pago aprobado.");
        }

        if (estadoPago != EstadoPagoReembolsado && monto != reserva.Total)
        {
            throw new ArgumentException(
                $"El monto del pago ({monto:C}) debe coincidir con el total de la reserva ({reserva.Total:C}).");
        }
    }

    private static string ObtenerNuevoEstadoReserva(string estadoActual, string estadoPago) =>
        estadoPago switch
        {
            EstadoPagoAprobado => EstadoReservaConfirmada,
            EstadoPagoRechazado => EstadoReservaPendientePago,
            EstadoPagoIniciado => EstadoReservaPendientePago,
            EstadoPagoReembolsado => estadoActual,
            _ => estadoActual
        };

    private static string NormalizarEstadoPago(string estadoPago)
    {
        if (string.IsNullOrWhiteSpace(estadoPago))
        {
            return EstadoPagoIniciado;
        }

        if (string.Equals(estadoPago, EstadoPagoIniciado, StringComparison.OrdinalIgnoreCase))
        {
            return EstadoPagoIniciado;
        }

        if (string.Equals(estadoPago, EstadoPagoAprobado, StringComparison.OrdinalIgnoreCase))
        {
            return EstadoPagoAprobado;
        }

        if (string.Equals(estadoPago, EstadoPagoRechazado, StringComparison.OrdinalIgnoreCase))
        {
            return EstadoPagoRechazado;
        }

        if (string.Equals(estadoPago, EstadoPagoReembolsado, StringComparison.OrdinalIgnoreCase))
        {
            return EstadoPagoReembolsado;
        }

        throw new ArgumentException("El estadoPago debe ser Iniciado, Aprobado, Rechazado o Reembolsado.");
    }
}
