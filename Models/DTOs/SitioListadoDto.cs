namespace FondoXYZ.Models.DTOs;

public class SitioListadoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int CupoMaximo { get; set; }
    public string DescripcionResumida { get; set; } = string.Empty;
}
