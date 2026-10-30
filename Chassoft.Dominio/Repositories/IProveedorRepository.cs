using Chassoft.Dominio.Personas;
using System.Collections.Generic;

namespace Chassoft.Dominio.Repositories
{
    public interface IProveedorRepository
    {
        Proveedor? ObtenerPorId(int id);
        Proveedor? ObtenerPorCuit(string cuit);
        IEnumerable<Proveedor> ObtenerTodos();

        Proveedor Agregar(Proveedor proveedor);
        void Actualizar(Proveedor proveedor);
        void Eliminar(int id);
    }
}
