using System.ComponentModel.DataAnnotations;

namespace Chassoft.Web.Models;

public class ProveedorDto
{
    public int     Id                  { get; set; }
    public bool Activo { get; set; } = true;
    public string  Nombre              { get; set; } = string.Empty;
    public string  Cuit                { get; set; } = string.Empty;
    public string? RazonSocial         { get; set; }
    public string? TelefonoDeContacto  { get; set; }
    public string? CorreoDeContacto    { get; set; }
    public string? CondicionIVA        { get; set; }
    public string? CbuAsociado         { get; set; }
}

public class CrearProveedorRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CUIT es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string Cuit { get; set; } = string.Empty;

    [StringLength(200)] public string? RazonSocial        { get; set; }
    [StringLength(50)]  public string? TelefonoDeContacto { get; set; }
    [EmailAddress][StringLength(254)] public string? CorreoDeContacto { get; set; }
    [StringLength(50)]  public string? CondicionIVA        { get; set; }
    [RegularExpression(@"^[0-9]{22}$", ErrorMessage = "El CBU debe tener 22 dígitos.")] public string? CbuAsociado         { get; set; }
}
