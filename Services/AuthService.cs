using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FondoXYZ.Data;
using FondoXYZ.Data.Entities;
using FondoXYZ.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FondoXYZ.Services;

public class AuthService : IAuthService
{
    private readonly FondoXYZDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Asociado> _passwordHasher = new();

    public AuthService(FondoXYZDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarSolicitud(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var asociado = await _context.Asociados
            .FirstOrDefaultAsync(a => a.Email == email && a.Activo, cancellationToken)
            ?? throw new ArgumentException("Credenciales inválidas.");

        if (!await VerificarClaveAsync(asociado, request.Clave, cancellationToken))
        {
            throw new ArgumentException("Credenciales inválidas.");
        }

        var expiraEn = DateTime.UtcNow.AddMinutes(ObtenerMinutosExpiracion());

        return new LoginResponseDto
        {
            Token = GenerarToken(asociado, expiraEn),
            ExpiraEn = expiraEn,
            Asociado = MapearAsociado(asociado)
        };
    }

    private async Task<bool> VerificarClaveAsync(
        Asociado asociado,
        string clave,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(asociado.Clave))
        {
            return false;
        }

        PasswordVerificationResult resultado;

        try
        {
            resultado = _passwordHasher.VerifyHashedPassword(asociado, asociado.Clave, clave);
        }
        catch (FormatException)
        {
            resultado = PasswordVerificationResult.Failed;
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            asociado.Clave = _passwordHasher.HashPassword(asociado, clave);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (resultado == PasswordVerificationResult.Success)
        {
            return true;
        }

        // Compatibilidad con claves antiguas guardadas en texto plano.
        if (asociado.Clave == clave)
        {
            asociado.Clave = _passwordHasher.HashPassword(asociado, clave);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    private string GenerarToken(Asociado asociado, DateTime expiraEn)
    {
        var issuer = ObtenerConfiguracionJwt("Issuer");
        var audience = ObtenerConfiguracionJwt("Audience");
        var key = ObtenerConfiguracionJwt("Key");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, asociado.AsociadoId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, asociado.Email),
            new Claim(ClaimTypes.NameIdentifier, asociado.AsociadoId.ToString()),
            new Claim(ClaimTypes.Name, $"{asociado.Nombres} {asociado.Apellidos}".Trim()),
            new Claim("numeroAsociado", asociado.NumeroAsociado)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string ObtenerConfiguracionJwt(string nombre)
    {
        var valor = _configuration[$"Jwt:{nombre}"];
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException($"La configuración Jwt:{nombre} es obligatoria.");
        }

        return valor;
    }

    private int ObtenerMinutosExpiracion()
    {
        var valor = _configuration["Jwt:ExpirationMinutes"];
        return int.TryParse(valor, out var minutos) && minutos > 0 ? minutos : 120;
    }

    private static void ValidarSolicitud(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("El email es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Clave))
        {
            throw new ArgumentException("La clave es obligatoria.");
        }
    }

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
