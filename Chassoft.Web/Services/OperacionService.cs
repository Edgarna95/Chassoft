using System.Net.Http.Json;
using Chassoft.Web.Models;
namespace Chassoft.Web.Services;
public class OperacionService(HttpClient http)
{
    public async Task<List<OperacionDto>> ObtenerAsync() => await http.GetFromJsonAsync<List<OperacionDto>>("api/operacion") ?? [];
    public async Task<List<EmpleadoDto>> EmpleadosAsync() => await http.GetFromJsonAsync<List<EmpleadoDto>>("api/empleado") ?? [];
    public async Task<decimal> TarifaAsync() => (await http.GetFromJsonAsync<TarifaCuotaDto>("api/operacion/tarifa-cuota"))!.ImporteMensual;
    public async Task GuardarAsync(string ruta, object pedido)
    {
        using var response = await http.PostAsJsonAsync("api/operacion/" + ruta, pedido);
        await RespuestaApi.VerificarAsync(response);
    }
}
