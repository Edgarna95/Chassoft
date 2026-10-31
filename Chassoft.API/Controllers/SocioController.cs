using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Chassoft.Dominio.Excepciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace Chassoft.API.Controllers
{
    public class CrearSocioRequest
    {
        [Required, StringLength(200)] public string NombreCompleto { get; set; } = string.Empty;
        [Required, StringLength(20)] public string Dni { get; set; } = string.Empty;
        [EmailAddress, StringLength(254)] public string? Email { get; set; }
    }
    [ApiController, Route("api/[controller]")]
    public class SocioController : ControllerBase
    {
        private readonly ISocioRepository _repo;
        private readonly IPagosCuotaRepository _pagos;
        public SocioController(ISocioRepository repo, IPagosCuotaRepository pagos) { _repo = repo; _pagos = pagos; }

        [HttpGet, ProducesResponseType(typeof(IEnumerable<Socio>), StatusCodes.Status200OK)]
        public IActionResult GetAll([FromQuery] bool? activo = null) => Ok(_repo.ObtenerTodos(activo));

        [HttpGet("{id:int}"), ProducesResponseType(typeof(Socio), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById([Range(1, int.MaxValue)] int id)
        {
            var socio = _repo.ObtenerPorId(id);
            return socio == null ? NotFound() : Ok(socio);
        }
        [HttpGet("por-dni")]
        public IActionResult GetByDni([FromQuery, Required, StringLength(20)] string dni)
        {
            var socio = _repo.ObtenerPorDni(dni);
            return socio == null ? NotFound() : Ok(socio);
        }
        [HttpGet("{id:int}/pagos")]
        public IActionResult Pagos([Range(1, int.MaxValue)] int id) => _repo.ObtenerPorId(id) == null ? NotFound() : Ok(_pagos.ObtenerPorSocio(id));

        [HttpPost, ProducesResponseType(typeof(Socio), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Crear([FromBody] CrearSocioRequest request)
        {
            try
            {
                var guardado = _repo.Agregar(new Socio(request.NombreCompleto, request.Dni, request.Email));
                return CreatedAtAction(nameof(GetById), new { id = guardado.Id }, guardado);
            }
            catch (RegistroDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }
        [HttpPut("{id:int}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Actualizar([Range(1, int.MaxValue)] int id, [FromBody] CrearSocioRequest request)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null) return NotFound();
            try
            {
                var actualizado = new Socio(id, request.NombreCompleto, request.Dni, existente.FechaAlta, existente.Activo, request.Email);
                return _repo.Actualizar(actualizado) ? NoContent() : NotFound();
            }
            catch (RegistroDuplicadoException ex) { return Conflict(new { mensaje = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        }
        [HttpDelete("{id:int}"), HttpPost("{id:int}/baja")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DarDeBaja([Range(1, int.MaxValue)] int id) => _repo.DarDeBaja(id) ? NoContent() : NotFound();

        [HttpPost("{id:int}/reactivar")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Reactivar([Range(1, int.MaxValue)] int id) => _repo.Reactivar(id) ? NoContent() : NotFound();
    }
}
