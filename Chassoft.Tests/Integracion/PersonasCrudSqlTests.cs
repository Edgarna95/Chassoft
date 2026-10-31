using System;
using System.Collections.Generic;
using System.Linq;
using Chassoft.Dominio.Excepciones;
using Chassoft.Dominio.Personas;
using Chassoft.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
namespace Chassoft.Tests;
[TestFixture, Category("SqlIntegration"), NonParallelizable]
public class PersonasCrudSqlTests
{
    private string _cs = "";
    private readonly List<int> _socios = new();
    private readonly List<int> _proveedores = new();
    [OneTimeSetUp] public void Preparar()
    {
        _cs = Environment.GetEnvironmentVariable("CHASSOFT_TEST_CONNECTION_STRING") ?? "";
        if (string.IsNullOrWhiteSpace(_cs)) Assert.Ignore("Se requiere una base aislada con migración 005 para verificar el CRUD SQL.");
        if (!new SqlConnectionStringBuilder(_cs).InitialCatalog.StartsWith("ChassoftTest_", StringComparison.Ordinal))
            Assert.Fail("Solo se permite una base ChassoftTest_; no utilizar Chassoft real.");
    }
    [Test] public void CrudSql_PreservaIdentidadHistoriaYUnicidad()
    {
        var socios = new SocioRepositorySqlServer(_cs);
        var proveedores = new ProveedorRepositorySqlServer(_cs);
        var documento = "T" + Guid.NewGuid().ToString("N").Substring(0, 18);
        var socio = socios.Agregar(new Socio("Prueba CRUD SQL", documento, "crud@example.test")); _socios.Add(socio.Id);
        var proveedor = proveedores.Agregar(new Proveedor("Prueba CRUD SQL", documento)); _proveedores.Add(proveedor.Id);
        Assert.That(socio.Id, Is.GreaterThan(0)); Assert.That(proveedor.Id, Is.GreaterThan(0));
        Assert.That(socios.DarDeBaja(socio.Id), Is.True); Assert.That(socios.DarDeBaja(socio.Id), Is.True);
        Assert.That(proveedores.DarDeBaja(proveedor.Id), Is.True); Assert.That(proveedores.DarDeBaja(proveedor.Id), Is.True);
        // Se intenta pasar estado y fecha distintos: el UPDATE solo modifica los datos editables.
        Assert.That(socios.Actualizar(new Socio(socio.Id, "Socio editado", documento, new DateTime(2000, 1, 1), true, null)), Is.True);
        Assert.That(proveedores.Actualizar(new Proveedor(proveedor.Id, "Proveedor editado", documento)), Is.True);
        var socioLeido = socios.ObtenerPorId(socio.Id)!; var proveedorLeido = proveedores.ObtenerPorId(proveedor.Id)!;
        Assert.That(socioLeido.FechaAlta, Is.EqualTo(socio.FechaAlta)); Assert.That(socioLeido.Activo, Is.False);
        Assert.That(proveedorLeido.Activo, Is.False);
        Assert.That(socios.ObtenerTodos(false).Any(s => s.Id == socio.Id), Is.True);
        Assert.That(proveedores.ObtenerTodos(true).Any(p => p.Id == proveedor.Id), Is.False);
        Assert.Throws<RegistroDuplicadoException>(() => socios.Agregar(new Socio("Duplicado", documento)));
        Assert.Throws<RegistroDuplicadoException>(() => proveedores.Agregar(new Proveedor("Duplicado", documento)));
        Assert.That(socios.Reactivar(socio.Id), Is.True); Assert.That(proveedores.Reactivar(proveedor.Id), Is.True);
        Assert.That(socios.ObtenerPorId(socio.Id)!.Activo, Is.True);
        Assert.That(proveedores.ObtenerPorId(proveedor.Id)!.Activo, Is.True);
    }
    [OneTimeTearDown] public void Limpiar()
    {
        if ((_socios.Count + _proveedores.Count) == 0) return;
        using var conn = new SqlConnection(_cs); conn.Open();
        foreach (var id in _socios)
        {
            using var cmd = new SqlCommand("DELETE FROM dbo.Socios WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
        }
        foreach (var id in _proveedores)
        {
            using var cmd = new SqlCommand("DELETE FROM dbo.Proveedores WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
        }
    }
}
