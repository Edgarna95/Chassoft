using System;
using System.Threading;
using System.Threading.Tasks;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Chassoft.Dominio.Servicios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Chassoft.API.Jobs
{
    /// <summary>
    /// Servicio en segundo plano que verifica diariamente el estado de cuotas.
    ///
    /// Logica:
    ///   - Dia  5 del mes - envia "RecordatorioCuota" a socios sin pago del mes en curso.
    ///   - Dia 15 del mes - envia "AvisoMora" a socios que siguen sin pagar.
    ///   - Maximo 1 email de cada tipo por socio por mes (controlado por INotificacionRepository).
    /// </summary>
    public class VerificarCuotasJob : BackgroundService
    {
        private static readonly TimeSpan _intervalo = TimeSpan.FromHours(1);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VerificarCuotasJob> _logger;
        private readonly TimeZoneInfo _zona;

        public VerificarCuotasJob(IServiceScopeFactory scopeFactory,
                                  ILogger<VerificarCuotasJob> logger, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _zona = TimeZoneInfo.FindSystemTimeZoneById(config["Notificaciones:TimeZoneId"] ?? "Argentina Standard Time");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VerificarCuotasJob iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en VerificarCuotasJob.");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task ProcesarAsync()
        {
            var hoy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zona).Date;
            var anio = hoy.Year;
            var mes  = hoy.Month;
            var dia  = hoy.Day;

            // Solo actua en dia 5 o dia 15
            if (dia != 5 && dia != 15)
                return;

            var tipo = dia == 5 ? "RecordatorioCuota" : "AvisoMora";
            _logger.LogInformation("Procesando notificaciones tipo '{Tipo}' para {Mes}/{Anio}", tipo, mes, anio);

            using var scope = _scopeFactory.CreateScope();
            var socioRepo    = scope.ServiceProvider.GetRequiredService<ISocioRepository>();
            var notifRepo    = scope.ServiceProvider.GetRequiredService<INotificacionRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<INotificacionEmailService>();

            var sociosSinPago = socioRepo.ObtenerSociosSinPago(anio, mes);
            int enviados = 0;

            foreach (var socio in sociosSinPago)
            {
                if (string.IsNullOrWhiteSpace(socio.Email))
                    continue;

                try
                {
                    if (await EnvioNotificacion.EnviarAsync(socio, tipo, anio, mes, notifRepo, emailService))
                        enviados++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar notificacion al socio {Id} ({Email})",
                        socio.Id, socio.Email);
                }
            }

            _logger.LogInformation("Notificaciones enviadas: {Count}", enviados);
        }
    }
}
