using System.Net.Mail;
using FondoXYZ.Data;
using FondoXYZ.Data.Entities;
using FondoXYZ.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Services;

public class AsociadoService : IAsociadoService
{
    private readonly FondoXYZDbContext _context;
    private readonly PasswordHasher<Asociado> _passwordHasher = new();

    public AsociadoService(FondoXYZDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AsociadoDto>> ListarAsociadosAsync(
        bool incluirInactivos = false,
        CancellationToken cancellationToken = default)
    {
        return await _context.Asociados
            .AsNoTracking()
            .Where(a => incluirInactivos || a.Activo)
            .OrderBy(a => a.Apellidos)
            .ThenBy(a => a.Nombres)
            .Select(a => MapearAsociado(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<AsociadoDto?> ObtenerAsociadoPorIdAsync(
        int asociadoId,
        CancellationToken cancellationToken = default)
    {
        if (asociadoId < 1)
        {
            throw new ArgumentException("El asociadoId debe ser mayor o igual a 1.");
        }

        return await _context.Asociados
            .AsNoTracking()
            .Where(a => a.AsociadoId == asociadoId)
            .Select(a => MapearAsociado(a))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AsociadoDto> CrearAsociadoAsync(
        CrearAsociadoRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitud(request);
        await ValidarDuplicadosAsync(
            request.NumeroAsociado,
            request.NumeroDocumento,
            request.Email,
            asociadoIdActual: null,
            cancellationToken);

        var asociado = new Asociado
        {
            NumeroAsociado = request.NumeroAsociado.Trim(),
            TipoDocumento = request.TipoDocumento.Trim().ToUpperInvariant(),
            NumeroDocumento = request.NumeroDocumento.Trim(),
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Telefono = NormalizarTextoOpcional(request.Telefono),
            Activo = true,
            FechaRegistro = DateTime.Now
        };

        if (!string.IsNullOrWhiteSpace(request.Clave))
        {
            asociado.Clave = _passwordHasher.HashPassword(asociado, request.Clave);
        }

        _context.Asociados.Add(asociado);
        await _context.SaveChangesAsync(cancellationToken);

        return MapearAsociado(asociado);
    }

    public async Task<AsociadoDto?> ActualizarAsociadoAsync(
        int asociadoId,
        ActualizarAsociadoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (asociadoId < 1)
        {
            throw new ArgumentException("El asociadoId debe ser mayor o igual a 1.");
        }

        ValidarSolicitud(request);

        var asociado = await _context.Asociados
            .FirstOrDefaultAsync(a => a.AsociadoId == asociadoId, cancellationToken);

        if (asociado is null)
        {
            return null;
        }

        await ValidarDuplicadosAsync(
            request.NumeroAsociado,
            request.NumeroDocumento,
            request.Email,
            asociadoId,
            cancellationToken);

        asociado.NumeroAsociado = request.NumeroAsociado.Trim();
        asociado.TipoDocumento = request.TipoDocumento.Trim().ToUpperInvariant();
        asociado.NumeroDocumento = request.NumeroDocumento.Trim();
        asociado.Nombres = request.Nombres.Trim();
        asociado.Apellidos = request.Apellidos.Trim();
        asociado.Email = request.Email.Trim().ToLowerInvariant();
        asociado.Telefono = NormalizarTextoOpcional(request.Telefono);
        asociado.Activo = request.Activo;

        if (!string.IsNullOrWhiteSpace(request.Clave))
        {
            asociado.Clave = _passwordHasher.HashPassword(asociado, request.Clave);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapearAsociado(asociado);
    }

    public async Task<bool> EliminarAsociadoAsync(
        int asociadoId,
        CancellationToken cancellationToken = default)
    {
        if (asociadoId < 1)
        {
            throw new ArgumentException("El asociadoId debe ser mayor o igual a 1.");
        }

        var asociado = await _context.Asociados
            .FirstOrDefaultAsync(a => a.AsociadoId == asociadoId && a.Activo, cancellationToken);

        if (asociado is null)
        {
            return false;
        }

        asociado.Activo = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidarDuplicadosAsync(
        string numeroAsociado,
        string numeroDocumento,
        string email,
        int? asociadoIdActual,
        CancellationToken cancellationToken)
    {
        var numeroAsociadoNormalizado = numeroAsociado.Trim();
        var numeroDocumentoNormalizado = numeroDocumento.Trim();
        var emailNormalizado = email.Trim().ToLowerInvariant();

        var existeNumeroAsociado = await _context.Asociados.AnyAsync(
            a => a.NumeroAsociado == numeroAsociadoNormalizado
                && (!asociadoIdActual.HasValue || a.AsociadoId != asociadoIdActual.Value),
            cancellationToken);

        if (existeNumeroAsociado)
        {
            throw new ArgumentException("Ya existe un asociado con ese número de asociado.");
        }

        var existeDocumento = await _context.Asociados.AnyAsync(
            a => a.NumeroDocumento == numeroDocumentoNormalizado
                && (!asociadoIdActual.HasValue || a.AsociadoId != asociadoIdActual.Value),
            cancellationToken);

        if (existeDocumento)
        {
            throw new ArgumentException("Ya existe un asociado con ese número de documento.");
        }

        var existeEmail = await _context.Asociados.AnyAsync(
            a => a.Email == emailNormalizado
                && (!asociadoIdActual.HasValue || a.AsociadoId != asociadoIdActual.Value),
            cancellationToken);

        if (existeEmail)
        {
            throw new ArgumentException("Ya existe un asociado con ese correo electrónico.");
        }
    }

    private static void ValidarSolicitud(CrearAsociadoRequest request)
    {
        ValidarCamposBase(
            request.NumeroAsociado,
            request.TipoDocumento,
            request.NumeroDocumento,
            request.Nombres,
            request.Apellidos,
            request.Email,
            request.Telefono);
    }

    private static void ValidarSolicitud(ActualizarAsociadoRequest request)
    {
        ValidarCamposBase(
            request.NumeroAsociado,
            request.TipoDocumento,
            request.NumeroDocumento,
            request.Nombres,
            request.Apellidos,
            request.Email,
            request.Telefono);
    }

    private static void ValidarCamposBase(
        string numeroAsociado,
        string tipoDocumento,
        string numeroDocumento,
        string nombres,
        string apellidos,
        string email,
        string? telefono)
    {
        ValidarTextoObligatorio(numeroAsociado, "El número de asociado es obligatorio.", 20);
        ValidarTextoObligatorio(tipoDocumento, "El tipo de documento es obligatorio.", 10);
        ValidarTextoObligatorio(numeroDocumento, "El número de documento es obligatorio.", 20);
        ValidarTextoObligatorio(nombres, "Los nombres son obligatorios.", 100);
        ValidarTextoObligatorio(apellidos, "Los apellidos son obligatorios.", 100);
        ValidarTextoObligatorio(email, "El correo electrónico es obligatorio.", 150);

        if (!EsEmailValido(email))
        {
            throw new ArgumentException("El correo electrónico no tiene un formato válido.");
        }

        if (!string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length > 20)
        {
            throw new ArgumentException("El teléfono no puede superar 20 caracteres.");
        }
    }

    private static void ValidarTextoObligatorio(string valor, string mensaje, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(mensaje);
        }

        if (valor.Trim().Length > longitudMaxima)
        {
            throw new ArgumentException($"El campo supera la longitud máxima de {longitudMaxima} caracteres.");
        }
    }

    private static bool EsEmailValido(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string? NormalizarTextoOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static AsociadoDto MapearAsociado(Asociado asociado) =>
        new()
        {
            AsociadoId = asociado.AsociadoId,
            NumeroAsociado = asociado.NumeroAsociado,
            TipoDocumento = asociado.TipoDocumento,
            NumeroDocumento = asociado.NumeroDocumento,
            Nombres = asociado.Nombres,
            Apellidos = asociado.Apellidos,
            Email = asociado.Email,
            Telefono = asociado.Telefono,
            Activo = asociado.Activo,
            FechaRegistro = asociado.FechaRegistro
        };
}
