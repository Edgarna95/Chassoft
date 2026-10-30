using System.Transactions;
using System.Text.Json;
using Chassoft.Dominio.Personas;
using Chassoft.Infrastructure.Repositories;

try
{
    if (args.Length != 1) throw new ArgumentException("Indicar la ruta a appsettings.json de la API.");
    using var config = JsonDocument.Parse(File.ReadAllText(args[0]));
    var connection = config.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()
        ?? throw new InvalidOperationException("Falta DefaultConnection.");
    var repo = new ProveedorRepositorySqlServer(connection);
    var anteriores = repo.ObtenerTodos().ToList();
    Console.WriteLine($"OK: conexion y lectura de {anteriores.Count} proveedores existentes.");
    var cuit = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 16);
    var otroCuit = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 16);
    var opciones = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromSeconds(60) };
    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, opciones))
    {
        var nuevo = new Proveedor("Prueba temporal", cuit, "Razon de prueba", "+54 11 1234",
            "prueba@example.com", "Prueba", "0012345678901234567890");
        var guardado = repo.Agregar(nuevo);
        Check(guardado.Id > 0, "ID generado");
        Igual(nuevo, guardado);
        Igual(guardado, repo.ObtenerPorId(guardado.Id));
        Igual(guardado, repo.ObtenerPorCuit(cuit));
        Igual(guardado, repo.ObtenerTodos().Single(p => p.Id == guardado.Id));
        Console.WriteLine("OK: alta completa, ID y lectura por ID, CUIT y listado.");

        var editado = new Proveedor(guardado.Id, "Nombre editado", cuit, "Razon editada", "+54 22 5678",
            "editado@example.com", "Otra prueba", "0098765432109876543210");
        repo.Actualizar(editado);
        Igual(editado, repo.ObtenerPorId(editado.Id));
        Console.WriteLine("OK: actualizacion de todos los campos y ceros iniciales del CBU.");

        var vacio = repo.Agregar(new Proveedor("Sin opcionales", otroCuit));
        Igual(vacio, repo.ObtenerPorId(vacio.Id));
        var limpiado = new Proveedor(guardado.Id, "Nombre editado", cuit);
        repo.Actualizar(limpiado);
        Igual(limpiado, repo.ObtenerPorId(limpiado.Id));
        Console.WriteLine("OK: alta con NULL y limpieza de campos opcionales.");
        // Sin Complete(): todas las escrituras se revierten al salir.
    }
    Check(repo.ObtenerPorCuit(cuit) == null && repo.ObtenerPorCuit(otroCuit) == null, "reversion de registros temporales");
    Console.WriteLine("PRUEBA APROBADA. Transaccion revertida; no quedan proveedores de prueba.");
    Console.WriteLine("SQL Server puede dejar saltos en los ID por las altas revertidas.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("PRUEBA NO APROBADA: " + ex.Message);
    Console.Error.WriteLine("Las escrituras dentro de la transaccion se revierten automaticamente.");
    return 1;
}

static void Check(bool condicion, string paso)
{
    if (!condicion) throw new InvalidOperationException("Fallo: " + paso);
}

static void Igual(Proveedor esperado, Proveedor? obtenido)
{
    Check(obtenido != null, "registro encontrado");
    if (obtenido == null) return;
    if (esperado.Id != 0) Check(esperado.Id == obtenido.Id, "identidad conservada");
    Check(esperado.Nombre == obtenido.Nombre && esperado.Cuit == obtenido.Cuit &&
        esperado.RazonSocial == obtenido.RazonSocial && esperado.TelefonoDeContacto == obtenido.TelefonoDeContacto &&
        esperado.CorreoDeContacto == obtenido.CorreoDeContacto && esperado.CondicionIVA == obtenido.CondicionIVA &&
        esperado.CbuAsociado == obtenido.CbuAsociado, "campos conservados");
}
