# Chassoft: cambio de nombre del proyecto

Fecha: 30/10/2026.

## Cambios aplicados

- Carpeta principal: C:\Users\Edgar\Desktop\Chassoft.
- Solución principal: Chassoft.sln; también se actualizó la solución de escritorio.
- Carpetas y proyectos: Chassoft.API, Chassoft.DB, Chassoft.Dominio, Chassoft.Infrastructure, Chassoft.Tests, Chassoft.UI y Chassoft.Web.
- Referencias entre proyectos, namespaces, nombres de ensamblados, visibilidad interna para tests y recursos de WinForms actualizados.
- Nombre de producto y títulos visibles actualizados en WinForms, web, Swagger y correos. El CSS de componentes se genera como Chassoft.Web.styles.css.
- Configuración de API y WinForms preparada para Database=Chassoft. Migraciones y datos de demostración usan el nombre nuevo.
- Documentación, comandos de compilación, solicitudes HTTP de ejemplo y GitHub Actions actualizados.
- Variable de pruebas SQL: CHASSOFT_TEST_CONNECTION_STRING; base de pruebas aislada con prefijo ChassoftTest_.
- Script de limpieza del índice de Git actualizado para reconocer las carpetas generadas con sus nombres anteriores o actuales.

## Base existente

El script scripts/RenombrarBaseAChassoft.sql cambia el nombre de la base existente sin crear otra base vacía, borrar registros ni forzar desconexiones. Es idempotente y se detiene si encuentra ambas bases o conexiones abiertas.

La referencia al nombre anterior se conserva solamente en ese script de transición, porque es necesaria para encontrar la base existente. Los archivos físicos MDF/LDF no se renombran.

Estado confirmado por el usuario en SSMS: Chassoft | ONLINE | MULTI_USER. El renombrado de la base se completó después de liberar las consultas abiertas. La verificación funcional de datos y cobros sigue pendiente; no se ejecutó una prueba SQL desde la sesión del asistente.

## Verificación

- Compilación desde la nueva carpeta real: 0 errores, 8 advertencias de estilo de pruebas existentes.
- 39 pruebas: 38 aprobadas, 1 SQL omitida, 0 fallidas.
- Comprobación WinForms en .NET Framework 4.8: ensamblado Chassoft.UI, recursos renombrados y formularios cargan correctamente; no se abrieron ventanas ni se conectó a la base.
- API iniciada bajo Chassoft.API: Swagger muestra el nombre nuevo y la tarifa sigue siendo ARS 10.000.
- Web iniciada: título Chassoft y recurso Chassoft.Web.styles.css disponibles por HTTP.
- Búsqueda de referencias antiguas: solo aparecen en el script de transición de base. No quedan nombres anteriores en archivos ni carpetas activos del código fuente.

## Preservación e historial

Se preservaron las fuentes anteriores al renombrado y se apartaron del proyecto los artefactos generados, la caché de Visual Studio y el ZIP histórico. Los nuevos bin/obj se regeneran con los nombres de Chassoft. Los antecedentes y respaldos del chat permanecen fuera del proyecto.

El historial de Git se conserva. No se hizo commit ni push, ni se cambió o renombró el repositorio remoto. El índice todavía conserva los archivos generados de la versión anterior: su limpieza quedó pendiente por el permiso de escritura de .git/index.lock en esta sesión, como ya estaba documentado.

## Para abrir y continuar

Abrir C:\Users\Edgar\Desktop\Chassoft\Chassoft.sln en Visual Studio. Las rutas anteriores ya no son las del proyecto activo. Revisar cualquier configuración personal o variable de entorno externa que todavía sobrescriba el nombre de la base.

La prueba completa contra SQL sigue pendiente desde el entorno del usuario; el cambio de nombre no sustituye la validación funcional de cobros.
