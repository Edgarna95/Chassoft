# CRUD de Socios y Proveedores en la API

Primera etapa acordada: completar la API; después integrar las cargas de WinForms a través de HTTP y finalmente desarrollar consultas y reportes web. Esta etapa no agrega formularios nuevos de escritorio ni modifica Empleados.

## Rutas comunes

Usar `socio` o `proveedor` en lugar de `{recurso}`.

| Método | Ruta | Resultado |
|---|---|---|
| GET | /api/{recurso} | 200, listado completo |
| GET | /api/{recurso}?activo=true | 200, activos |
| GET | /api/{recurso}?activo=false | 200, inactivos |
| GET | /api/{recurso}/{id} | 200 o 404 |
| POST | /api/{recurso} | 201, entidad con ID y Location |
| PUT | /api/{recurso}/{id} | 204 o 404 |
| DELETE | /api/{recurso}/{id} | 204, baja lógica; 404 si no existe |
| POST | /api/{recurso}/{id}/reactivar | 204 o 404 |

También se ofrecen GET /api/socio/por-dni?dni=..., GET /api/proveedor/por-cuit?cuit=... y GET /api/socio/{id}/pagos. El endpoint POST /api/socio/{id}/baja se conserva por compatibilidad, aunque se recomienda DELETE.

## Reglas

- DELETE no elimina filas: cambia Activo a false. Repetir una baja o reactivación sobre un registro existente devuelve 204.
- Consultar por ID o documento permite encontrar registros inactivos. El listado sin filtro incluye ambos estados.
- Editar datos no cambia el ID, la fecha de alta del socio ni el estado; se reactiva únicamente con la operación explícita.
- DNI y CUIT no se reutilizan aunque el registro esté de baja. Los duplicados devuelven 409. Un registro de baja debe reactivarse.
- El cuerpo de POST/PUT contiene datos personales o comerciales, sin ID, fecha de alta ni estado. PUT reemplaza todos los datos editables: un opcional omitido se guarda vacío/NULL.
- IDs no positivos, obligatorios vacíos, longitudes inválidas y formatos de correo/CBU inválidos devuelven 400. Se mantienen las reglas actuales de DNI/CUIT, sin incorporar una nueva validación de dígito verificador.
- La API devuelve errores de negocio en `mensaje`; los errores de validación automática contienen `errors` (ValidationProblemDetails).
- No se crean egresos nuevos para proveedores inactivos. Su historial sigue siendo consultable, con su ID y estado recuperados. La comprobación se repite dentro de la transacción SQL para evitar usar un estado obsoleto.

## Base de datos

Aplicar Chassoft.DB/Migraciones/005_CrudProveedores.sql antes de usar esta versión con la base real. Agrega Proveedores.Activo BIT NOT NULL, inicializa los proveedores existentes como activos y asegura un índice único por CUIT. Si encuentra duplicados se detiene y revierte, sin borrarlos ni fusionarlos.

La migración no cambia Socios, cuotas ni movimientos. El estado se conserva mediante bajas lógicas, no borrado físico. Esta API requiere la migración; no debe iniciarse contra un esquema anterior si se van a consultar proveedores o movimientos asociados.

## Probar

1. Aplicar 005 desde SSMS en la base Chassoft y confirmar Activo | bit | 0.
2. Desde la raíz: `dotnet run --project Chassoft.API --launch-profile https`.
3. Abrir https://localhost:7000 y usar Swagger. Los ejemplos están en Chassoft.API/Chassoft.API.http. No usar los IDs de ejemplo sin sustituirlos por los devueltos en cada alta.
4. Crear, consultar, editar, dar de baja, confirmar que sigue siendo consultable y reactivar. Repetir documento para comprobar el 409.

Los ejemplos HTTP producen escrituras cuando se ejecutan: usarlos con registros de prueba identificables, preferentemente en una base aislada. No ejecutarlos todos automáticamente sobre datos existentes.

## Verificación y próximos pasos

Las pruebas HTTP levantan Kestrel con los controladores reales y repositorios en memoria. Verifican rutas, códigos, binding, validaciones, filtros, duplicados, preservación de estado e historial y bloqueo de egresos a proveedores inactivos. No sustituyen pruebas del motor SQL.

Las pruebas SQL son optativas mediante CHASSOFT_TEST_CONNECTION_STRING y solo aceptan bases ChassoftTest_. Requieren también 005. No se ejecutan por defecto contra la base real.

WinForms continúa usando SQL directamente en el módulo existente; la siguiente etapa será incorporar un cliente HTTP para este CRUD. La web existente se mantiene compatible y su selector de proveedores excluye los inactivos. Empleados, paginación, reportes y autenticación quedan fuera de esta primera etapa.

No se hizo commit ni push automático de estos cambios.

## Estado confirmado al cierre

- El usuario ejecutó 005 en SSMS y confirmó `Activo | bit | 0`.
- Compilación Release de toda la solución, incluido WinForms: 0 errores y 0 advertencias.
- 60 pruebas: 58 aprobadas, 2 SQL omitidas, 0 fallidas. Las pruebas HTTP del CRUD usan controladores reales y repositorios en memoria.
- Falta verificar el circuito contra el SQL del usuario desde Swagger. Las pruebas aisladas del motor requieren CHASSOFT_TEST_CONNECTION_STRING.
- Cambios locales, sin commit ni push automático.
