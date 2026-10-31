using System;
using System.ComponentModel.DataAnnotations;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Excepciones;
using Microsoft.AspNetCore.Mvc;
namespace Chassoft.API.Controllers
{
    public class CrearProveedorRequest
    {
        [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(20)] public string Cuit { get; set; } = string.Empty;
        [StringLength(200)] public string? RazonSocial { get; set; }
        [StringLength(50)] public string? TelefonoDeContacto { get; set; }
        [EmailAddress, StringLength(254)] public string? CorreoDeContacto { get; set; }
        [StringLength(50)] public string? CondicionIVA { get; set; }
        [RegularExpression(@"^[0-9]{22}$", ErrorMessage = "El CBU debe tener 22 dígitos.")] public string? CbuAsociado { get; set; }
    }
    [ApiController, Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorRepository _repo;
        public ProveedorController(IProveedorRepository repo) => _repo = repo;

        [HttpGet, ProducesResponseType(typeof(IEnumerable<Proveedor>), StatusCodes.Status200OK)]
        public IActionResult GetAll([FromQuery] bool? activo = null) => Ok(_repo.ObtenerTodos(activo));

        [HttpGet("{id:int}"), ProducesResponseType(typeof(Proveedor), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById([Range(1, int.MaxValue)] int id)
        {
            var proveedor = _repo.ObtenerPorId(id);
            return proveedor == null ? NotFound() : Ok(proveedor);
        }
        [HttpGet("por-cuit")]
        public IActionResult GetByCuit([FromQuery, Required, StringLength(20)] string cuit)
        {
            var proveedor = _repo.ObtenerPorCuit(cuit);
            return proveedor == null ? NotFound() : Ok(proveedor);
        }
        [HttpPost, ProducesResponseType(typeof(Proveedor), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Crear([FromBody] CrearProveedorRequest request)
        {
            try
            {
                var nuevo = new Proveedor(request.Nombre, request.Cuit, request.RazonSocial, request.TelefonoDeContacto,
                    request.CorreoDeContacto, request.CondicionIVA, request.CbuAsociado);
                var guardado = _repo.Agregar(nuevo);
                return CreatedAtAction(nameof(GetById), new { id = guardado.Id }, guardado);
            }
            catch (RegistroDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }
        [HttpPut("{id:int}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Actualizar([Range(1, int.MaxValue)] int id, [FromBody] CrearProveedorRequest request)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null) return NotFound();
            try
            {
                var actualizado = new Proveedor(id, request.Nombre, request.Cuit, request.RazonSocial, request.TelefonoDeContacto,
                    request.CorreoDeContacto, request.CondicionIVA, request.CbuAsociado, existente.Activo);
                return _repo.Actualizar(actualizado) ? NoContent() : NotFound();
            }
            catch (RegistroDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DarDeBaja([Range(1, int.MaxValue)] int id) => _repo.DarDeBaja(id) ? NoContent() : NotFound();

        [HttpPost("{id:int}/reactivar")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Reactivar([Range(1, int.MaxValue)] int id) => _repo.Reactivar(id) ? NoContent() : NotFound();
    }
}
