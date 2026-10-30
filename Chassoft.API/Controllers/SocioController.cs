using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Chassoft.API.Controllers
{
    public class CrearSocioRequest
    {
        [Required, StringLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Dni { get; set; } = string.Empty;
        [EmailAddress, StringLength(254)] public string? Email { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SocioController : ControllerBase
    {
        private readonly ISocioRepository _repo;
        private readonly IPagosCuotaRepository _pagos;

        public SocioController(ISocioRepository repo, IPagosCuotaRepository pagos)
        {
            _repo = repo;
            _pagos = pagos;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var socios = _repo.ObtenerTodos();
            return Ok(socios);
        }

        [HttpGet("{id:int}/pagos")]
        public IActionResult Pagos(int id) => _repo.ObtenerPorId(id) == null ? NotFound() : Ok(_pagos.ObtenerPorSocio(id));

        [HttpPost("{id:int}/baja")]
        public IActionResult DarDeBaja(int id)
        {
            try { _repo.DarDeBaja(id); return NoContent(); }
            catch (ArgumentException ex) { return NotFound(new { mensaje = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Crear([FromBody] CrearSocioRequest request)
        {
            Socio nuevo;
            try
            {
                nuevo = new Socio(request.NombreCompleto, request.Dni, request.Email);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            try
            {
                _repo.Agregar(nuevo);
            }
            catch (ArgumentException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }

            return Ok(new { mensaje = "Socio registrado correctamente" });
        }
    }
}
