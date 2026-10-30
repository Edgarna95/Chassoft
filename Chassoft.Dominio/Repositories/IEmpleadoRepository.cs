using System.Collections.Generic;
namespace Chassoft.Dominio.Repositories
{
    public sealed class EmpleadoDisponible
    {
        public int Id { get; }
        public string NombreCompleto { get; }
        public EmpleadoDisponible(int id, string nombre) { Id = id; NombreCompleto = nombre; }
    }
    public interface IEmpleadoRepository
    {
        IEnumerable<EmpleadoDisponible> ObtenerActivos();
    }
}
