using System;
using System.ComponentModel.DataAnnotations;

namespace Chassoft.Web.Models;

public class SocioDto
{
    public int      Id             { get; set; }
    public string   NombreCompleto { get; set; } = string.Empty;
    public string   Dni            { get; set; } = string.Empty;
    public DateTime FechaAlta      { get; set; }
    public bool     Activo         { get; set; }
    public string?  Email          { get; set; }
}

public class CrearSocioRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string Dni { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido"), StringLength(254)]
    public string? Email { get; set; }
}

/// <summary>Estado de cuota de un socio para el mes en curso.</summary>
public enum EstadoCuota
{
    AlDia,          // Pagó el mes en curso
    Vencida,        // No pagó el mes en curso (1 mes de mora)
    Moroso          // No pagó el mes en curso ni el anterior (2+ meses)
}

/// <summary>Vista enriquecida del socio con su estado de cuota del mes.</summary>
public class SocioConCuotaDto
{
    public SocioDto     Socio       { get; set; } = new();
    public EstadoCuota  Estado      { get; set; }
    public string       EtiquetaEstado => Estado switch
    {
        EstadoCuota.AlDia   => "Al día",
        EstadoCuota.Vencida => "Vencida",
        EstadoCuota.Moroso  => "Moroso",
        _                   => "—"
    };
    public string CssEstado => Estado switch
    {
        EstadoCuota.AlDia   => "bg-success",
        EstadoCuota.Vencida => "bg-warning text-dark",
        EstadoCuota.Moroso  => "bg-danger",
        _                   => "bg-secondary"
    };
}
