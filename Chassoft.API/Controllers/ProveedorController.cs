using System;
using System.ComponentModel.DataAnnotations;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Chassoft.API.Controllers
{
    public class CrearProveedorRequest
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Cuit { get; set; } = string.Empty;
        [StringLength(200)] public string? RazonSocial { get; set; }
        [StringLength(50)] public string? TelefonoDeContacto { get; set; }
        [EmailAddress, StringLength(254)] public string? CorreoDeContacto { get; set; }
        [StringLength(50)] public string? CondicionIVA { get; set; }
        [RegularExpression(@"^[0-9]{22}$", ErrorMessage = "El CBU debe tener 22 dígitos.")]
        public string? CbuAsociado { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorRepository _repo;
        public ProveedorController(IProveedorRepository repo) { _repo = repo; }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.ObtenerTodos());

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var proveedor = _repo.ObtenerPorId(id);
            return proveedor == null ? NotFound() : Ok(proveedor);
        }

        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] CrearProveedorRequest request)
        {
            if (_repo.ObtenerPorId(id) == null) return NotFound();
            try
            {
                _repo.Actualizar(new Proveedor(id, request.Nombre, request.Cuit, request.RazonSocial,
                    request.TelefonoDeContacto, request.CorreoDeContacto, request.CondicionIVA, request.CbuAsociado));
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Crear([FromBody] CrearProveedorRequest request)
        {
            Proveedor nuevo;
            try { nuevo = new Proveedor(request.Nombre, request.Cuit, request.RazonSocial, request.TelefonoDeContacto, request.CorreoDeContacto, request.CondicionIVA, request.CbuAsociado); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
            var guardado = _repo.Agregar(nuevo);
            return CreatedAtAction(nameof(GetById), new { id = guardado.Id }, guardado);
        }
    }
}
