using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chassoft.API.Controllers;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
namespace Chassoft.Tests;
public class ContratosApiTests
{
    [Test]
    public void AltaProveedor_PasaTodosLosCamposAlRepositorio()
    {
        var repo = new ProveedorRepositoryInMemory();
        var controller = new ProveedorController(repo);
        var resultado = controller.Crear(new CrearProveedorRequest {
            Nombre="Distribuidora", Cuit="123", RazonSocial="Razón", TelefonoDeContacto="111",
            CorreoDeContacto="correo@example.test", CondicionIVA="IVA", CbuAsociado="0012345678901234567890" });
        var p = repo.ObtenerPorCuit("123")!;
        Assert.Multiple(() => {
            Assert.That(resultado, Is.TypeOf<CreatedAtActionResult>());
            Assert.That(p.RazonSocial, Is.EqualTo("Razón")); Assert.That(p.TelefonoDeContacto, Is.EqualTo("111"));
            Assert.That(p.CorreoDeContacto, Is.EqualTo("correo@example.test")); Assert.That(p.CondicionIVA, Is.EqualTo("IVA"));
            Assert.That(p.CbuAsociado, Is.EqualTo("0012345678901234567890"));
        });
    }
    [Test] public void CbuYCorreoInvalidos_ContratoRechazado()
    {
        var pedido = new CrearProveedorRequest { Nombre="P", Cuit="123", CbuAsociado="12", CorreoDeContacto="invalido" };
        var errores = new List<ValidationResult>();
        Assert.That(Validator.TryValidateObject(pedido,new ValidationContext(pedido),errores,true),Is.False);
        Assert.That(errores.Count,Is.EqualTo(2));
    }
    [Test] public void CobroMeses_CalculaEnServidorYHaceUnaLlamada()
    {
        var socios=new SocioRepositoryInMemory(); socios.Agregar(new("Socio","123","s@example.test"));
        var repo=new RepositorioCobro();
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Cuotas:ImporteMensual"]="10000" }).Build();
        var api=new OperacionController(repo,socios,new ProveedorRepositoryInMemory(),config);
        var resultado=api.CobrarCuotas(new() { IdSocio=1,IdEmpleado=1,Periodos=new() { new() {Anio=2026,Mes=10},new(){Anio=2026,Mes=11} } });
        Assert.That(resultado,Is.TypeOf<OkObjectResult>());
        Assert.That(repo.Llamadas,Is.EqualTo(1)); Assert.That(repo.Total,Is.EqualTo(20000m));
    }
    [Test] public void CuotaSinPeriodo_NoGeneraIngreso()
    {
        var repo=new RepositorioCobro();
        var api=new OperacionController(repo,new SocioRepositoryInMemory(),new ProveedorRepositoryInMemory(),new ConfigurationBuilder().Build());
        Assert.That(api.RegistrarIngreso(new(){Monto=10000,IdEmpleado=1,Tipo=1}),Is.TypeOf<BadRequestObjectResult>());
        Assert.That(repo.Llamadas,Is.Zero);
    }
    [Test] public void ValidacionMonetaria_AdmiteCulturaArgentina()
    {
        var anterior = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-AR");
            var req = new RegistrarIngresoRequest { Monto=10.25m,IdEmpleado=1,Tipo=2 };
            Assert.That(Validator.TryValidateObject(req,new ValidationContext(req),new List<ValidationResult>(),true),Is.True);
            req.Tipo=99;
            Assert.That(Validator.TryValidateObject(req,new ValidationContext(req),new List<ValidationResult>(),true),Is.False);
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = anterior; }
    }
    private class RepositorioCobro : IOperacionRepository
    {
        public int Llamadas; public decimal Total;
        public void Agregar(Operacion operacion) { Llamadas++; }
        public int CobrarCuotas(CobroCuotas cobro) { Llamadas++;Total=cobro.Total;return 10; }
        public IEnumerable<Operacion> ObtenerTodas()=>Array.Empty<Operacion>();
    }
}
