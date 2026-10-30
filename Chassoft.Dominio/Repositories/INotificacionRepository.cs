namespace Chassoft.Dominio.Repositories
{
    public interface INotificacionRepository
    {
        // La reserva durable se hace ANTES del SMTP. No se reintenta un resultado incierto.
        bool IntentarReservar(int idSocio, string tipo, int anio, int mes, string destinatario);
        void MarcarResultado(int idSocio, string tipo, int anio, int mes, bool enviado);
    }
}
