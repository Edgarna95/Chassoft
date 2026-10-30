using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chassoft.Tests
{
    /// <summary>
    /// Implementacion en memoria de IProveedorRepository para tests unitarios.
    /// No toca la base de datos.
    /// </summary>
    internal class ProveedorRepositoryInMemory : IProveedorRepository
    {
        private readonly List<Proveedor> _proveedores = new();
        private int _nextId = 1;

        public Proveedor Agregar(Proveedor proveedor)
        {
            if (_proveedores.Any(p => p.Cuit == proveedor.Cuit))
                throw new ArgumentException("Ya existe un proveedor registrado con el CUIT ingresado");

            var conId = new Proveedor(_nextId++, proveedor.Nombre, proveedor.Cuit,
                proveedor.RazonSocial, proveedor.TelefonoDeContacto,
                proveedor.CorreoDeContacto, proveedor.CondicionIVA, proveedor.CbuAsociado);

            _proveedores.Add(conId);
            return conId;
        }

        public void Actualizar(Proveedor proveedor)
        {
            var indice = _proveedores.FindIndex(p => p.Id == proveedor.Id);
            if (indice < 0)
                throw new ArgumentException($"Proveedor con Id {proveedor.Id} no encontrado");
            _proveedores[indice] = proveedor;
        }

        public void Eliminar(int id)
        {
            var proveedor = _proveedores.FirstOrDefault(p => p.Id == id);
            if (proveedor != null)
                _proveedores.Remove(proveedor);
        }

        public Proveedor? ObtenerPorId(int id)
            => _proveedores.FirstOrDefault(p => p.Id == id);

        public Proveedor? ObtenerPorCuit(string cuit)
            => _proveedores.FirstOrDefault(p => p.Cuit == cuit);

        public IEnumerable<Proveedor> ObtenerTodos()
            => _proveedores.AsReadOnly();
    }
}
