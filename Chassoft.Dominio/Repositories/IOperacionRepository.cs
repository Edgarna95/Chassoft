using System.Collections.Generic;
using Chassoft.Dominio.Entidades;

namespace Chassoft.Dominio.Repositories
{
    public interface IOperacionRepository
    {
        void Agregar(Operacion operacion);
        int CobrarCuotas(CobroCuotas cobro);
        IEnumerable<Operacion> ObtenerTodas();
    }
}
