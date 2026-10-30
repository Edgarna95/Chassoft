using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Chassoft.Web;
using Chassoft.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URL base de la API — en producción usar variable de entorno
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7000/")
});

// Servicios del cliente
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<SocioService>();
builder.Services.AddScoped<OperacionService>();

await builder.Build().RunAsync();
