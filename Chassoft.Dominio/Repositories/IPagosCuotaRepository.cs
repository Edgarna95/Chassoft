using Chassoft.Dominio.Entidades;
using System.Collections.Generic;

namespace Chassoft.Dominio.Repositories
{
    public interface IPagosCuotaRepository
    {

        /// <summary>Devuelve true si el socio ya pagó el mes indicado.</summary>
        bool ExistePago(int idSocio, int anio, int mes);

        /// <summary>Historial de pagos de un socio.</summary>
        IEnumerable<PagoCuota> ObtenerPorSocio(int idSocio);

        /// <summary>Socios que NO pagaron el mes indicado y están activos.</summary>
        IEnumerable<int> ObtenerIdsSociosSinPago(int anio, int mes);
    }
}
