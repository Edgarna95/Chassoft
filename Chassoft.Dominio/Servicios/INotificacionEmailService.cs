using Chassoft.Dominio.Personas;
using System.Threading.Tasks;

namespace Chassoft.Dominio.Servicios
{
    public interface INotificacionEmailService
    {
        /// <summary>Email 1 — Día 5: recordatorio preventivo de cuota.</summary>
        Task EnviarRecordatorioCuotaAsync(Socio socio, int anio, int mes);

        /// <summary>Email 2 — Día 15: aviso de mora para socios que no pagaron tras el recordatorio.</summary>
        Task EnviarAvisoMoraAsync(Socio socio, int anio, int mes);
    }
}
