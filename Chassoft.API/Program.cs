using Chassoft.API.Jobs;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Repositorios;
using Chassoft.Dominio.Servicios;
using Chassoft.Infrastructure.Repositories;
using Chassoft.Infrastructure.Servicios;

var builder = WebApplication.CreateBuilder(args);
// No requiere permisos de escritura sobre el registro de eventos de Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// -- CORS -------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// -- Repositorios -----------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");

builder.Services.AddScoped<ISocioRepository>(     _ => new SocioRepositorySqlServer(cs));
builder.Services.AddScoped<IProveedorRepository>( _ => new ProveedorRepositorySqlServer(cs));
builder.Services.AddScoped<IOperacionRepository>( _ => new OperacionRepositorySqlServer(cs));
builder.Services.AddScoped<IPagosCuotaRepository>(_ => new PagosCuotaRepositorySqlServer(cs));
builder.Services.AddScoped<INotificacionRepository>(_ => new NotificacionRepositorySqlServer(cs));

builder.Services.AddScoped<IEmpleadoRepository>(_ => new EmpleadoRepositorySqlServer(cs));

// -- Servicios de aplicacion ------------------------------------------------
builder.Services.AddScoped<INotificacionEmailService, SmtpNotificacionEmailService>();

// -- Background job ---------------------------------------------------------
if (builder.Configuration.GetValue<bool>("Notificaciones:Enabled"))
    builder.Services.AddHostedService<VerificarCuotasJob>();

// -- Swagger ----------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -- Pipeline ---------------------------------------------------------------
var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var sql = error as Microsoft.Data.SqlClient.SqlException;
    var status = sql?.Number == 547 ? 400 : (sql?.Number == 2601 || sql?.Number == 2627 ? 409 : 500);
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new {
        mensaje = status == 400 ? "Verificá las referencias y los datos del movimiento. No se guardaron cambios."
            : status == 409 ? "Ya existe un registro con esos datos."
            : "No se pudo completar la operación. Revisá la conexión y consultá el registro del servidor."
    });
}));

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chassoft.API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("BlazorClient");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
