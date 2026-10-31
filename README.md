# Chassoft

Sistema en desarrollo para proveedores, socios y caja. Incluye una web Blazor y una interfaz WinForms que se mantiene operativa. La API todavía no tiene autenticación: esta versión se utiliza en un entorno local.

## Requisitos

- Windows, SDK .NET 9 y runtime .NET 8 (API y pruebas).
- .NET Framework 4.8 Developer Pack para compilar WinForms.
- SQL Server / LocalDB y una herramienta como SQL Server Management Studio.

## Compilar y probar

```powershell
dotnet restore Chassoft.sln
dotnet build Chassoft.sln
dotnet test Chassoft.Tests/Chassoft.Tests.csproj
```

La solución raíz incluye web, API, bibliotecas, tests y WinForms. Infrastructure ofrece netstandard2.0 para .NET Framework y netstandard2.1 para la API. El cliente SMTP solo se incluye en la variante del servidor. WinForms utiliza PackageReference: ya no necesita la carpeta local packages del proyecto antiguo.

## Base nueva

Ejecutar en orden, revisando el destino antes de cada archivo:

1. `Chassoft.DB/Migraciones/000_CrearBase.sql`
2. `001_AmpliarProveedores.sql`
3. `002_CrearEsquemaCompleto.sql`
4. `003_UsuariosCuotasNotificaciones.sql`
5. `004_VincularCuotasCaja.sql`
6. `005_CrudProveedores.sql`

La versión corregida de 002 conserva Socios. Si encuentra una estructura antigua incompatible, se detiene sin borrar datos; esa base requiere una conversión específica, conservando IDs y relaciones. No se debe usar la versión anterior que eliminaba la tabla.

Los datos de demostración están separados en `Chassoft.DB/DatosEjemplo`; son opcionales y exclusivos de bases de prueba. Una base real necesita sus empleados activos: el formulario de operaciones los consulta, no usa IDs ficticios.

## Instalación existente: transición a Chassoft

El proyecto, sus conexiones y las migraciones ahora usan la base **Chassoft**. Para conservar los datos de la instalación existente:

1. Cerrar la web, API, WinForms y las consultas de SSMS conectadas a la base anterior.
2. Abrir `scripts/RenombrarBaseAChassoft.sql` desde SSMS y ejecutarlo en la misma instancia LocalDB. El script se posiciona en master y cambia solo el nombre de la base. Se detiene si hay conexiones abiertas o si existen ambas bases. No fuerza cierres ni elimina registros.
3. Confirmar el resultado `Chassoft | ONLINE | MULTI_USER` y actualizar el Explorador de objetos de SSMS.
4. Abrir `Chassoft.sln` en la nueva carpeta del proyecto. Las conexiones entregadas ya apuntan a Chassoft; revisar también variables de entorno o configuraciones personales que puedan sobrescribirlas.

No ejecutar 000 para la instalación existente: crearía una base nueva en lugar de conservar la que contiene los datos. La migración 004 ya fue ejecutada por el usuario antes del renombrado y confirmó la columna IdOperacion; el cambio de nombre de base conserva ese esquema.

El usuario confirmó la ejecución del renombrado en SSMS: **Chassoft | ONLINE | MULTI_USER**.

Los nombres físicos de los archivos MDF/LDF pueden conservar el nombre histórico; no afectan al nombre de conexión ni se modifican con este script.

## Iniciar

```powershell
dotnet run --project Chassoft.API --launch-profile https
dotnet run --project Chassoft.Web --launch-profile https
```

API: https://localhost:7000 (Swagger en la raíz). Web: https://localhost:7001. Si corresponde, confiar en el certificado local con `dotnet dev-certs https --trust`.

WinForms: compilar desde Visual Studio o ejecutar `Chassoft.UI/bin/Debug/net48/Chassoft.UI.exe`. Mantiene su alta y listado de proveedores y su conexión SQL directa. La configuración SQL de escritorio se encuentra en App.config; la de API en appsettings.json o `ConnectionStrings__DefaultConnection`.

## Cuotas

