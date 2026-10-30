using System.Net.Http.Json;
using Chassoft.Web.Models;

namespace Chassoft.Web.Services;

public class ProveedorService(HttpClient http)
{
    public Task<List<ProveedorDto>?> ObtenerTodosAsync()
        => http.GetFromJsonAsync<List<ProveedorDto>>("api/proveedor");

    public Task<ProveedorDto?> ObtenerPorIdAsync(int id)
        => http.GetFromJsonAsync<ProveedorDto>($"api/proveedor/{id}");

    public async Task<ProveedorDto?> CrearAsync(CrearProveedorRequest request)
    {
        using var response = await http.PostAsJsonAsync("api/proveedor", request);
        await RespuestaApi.VerificarAsync(response);
        return await response.Content.ReadFromJsonAsync<ProveedorDto>();
    }
    public async Task ActualizarAsync(int id, CrearProveedorRequest request)
    {
        using var response = await http.PutAsJsonAsync($"api/proveedor/{id}", request);
        await RespuestaApi.VerificarAsync(response);
    }
}
