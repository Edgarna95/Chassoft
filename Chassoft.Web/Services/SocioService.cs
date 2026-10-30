using System.Net.Http.Json;
using Chassoft.Web.Models;

namespace Chassoft.Web.Services;

public class SocioService(HttpClient http)
{
    public Task<List<SocioDto>?> ObtenerTodosAsync()
        => http.GetFromJsonAsync<List<SocioDto>>("api/socio");

    public async Task<bool> CrearAsync(CrearSocioRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/socio", request);
        await RespuestaApi.VerificarAsync(response);
        return true;
    }
}
