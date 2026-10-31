using System;
using System.ComponentModel.DataAnnotations;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;
namespace Chassoft.API.Controllers
{
    public class RegistrarIngresoRequest
    {
        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Monto { get; set; }
        [Range(1, int.MaxValue)] public int IdEmpleado { get; set; }
        [EnumDataType(typeof(TipoIngreso))] public int Tipo { get; set; }
        public int? IdSocio { get; set; }
    }
    public class RegistrarEgresoRequest
    {
        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Monto { get; set; }
        [Range(1, int.MaxValue)] public int IdEmpleado { get; set; }
        [EnumDataType(typeof(TipoEgreso))] public int Tipo { get; set; }
        [Range(1, int.MaxValue)] public int IdProveedor { get; set; }
    }
    public class PeriodoCuotaRequest
    {
        [Range(2000, 9999)] public int Anio { get; set; }
        [Range(1, 12)] public int Mes { get; set; }
    }
    public class CobrarCuotasRequest
    {
        [Range(1, int.MaxValue)] public int IdSocio { get; set; }
        [Range(1, int.MaxValue)] public int IdEmpleado { get; set; }
        [Required, MinLength(1)] public List<PeriodoCuotaRequest> Periodos { get; set; } = new();
    }
    [ApiController, Route("api/[controller]")]
    public class OperacionController : ControllerBase
    {
        private readonly IOperacionRepository _repo;
        private readonly ISocioRepository _socios;
        private readonly IProveedorRepository _proveedores;
        private readonly IConfiguration _config;
        public OperacionController(IOperacionRepository repo, ISocioRepository socios,
            IProveedorRepository proveedores, IConfiguration config)
        { _repo = repo; _socios = socios; _proveedores = proveedores; _config = config; }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.ObtenerTodas().Select(o => new {
            o.Id, o.Fecha, o.Monto, o.IdEmpleado,
            TipoOperacion = o is Ingreso ? "Ingreso" : "Egreso",
            Tipo = o is Ingreso i ? i.Tipo.ToString() : ((Egreso)o).Tipo.ToString(),
            IdSocio = (o as Ingreso)?.Socio?.Id,
            IdProveedor = (o as Egreso)?.Proveedor.Id
        }));

        [HttpGet("tarifa-cuota")]
        public IActionResult Tarifa() => Ok(new { importeMensual = ImporteMensual(), moneda = "ARS", bonificacionesDisponibles = false });
        private decimal ImporteMensual() => _config.GetValue<decimal>("Cuotas:ImporteMensual");

        [HttpPost("ingreso")]
        public IActionResult RegistrarIngreso(RegistrarIngresoRequest req)
        {
            if (req.Tipo == (int)TipoIngreso.Cuota)
                return BadRequest(new { mensaje = "Las cuotas se cobran desde /api/operacion/cuotas, indicando todos los meses." });
            try
            {
                var socio = req.IdSocio.HasValue ? _socios.ObtenerPorId(req.IdSocio.Value) : null;
                if (req.IdSocio.HasValue && socio == null) return NotFound(new { mensaje = "Socio no encontrado." });
                _repo.Agregar(new Ingreso(req.Monto, req.IdEmpleado, (TipoIngreso)req.Tipo, socio));
                return Ok(new { mensaje = "Ingreso registrado." });
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }

        [HttpPost("egreso")]
        public IActionResult RegistrarEgreso(RegistrarEgresoRequest req)
        {
            var proveedor = _proveedores.ObtenerPorId(req.IdProveedor);
            if (proveedor == null) return NotFound(new { mensaje = "Proveedor no encontrado." });
            if (!proveedor.Activo) return Conflict(new { mensaje = "El proveedor está inactivo. Reactivalo antes de registrar un pago." });
            try
            {
                _repo.Agregar(new Egreso(req.Monto, req.IdEmpleado, proveedor, (TipoEgreso)req.Tipo));
                return Ok(new { mensaje = "Egreso registrado." });
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }

        [HttpPost("cuotas")]
        public IActionResult CobrarCuotas(CobrarCuotasRequest req)
        {
            var socio = _socios.ObtenerPorId(req.IdSocio);
            if (socio == null) return NotFound(new { mensaje = "Socio no encontrado." });
            try
            {
                var cobro = new CobroCuotas(socio, req.IdEmpleado, ImporteMensual(),
                    req.Periodos.Select(p => p == null ? throw new ArgumentException("Período inválido.") : new PeriodoCuota(p.Anio, p.Mes)));
                var id = _repo.CobrarCuotas(cobro);
                return Ok(new { idOperacion = id, total = cobro.Total, mensaje = "Cuotas e ingreso registrados juntos." });
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (CuotasYaPagadasException ex) { return Conflict(new { mensaje = ex.Message }); }
        }
    }
}
