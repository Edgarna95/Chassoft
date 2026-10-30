using System;
using System.Linq;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Personas;
using Chassoft.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
namespace Chassoft.Tests;
[TestFixture, Category("SqlIntegration"), NonParallelizable]
public class CajaSqlTests
{
    private string _cs = "";
    private int _empleado, _socio;
    [OneTimeSetUp] public void Preparar()
    {
        _cs = Environment.GetEnvironmentVariable("CHASSOFT_TEST_CONNECTION_STRING") ?? "";
        if (string.IsNullOrWhiteSpace(_cs)) Assert.Ignore("Configurar una base de pruebas aislada con CHASSOFT_TEST_CONNECTION_STRING.");
        var builder = new SqlConnectionStringBuilder(_cs);
        if (!builder.InitialCatalog.StartsWith("ChassoftTest_", StringComparison.Ordinal))
            Assert.Fail("La base de integración debe comenzar con ChassoftTest_; no usar la base real.");
        using var conn = new SqlConnection(_cs); conn.Open();
        var dni = "T" + Guid.NewGuid().ToString("N").Substring(0,18);
        using var cmd = new SqlCommand(@"INSERT INTO Empleados (NombreCompleto,Dni,Categoria,FechaIngreso,Activo)
            OUTPUT INSERTED.Id VALUES ('Test SQL',@dni,'Administrativo',GETDATE(),1)",conn);
        cmd.Parameters.AddWithValue("@dni",dni); _empleado=(int)cmd.ExecuteScalar()!;
        using var socio=new SqlCommand(@"INSERT INTO Socios (NombreCompleto,Dni,FechaAlta,Activo)
            OUTPUT INSERTED.Id VALUES ('Test SQL',@dni,'2020-01-01',1)",conn);
        socio.Parameters.AddWithValue("@dni",dni);_socio=(int)socio.ExecuteScalar()!;
    }
    [Test] public void CobroMultiple_DuplicadoRevierteTodoYConservaFecha()
    {
        var repo=new OperacionRepositorySqlServer(_cs);
        var socio=new Socio(_socio,"Test SQL","123",new DateTime(2020,1,1),true,null);
        var id=repo.CobrarCuotas(new CobroCuotas(socio,_empleado,10000m,new[]{new PeriodoCuota(2026,10),new PeriodoCuota(2026,11)}));
        var pagos=new PagosCuotaRepositorySqlServer(_cs);
        Assert.That(pagos.ObtenerPorSocio(_socio).Count(),Is.EqualTo(2));
        Assert.Throws<CuotasYaPagadasException>(()=>repo.CobrarCuotas(new CobroCuotas(socio,_empleado,10000m,
            new[]{new PeriodoCuota(2026,12),new PeriodoCuota(2026,10)})));
        Assert.That(pagos.ExistePago(_socio,2026,12),Is.False);
        using var conn=new SqlConnection(_cs);conn.Open();
        using var cmd=new SqlCommand("UPDATE Socios SET Activo=0 WHERE Id=@s; UPDATE Operaciones SET Fecha='2022-02-03T10:30:00' WHERE Id=@o; SELECT COUNT(*) FROM Operaciones WHERE IdEmpleado=@e",conn);
        cmd.Parameters.AddWithValue("@s",_socio);cmd.Parameters.AddWithValue("@o",id);cmd.Parameters.AddWithValue("@e",_empleado);
        Assert.That((int)cmd.ExecuteScalar()!,Is.EqualTo(1));
        var lectura=(Ingreso)repo.ObtenerTodas().Single(o=>o.Id==id);
        Assert.That(lectura.Fecha,Is.EqualTo(new DateTime(2022,2,3,10,30,0)));
        Assert.That(lectura.Socio!.Activo,Is.False);Assert.That(lectura.Socio.Id,Is.EqualTo(_socio));
    }
    [OneTimeTearDown] public void Limpiar()
    {
        if (_empleado==0 || !_cs.Contains("ChassoftTest_")) return;
        using var conn=new SqlConnection(_cs);conn.Open();
        using var cmd=new SqlCommand(@"DELETE FROM PagosCuota WHERE IdEmpleado=@e;
            DELETE FROM Ingresos WHERE IdOperacion IN (SELECT Id FROM Operaciones WHERE IdEmpleado=@e);
            DELETE FROM Egresos WHERE IdOperacion IN (SELECT Id FROM Operaciones WHERE IdEmpleado=@e);
            DELETE FROM Operaciones WHERE IdEmpleado=@e; DELETE FROM Socios WHERE Id=@s; DELETE FROM Empleados WHERE Id=@e;",conn);
        cmd.Parameters.AddWithValue("@e",_empleado);cmd.Parameters.AddWithValue("@s",_socio);cmd.ExecuteNonQuery();
    }
}
