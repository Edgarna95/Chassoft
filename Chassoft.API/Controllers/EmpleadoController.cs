using Chassoft.Dominio.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace Chassoft.API.Controllers
{
    [ApiController, Route("api/empleado")]
    public class EmpleadoController : ControllerBase
    {
        private readonly IEmpleadoRepository _repo;
        public EmpleadoController(IEmpleadoRepository repo) => _repo = repo;
        [HttpGet] public IActionResult ObtenerActivos() => Ok(_repo.ObtenerActivos());
    }
}
