using System;
using System.Threading.Tasks;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Servicios;
namespace Chassoft.Tests;
public class NotificacionesTests
{
    [Test] public async Task RepetirProceso_NoDuplicaCorreo()
    {
        var repo=new Reserva();var correo=new Correo();var socio=new Socio(1,"S","123",DateTime.Today,true,"s@example.test");
        await EnvioNotificacion.EnviarAsync(socio,"RecordatorioCuota",2026,10,repo,correo);
        await EnvioNotificacion.EnviarAsync(socio,"RecordatorioCuota",2026,10,repo,correo);
        Assert.That(correo.Envios,Is.EqualTo(1)); Assert.That(repo.Enviado,Is.True);
    }
    [Test] public async Task FalloSmtp_QuedaParaRevisionSinReenviar()
    {
        var repo=new Reserva();var correo=new Correo{Falla=true};var socio=new Socio(1,"S","123",DateTime.Today,true,"s@example.test");
        Assert.ThrowsAsync<InvalidOperationException>(()=>EnvioNotificacion.EnviarAsync(socio,"AvisoMora",2026,10,repo,correo));
        await EnvioNotificacion.EnviarAsync(socio,"AvisoMora",2026,10,repo,correo);
        Assert.That(correo.Envios,Is.EqualTo(1)); Assert.That(repo.Enviado,Is.False);
    }
    [Test] public async Task SinEmail_NoReservaNiEnvia()
    {
        var repo=new Reserva();var correo=new Correo();
        await EnvioNotificacion.EnviarAsync(new Socio(1,"S","123",DateTime.Today,true,null),"AvisoMora",2026,10,repo,correo);
        Assert.That(repo.Reservada,Is.False); Assert.That(correo.Envios,Is.Zero);
    }
    private class Reserva:INotificacionRepository
    {
        public bool Reservada,Enviado;
        public bool IntentarReservar(int s,string t,int a,int m,string d){if(Reservada)return false;Reservada=true;return true;}
        public void MarcarResultado(int s,string t,int a,int m,bool enviado)=>Enviado=enviado;
    }
    private class Correo:INotificacionEmailService
    {
        public int Envios;public bool Falla;
        public Task EnviarRecordatorioCuotaAsync(Socio s,int a,int m){Envios++;if(Falla)throw new InvalidOperationException("SMTP simulado");return Task.CompletedTask;}
        public Task EnviarAvisoMoraAsync(Socio s,int a,int m)=>EnviarRecordatorioCuotaAsync(s,a,m);
    }
}
