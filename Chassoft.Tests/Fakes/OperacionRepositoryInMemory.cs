using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Repositories;
using System.Collections.Generic;

namespace Chassoft.Tests
{
    /// <summary>
    /// Implementacion en memoria de IOperacionRepository para tests unitarios.
    /// No toca la base de datos.
    /// </summary>
    internal class OperacionRepositoryInMemory : IOperacionRepository
    {
        private readonly List<Operacion> _operaciones = new();

        public void Agregar(Operacion operacion)
            => _operaciones.Add(operacion);

        public int CobrarCuotas(CobroCuotas cobro)
        {
            _operaciones.Add(new Ingreso(cobro.Total, cobro.IdEmpleado,
                Chassoft.Dominio.Entidades.Enums.TipoIngreso.Cuota, cobro.Socio));
            return _operaciones.Count;
        }

        public IEnumerable<Operacion> ObtenerTodas()
            => _operaciones.AsReadOnly();
    }
}
