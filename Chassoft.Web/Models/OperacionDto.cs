namespace Chassoft.Web.Models;
public class OperacionDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string TipoOperacion { get; set; } = "";
    public string Tipo { get; set; } = "";
}
public class EmpleadoDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
}
public class TarifaCuotaDto { public decimal ImporteMensual { get; set; } }
public class PeriodoCuotaDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
}
