using Chassoft.Dominio.Personas;
using System.Collections.Generic;
namespace Chassoft.Dominio.Repositories
{
    public interface IProveedorRepository
    {
        Proveedor? ObtenerPorId(int id);
        Proveedor? ObtenerPorCuit(string cuit);
        IEnumerable<Proveedor> ObtenerTodos(bool? activo = null);
        Proveedor Agregar(Proveedor proveedor);
        bool Actualizar(Proveedor proveedor);
        bool DarDeBaja(int id);
        bool Reactivar(int id);
    }
}