- Canon inicial aprobado: ARS 10.000 por mes completo, configurable mediante `Cuotas:ImporteMensual` / `Cuotas__ImporteMensual`.
- Se pueden seleccionar varios meses y años en un cobro. El servidor calcula el total; el cliente no elige el precio mensual.
- Se registra un ingreso y un pago por cada período, dentro de una única transacción. Un mes duplicado revierte el cobro completo.
- Bonificaciones por familia y por cantidad quedan pendientes; por decisión del usuario no se aplican en esta versión.
- Las cuotas se registran en `POST /api/operacion/cuotas`; `POST /api/operacion/ingreso` admite donaciones, no cuotas sin detalle.
- `GET /api/socio/{id}/pagos` devuelve el historial mensual.
- Los nuevos movimientos usan UTC; las fechas históricas se recuperan sin reemplazarlas por la fecha actual.

## Correos

`Notificaciones:Enabled` está en **false**. No se envían correos al iniciar la API con la configuración entregada.

Al habilitarlos expresamente:

- Se revisa cada hora, solo los días 5 y 15, usando Argentina Standard Time. No se recuperan días perdidos, por decisión del usuario.
- Solo se consideran socios activos sin pago del mes y con email.
- Se reserva la notificación en SQL antes del SMTP. Una reserva existente no se reenvía automáticamente.
- `Pendiente` o `Revisar` exige conciliación manual: el sistema no puede garantizar si SMTP aceptó un mensaje antes de un corte. Revisar el correo/proveedor y el registro antes de cualquier reintento manual.
- `FechaEnvio` en reservas todavía pendientes identifica la creación de la reserva; en estados finales indica su última actualización.
- Configurar Host, Port, UseSsl, Username, Password y FromAddress. No guardar contraseñas reales en archivos versionados; usar variables de entorno como `Smtp__Password`.

## Prueba SQL optativa

La prueba de integración se omite si no hay `CHASSOFT_TEST_CONNECTION_STRING`. Para ejecutarla, preparar una **base aislada** llamada `ChassoftTest_...`, aplicar el esquema allí adaptando el nombre de base en los scripts y configurar esa variable. El test rechaza cualquier otro nombre; inserta registros propios y los elimina al finalizar. Comprueba cobro múltiple, reversión por duplicado y lectura histórica de un socio dado de baja. Nunca usar la base real.

## Límites actuales

- No se implementó login ni bonificaciones.
- WinForms mantiene Proveedores; los módulos nuevos de cuotas y caja se operan desde la web. El menú de escritorio que existía sigue siendo un borrador y no es la pantalla de inicio.
- Los datos de tarifas son una configuración inicial; aún no hay catálogo histórico por categoría/período.
- La ejecución SQL y SMTP reales requiere validación en el entorno del usuario. La compilación y las pruebas sin servidor no sustituyen esa verificación.
- No se hizo commit ni push de estas correcciones. GitHub Actions se ejecutará cuando se publique la rama.

## Preparación del índice de Git

El .gitignore está preparado. En la sesión de corrección, Windows denegó escribir `.git/index.lock` incluso después de conceder acceso a la carpeta; por eso los 289 archivos generados siguen en el índice. Desde una consola del usuario, `./scripts/PrepararGit.ps1` retira solamente bin/obj del seguimiento, conservándolos en disco. Revisar las eliminaciones del índice antes del commit. El script no hace commit ni push.

## Nombre e historial

Solución: Chassoft.sln. Proyectos, namespaces, ensamblados y recursos: Chassoft.*. La referencia al nombre anterior se conserva únicamente en el script de transición que necesita localizar la base existente. El historial de Git y los informes guardados fuera del proyecto se mantienen como antecedentes; no se reescribieron.

El remoto de GitHub aún conserva su dirección original. No se renombró ni publicó ningún repositorio remoto; esa decisión se coordinará al confirmar la subida.

## CRUD API: primera etapa

Socios y Proveedores tienen consulta por ID/documento, alta, edición, baja lógica y reactivación. Los listados admiten `?activo=true` o `?activo=false`; sin filtro incluyen ambos estados. DNI/CUIT conservan su unicidad incluso en registros inactivos.

**Aplicar la migración 005_CrudProveedores.sql antes de usar esta versión contra SQL.** Agrega el estado de proveedores y el índice único de CUIT, sin eliminar datos. Se detiene ante duplicados para revisión manual.

Rutas, reglas, ejemplos y secuencia de prueba: `docs/Crud_API_Socios_Proveedores.md` y `Chassoft.API/Chassoft.API.http`. Esta etapa prepara los contratos para las cargas desde WinForms; el cliente HTTP de escritorio se desarrollará después.

Migración 005: el usuario confirmó su ejecución en Chassoft con `Activo | bit | 0`.
