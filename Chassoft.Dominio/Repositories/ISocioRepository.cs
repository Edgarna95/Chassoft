using Chassoft.Dominio.Personas;
using System.Collections.Generic;
namespace Chassoft.Dominio.Repositorios
{
    public interface ISocioRepository
    {
        Socio Agregar(Socio socio);
        bool Actualizar(Socio socio);
        bool DarDeBaja(int id);
        bool Reactivar(int id);
        Socio? ObtenerPorId(int id);
        Socio? ObtenerPorDni(string dni);
        IEnumerable<Socio> ObtenerTodos(bool? activo = null);
        IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes);
    }
}
