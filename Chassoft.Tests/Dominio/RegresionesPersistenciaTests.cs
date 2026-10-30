using System;
using System.Data;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Dominio.Personas;
using Chassoft.Infrastructure.Repositories;
namespace Chassoft.Tests;
public class RegresionesPersistenciaTests
{
    [Test]
    public void LecturaCuotaConSocioDadoDeBaja_ConservaHistoria()
    {
        var t = new DataTable();
        foreach (var x in new[] { typeof(int), typeof(DateTime), typeof(decimal), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(DateTime), typeof(bool), typeof(string) })
            t.Columns.Add("c" + t.Columns.Count, x);
        var fecha = new DateTime(2023, 2, 10, 9, 30, 0);
        var alta = new DateTime(2020, 5, 6);
        t.Rows.Add(42, fecha, 10000m, 7, 1, 15, "Socio histórico", "12345678", alta, false, "historico@example.test");
        using var reader = t.CreateDataReader(); reader.Read();
        var ingreso = OperacionRepositorySqlServer.LeerIngreso(reader);
        Assert.Multiple(() => {
            Assert.That(ingreso.Id, Is.EqualTo(42)); Assert.That(ingreso.Fecha, Is.EqualTo(fecha));
            Assert.That(ingreso.Socio!.Id, Is.EqualTo(15)); Assert.That(ingreso.Socio.FechaAlta, Is.EqualTo(alta));
            Assert.That(ingreso.Socio.Activo, Is.False); Assert.That(ingreso.Socio.Email, Is.EqualTo("historico@example.test"));
        });
    }
    [Test]
    public void LecturaEgreso_ConservaFechaYProveedorCompleto()
    {
        var t = new DataTable();
        foreach (var x in new[] { typeof(int), typeof(DateTime), typeof(decimal), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) })
            t.Columns.Add("c" + t.Columns.Count, x);
        var fecha = new DateTime(2021, 1, 5);
        t.Rows.Add(9, fecha, 50m, 7, 1, 3, "Proveedor", "123", "Razón", DBNull.Value, "p@example.test", "IVA", "0012345678901234567890");
        using var reader = t.CreateDataReader(); reader.Read();
        var egreso = OperacionRepositorySqlServer.LeerEgreso(reader);
        Assert.Multiple(() => {
            Assert.That(egreso.Fecha, Is.EqualTo(fecha)); Assert.That(egreso.Proveedor.Id, Is.EqualTo(3));
            Assert.That(egreso.Proveedor.RazonSocial, Is.EqualTo("Razón")); Assert.That(egreso.Proveedor.TelefonoDeContacto, Is.Null);
            Assert.That(egreso.Proveedor.CbuAsociado, Does.StartWith("00"));
        });
    }
    [Test] public void Ingreso_TipoInexistente_SeRechaza() => Assert.Throws<ArgumentException>(() => new Ingreso(100m, 1, (TipoIngreso)99));
    [Test] public void Egreso_TipoInexistente_SeRechaza() => Assert.Throws<ArgumentException>(() => new Egreso(100m,1,new Proveedor("P","123"),(TipoEgreso)99));
    [Test] public void DineroConFraccionesDeCentavo_SeRechaza() => Assert.Throws<ArgumentException>(() => new Ingreso(10.001m,1,TipoIngreso.Donacion));
    [Test] public void CuotaSinDetalle_NoAbreConexion() => Assert.Throws<ArgumentException>(() => new OperacionRepositorySqlServer("no usar").Agregar(new Ingreso(10000m,1,TipoIngreso.Cuota,new Socio("Socio","123"))));
    [Test] public void TresMeses_Completos_SinBonificar()
    {
        var cobro = new CobroCuotas(new Socio(1,"S","123",DateTime.Today,true,null),1,10000m,
            new[] { new PeriodoCuota(2026,10), new PeriodoCuota(2026,11), new PeriodoCuota(2026,12) });
        Assert.That(cobro.Total, Is.EqualTo(30000m));
    }
    [Test] public void MesRepetido_SeRechaza() => Assert.Throws<ArgumentException>(() => new CobroCuotas(
        new Socio(1,"S","123",DateTime.Today,true,null),1,10000m,new[] { new PeriodoCuota(2026,10), new PeriodoCuota(2026,10) }));
    [Test] public void SocioInactivo_NoPuedePagar() => Assert.Throws<ArgumentException>(() => new CobroCuotas(
        new Socio(1,"S","123",DateTime.Today,false,null),1,10000m,new[] { new PeriodoCuota(2026,10) }));
}
