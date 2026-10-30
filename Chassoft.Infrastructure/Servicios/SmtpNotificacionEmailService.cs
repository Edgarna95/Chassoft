using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Servicios;
using Microsoft.Extensions.Configuration;

namespace Chassoft.Infrastructure.Servicios
{
    /// <summary>
    /// Envia notificaciones de cuota por correo electronico via SMTP.
    /// Configurable desde appsettings.json seccion "Smtp".
    /// </summary>
    public class SmtpNotificacionEmailService : INotificacionEmailService
    {
        private readonly SmtpConfig _config;

        public SmtpNotificacionEmailService(IConfiguration configuration)
        {
            _config = configuration.GetSection("Smtp").Get<SmtpConfig>()
                      ?? throw new InvalidOperationException("Falta la seccion 'Smtp' en appsettings.json");
            if (string.IsNullOrWhiteSpace(_config.Host) || string.IsNullOrWhiteSpace(_config.Username)
                || string.IsNullOrWhiteSpace(_config.Password) || string.IsNullOrWhiteSpace(_config.FromAddress))
                throw new InvalidOperationException("Configurá SMTP antes de habilitar las notificaciones.");
        }

        public async Task EnviarRecordatorioCuotaAsync(Socio socio, int anio, int mes)
        {
            if (string.IsNullOrWhiteSpace(socio.Email))
                return;

            var nombreMes = NombreMes(mes);
            var asunto = $"Recordatorio de cuota - {nombreMes} {anio}";
            var cuerpo = $@"Estimado/a {socio.NombreCompleto},

Le recordamos que su cuota mensual correspondiente a {nombreMes} {anio} aun no ha sido registrada en nuestro sistema.

Para realizar el pago puede acercarse a la administracion del club en horario de atencion.

Atentamente,
Administracion - Chassoft";

            await EnviarAsync(socio.Email, asunto, cuerpo);
        }

        public async Task EnviarAvisoMoraAsync(Socio socio, int anio, int mes)
        {
            if (string.IsNullOrWhiteSpace(socio.Email))
                return;

            var nombreMes = NombreMes(mes);
            var asunto = $"Aviso de mora - cuota {nombreMes} {anio} impaga";
            var cuerpo = $@"Estimado/a {socio.NombreCompleto},

Le informamos que su cuota correspondiente a {nombreMes} {anio} se encuentra IMPAGA.

Le solicitamos regularizar su situacion a la brevedad para mantener su condicion de socio activo y evitar la suspension de servicios.

Por favor comuniquese con la administracion del club.

Atentamente,
Administracion - Chassoft";

            await EnviarAsync(socio.Email, asunto, cuerpo);
        }

        private async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            using var client = new SmtpClient(_config.Host, _config.Port)
            {
                EnableSsl = _config.UseSsl,
                Credentials = new NetworkCredential(_config.Username, _config.Password)
            };

            using var mensaje = new MailMessage(
                new MailAddress(_config.FromAddress, _config.FromName),
                new MailAddress(destinatario))
            {
                Subject = asunto,
                Body    = cuerpo,
                IsBodyHtml = false
            };

            await client.SendMailAsync(mensaje);
        }

        private static string NombreMes(int mes) => mes switch
        {
            1  => "Enero",    2  => "Febrero",  3  => "Marzo",
            4  => "Abril",    5  => "Mayo",      6  => "Junio",
            7  => "Julio",    8  => "Agosto",    9  => "Septiembre",
            10 => "Octubre",  11 => "Noviembre", 12 => "Diciembre",
            _  => mes.ToString()
        };
    }

    public class SmtpConfig
    {
        public string Host        { get; set; } = "";
        public int    Port        { get; set; } = 587;
        public bool   UseSsl      { get; set; } = true;
        public string Username    { get; set; } = "";
        public string Password    { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName    { get; set; } = "Chassoft";
    }
}
