using Chassoft.Dominio.Personas;
using System.Collections.Generic;

namespace Chassoft.Dominio.Repositorios
{
    public interface ISocioRepository
    {
        void Agregar(Socio socio);
        void DarDeBaja(int id);
        Socio? ObtenerPorId(int id);
        Socio? ObtenerPorDni(string dni);
        IEnumerable<Socio> ObtenerTodos();
        /// <summary>Socios activos que no tienen pago registrado en el mes indicado.</summary>
        IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes);
    }
}
