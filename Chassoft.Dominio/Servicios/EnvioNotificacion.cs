using System;
using System.Threading.Tasks;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
namespace Chassoft.Dominio.Servicios
{
    public static class EnvioNotificacion
    {
        public static async Task<bool> EnviarAsync(Socio socio, string tipo, int anio, int mes,
            INotificacionRepository repo, INotificacionEmailService correo)
        {
            if (!socio.Activo || string.IsNullOrWhiteSpace(socio.Email)) return false;
            if (!repo.IntentarReservar(socio.Id, tipo, anio, mes, socio.Email!)) return false;
            try
            {
                if (tipo == "RecordatorioCuota") await correo.EnviarRecordatorioCuotaAsync(socio, anio, mes);
                else if (tipo == "AvisoMora") await correo.EnviarAvisoMoraAsync(socio, anio, mes);
                else throw new ArgumentException("Tipo de notificación inválido.");
                repo.MarcarResultado(socio.Id, tipo, anio, mes, true);
                return true;
            }
            catch
            {
                // SMTP puede haber aceptado el correo aunque se pierda su respuesta.
                // Revisar/Pendiente requiere conciliación manual, sin reenvío automático.
                repo.MarcarResultado(socio.Id, tipo, anio, mes, false);
                throw;
            }
        }
    }
}
