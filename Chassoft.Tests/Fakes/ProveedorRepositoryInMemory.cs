using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Excepciones;
using System.Collections.Generic;
using System.Linq;
namespace Chassoft.Tests
{
    internal class ProveedorRepositoryInMemory : IProveedorRepository
    {
        private readonly List<Proveedor> _proveedores = new();
        private int _nextId = 1;
        public Proveedor Agregar(Proveedor proveedor)
        {
            VerificarCuit(proveedor);
            var guardado = Copiar(_nextId++, proveedor, true);
            _proveedores.Add(guardado); return guardado;
        }
        public bool Actualizar(Proveedor proveedor)
        {
            var i = _proveedores.FindIndex(p => p.Id == proveedor.Id);
            if (i < 0) return false;
            VerificarCuit(proveedor);
            _proveedores[i] = Copiar(proveedor.Id, proveedor, _proveedores[i].Activo);
            return true;
        }
        private void VerificarCuit(Proveedor proveedor)
        {
            if (_proveedores.Any(p => p.Cuit == proveedor.Cuit && p.Id != proveedor.Id))
                throw new RegistroDuplicadoException("Ya existe un proveedor registrado con el CUIT ingresado");
        }
        public bool DarDeBaja(int id) => CambiarEstado(id, false);
        public bool Reactivar(int id) => CambiarEstado(id, true);
        private bool CambiarEstado(int id, bool activo)
        {
            var i = _proveedores.FindIndex(p => p.Id == id);
            if (i < 0) return false;
            _proveedores[i] = Copiar(id, _proveedores[i], activo); return true;
        }
        private static Proveedor Copiar(int id, Proveedor p, bool activo) => new Proveedor(id, p.Nombre, p.Cuit,
            p.RazonSocial, p.TelefonoDeContacto, p.CorreoDeContacto, p.CondicionIVA, p.CbuAsociado, activo);
        public Proveedor? ObtenerPorId(int id) => _proveedores.FirstOrDefault(p => p.Id == id);
        public Proveedor? ObtenerPorCuit(string cuit) => _proveedores.FirstOrDefault(p => p.Cuit == cuit.Trim());
        public IEnumerable<Proveedor> ObtenerTodos(bool? activo = null) => _proveedores.Where(p => !activo.HasValue || p.Activo == activo.Value).ToList();
    }
}
