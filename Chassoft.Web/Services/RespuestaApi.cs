using System.Net.Http.Json;
using System.Text.Json;
namespace Chassoft.Web.Services;
public static class RespuestaApi
{
    public static async Task VerificarAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        string mensaje = "No se pudo completar la operación.";
        try
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (body.TryGetProperty("mensaje", out var texto)) mensaje = texto.GetString() ?? mensaje;
            else if (body.TryGetProperty("errors", out var errores))
                mensaje = string.Join(" ", errores.EnumerateObject().SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString())));
        }
        catch (JsonException) { }
        throw new HttpRequestException(mensaje, null, response.StatusCode);
    }
}
