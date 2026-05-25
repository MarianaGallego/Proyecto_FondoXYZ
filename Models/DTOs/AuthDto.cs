namespace FondoXYZ.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public AsociadoDto Asociado { get; set; } = new();
}
