using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Chassoft.API.Controllers;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Chassoft.Tests;
[TestFixture, NonParallelizable]
public class CrudHttpTests
{
    private WebApplication _app = null!;
    private HttpClient _http = null!;
    private int _secuencia = 10000000;
    private readonly OperacionRepositoryInMemory _operaciones = new();
    [OneTimeSetUp]
    public async Task Iniciar()
    {
        // Controladores reales, model binding real y Kestrel local; sin SQL ni SMTP.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
            ApplicationName = typeof(SocioController).Assembly.GetName().Name,
            EnvironmentName = "Testing", ContentRootPath = AppContext.BaseDirectory });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(SocioController).Assembly);
        builder.Services.AddSingleton<ISocioRepository>(new SocioRepositoryInMemory());
        builder.Services.AddSingleton<IProveedorRepository>(new ProveedorRepositoryInMemory());
        builder.Services.AddSingleton<IPagosCuotaRepository>(new PagosSinSql());
        builder.Services.AddSingleton<IOperacionRepository>(_operaciones);
        _app = builder.Build(); _app.MapControllers(); await _app.StartAsync();
        var direccion = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        _http = new HttpClient { BaseAddress = new Uri(direccion) };
    }
    [OneTimeTearDown] public async Task Cerrar()
    {
        _http?.Dispose();
        if (_app != null) await _app.DisposeAsync();
    }
    private object Datos(string recurso, string documento, string nombre = "Nombre de prueba") => recurso == "socio"
        ? new { NombreCompleto = nombre, Dni = documento, Email = "nuevo@example.test", Activo = true, FechaAlta = "2000-01-01" }
        : (object)new { Nombre = nombre, Cuit = documento, CorreoDeContacto = "nuevo@example.test", RazonSocial = "Razón editada", CbuAsociado = "0012345678901234567890", Activo = true };
    private async Task<(int Id, string Documento, JsonElement Entidad, Uri? Location)> Alta(string recurso)
    {
        var documento = (++_secuencia).ToString();
        using var res = await _http.PostAsJsonAsync("/api/" + recurso, Datos(recurso, documento));
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created), await res.Content.ReadAsStringAsync());
        var entidad = await res.Content.ReadFromJsonAsync<JsonElement>();
        return (entidad.GetProperty("id").GetInt32(), documento, entidad, res.Headers.Location);
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task Alta_Devuelve201IdYLocationConsultable(string recurso)
    {
        var alta = await Alta(recurso);
        Assert.That(alta.Id, Is.GreaterThan(0)); Assert.That(alta.Location, Is.Not.Null);
        using var res = await _http.GetAsync(alta.Location);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var leido = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(leido.GetProperty("id").GetInt32(), Is.EqualTo(alta.Id));
        Assert.That(leido.GetProperty("activo").GetBoolean(), Is.True);
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task BajaLogicaEsIdempotenteYEdicionNoReactiva(string recurso)
    {
        var alta = await Alta(recurso); var ruta = $"/api/{recurso}/{alta.Id}";
        using var baja1 = await _http.DeleteAsync(ruta);
        using var baja2 = await _http.DeleteAsync(ruta);
        Assert.That(baja1.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(baja2.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        using var edicion = await _http.PutAsJsonAsync(ruta, Datos(recurso, alta.Documento, "Editado"));
        Assert.That(edicion.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var leido = await _http.GetFromJsonAsync<JsonElement>(ruta);
        Assert.That(leido.GetProperty("id").GetInt32(), Is.EqualTo(alta.Id));
        Assert.That(leido.GetProperty("activo").GetBoolean(), Is.False);
        Assert.That(leido.GetProperty(recurso == "socio" ? "nombreCompleto" : "nombre").GetString(), Is.EqualTo("Editado"));
        if (recurso == "socio") Assert.That(leido.GetProperty("fechaAlta").GetString(), Is.EqualTo(alta.Entidad.GetProperty("fechaAlta").GetString()));
        else Assert.That(leido.GetProperty("razonSocial").GetString(), Is.EqualTo("Razón editada"));
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task Reactivar_ConservaIdYDocumento(string recurso)
    {
        var alta = await Alta(recurso); var ruta = $"/api/{recurso}/{alta.Id}";
        using var baja = await _http.DeleteAsync(ruta);
        using var reactivacion = await _http.PostAsync(ruta + "/reactivar", null);
        Assert.That(reactivacion.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var leido = await _http.GetFromJsonAsync<JsonElement>(ruta);
        Assert.That(leido.GetProperty("activo").GetBoolean(), Is.True);
        Assert.That(leido.GetProperty("id").GetInt32(), Is.EqualTo(alta.Id));
        Assert.That(leido.GetProperty(recurso == "socio" ? "dni" : "cuit").GetString(), Is.EqualTo(alta.Documento));
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task DocumentoDuplicado_Devuelve409InclusoDeBaja(string recurso)
    {
        var primero = await Alta(recurso); var segundo = await Alta(recurso);
        using var baja = await _http.DeleteAsync($"/api/{recurso}/{primero.Id}");
        using var altaDuplicada = await _http.PostAsJsonAsync("/api/" + recurso, Datos(recurso, primero.Documento));
        using var edicionDuplicada = await _http.PutAsJsonAsync($"/api/{recurso}/{segundo.Id}", Datos(recurso, primero.Documento));
        Assert.That(altaDuplicada.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(edicionDuplicada.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        var sinCambios = await _http.GetFromJsonAsync<JsonElement>($"/api/{recurso}/{segundo.Id}");
        Assert.That(sinCambios.GetProperty(recurso == "socio" ? "dni" : "cuit").GetString(), Is.EqualTo(segundo.Documento));
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task IdInexistente_Devuelve404EnConsultaEdicionBajaYReactivacion(string recurso)
    {
        var ruta = $"/api/{recurso}/2147483000";
        using var get = await _http.GetAsync(ruta);
        using var put = await _http.PutAsJsonAsync(ruta, Datos(recurso, "99887766"));
        using var delete = await _http.DeleteAsync(ruta);
        using var post = await _http.PostAsync(ruta + "/reactivar", null);
        foreach (var res in new[] { get, put, delete, post }) Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task DatosInvalidos_Devuelven400(string recurso)
    {
        using var invalidos = await _http.PostAsJsonAsync("/api/" + recurso, Datos(recurso, "123", "   "));
        using var idInvalido = await _http.GetAsync($"/api/{recurso}/0");
        using var filtroInvalido = await _http.GetAsync($"/api/{recurso}?activo=invalido");
        var busqueda = recurso == "socio" ? "por-dni" : "por-cuit";
        using var documentoFaltante = await _http.GetAsync($"/api/{recurso}/{busqueda}");
        foreach (var res in new[] { invalidos, idInvalido, filtroInvalido, documentoFaltante }) Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    [TestCase("socio"), TestCase("proveedor")]
    public async Task FiltrosYBusqueda_IncluyenBajasSinBorrarHistoria(string recurso)
    {
        var alta = await Alta(recurso);
        using var baja = await _http.DeleteAsync($"/api/{recurso}/{alta.Id}");
        var inactivos = await _http.GetFromJsonAsync<JsonElement[]>($"/api/{recurso}?activo=false");
        var activos = await _http.GetFromJsonAsync<JsonElement[]>($"/api/{recurso}?activo=true");
        var todos = await _http.GetFromJsonAsync<JsonElement[]>($"/api/{recurso}");
        Assert.That(inactivos!.Any(e => e.GetProperty("id").GetInt32() == alta.Id), Is.True);
        Assert.That(activos!.Any(e => e.GetProperty("id").GetInt32() == alta.Id), Is.False);
        Assert.That(todos!.Any(e => e.GetProperty("id").GetInt32() == alta.Id), Is.True);
        var consulta = recurso == "socio" ? "por-dni?dni=" : "por-cuit?cuit=";
        var encontrado = await _http.GetFromJsonAsync<JsonElement>($"/api/{recurso}/{consulta}{alta.Documento}");
        Assert.That(encontrado.GetProperty("id").GetInt32(), Is.EqualTo(alta.Id));
    }
    [Test] public async Task BajaSocioAnterior_SigueDisponible()
    {
        var alta = await Alta("socio");
        using var baja = await _http.PostAsync($"/api/socio/{alta.Id}/baja", null);
        Assert.That(baja.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
    [Test] public async Task ProveedorInactivo_NoPermiteRegistrarEgreso()
    {
        var alta = await Alta("proveedor");
        using var baja = await _http.DeleteAsync($"/api/proveedor/{alta.Id}");
        using var egreso = await _http.PostAsJsonAsync("/api/operacion/egreso", new { Monto = 100m, IdEmpleado = 1, Tipo = 1, IdProveedor = alta.Id });
        Assert.That(egreso.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(_operaciones.ObtenerTodas(), Is.Empty);
    }
    private sealed class PagosSinSql : IPagosCuotaRepository
    {
        public bool ExistePago(int id, int anio, int mes) => false;
        public System.Collections.Generic.IEnumerable<PagoCuota> ObtenerPorSocio(int id) => Array.Empty<PagoCuota>();
        public System.Collections.Generic.IEnumerable<int> ObtenerIdsSociosSinPago(int anio, int mes) => Array.Empty<int>();
    }
}
