# Chassoft: correcciones y estado al 30/10/2026

## Decisiones confirmadas por el usuario

- Se mantienen la web y WinForms.
- Cuota inicial: ARS 10.000 por mes completo. Se pueden pagar varios períodos en un solo cobro.
- Un ingreso de caja por cobro, con un pago detallado por mes. Sin descuentos automáticos en esta versión; las bonificaciones quedan pendientes de definición.
- Correos automáticos desactivados hasta su habilitación expresa. Al habilitarlos, se ejecutan únicamente los días 5 y 15; no se recuperan días perdidos.
- No se hará commit ni push hasta la confirmación del usuario.

## Correcciones aplicadas al proyecto original

1. Las operaciones guardan el Tipo obligatorio y recuperan su ID y fecha histórica. Los ingresos reconstruyen el socio con su identidad, fecha de alta, estado y email; un socio dado de baja después del cobro no impide leer su historia. Los egresos conservan todos los campos del proveedor.
2. Cobro de varios meses completos: tarifa calculada por el servidor, validación de períodos repetidos, socio y empleado activos, y una transacción para el ingreso, el detalle y todos los pagos. Si un período ya está pagado, se revierte todo el cobro. El alta genérica de ingresos rechaza cuotas sin períodos.
3. La API de proveedores recibe todos los campos del formulario y permite edición. La web tiene un botón de edición. Se validan correo, longitudes y CBU de 22 dígitos.
4. El email de socios se captura en web y API. Se agregaron consultas del historial de pagos y baja lógica en API.
5. Operaciones web ya no es una página en construcción: permite donaciones, egresos y cobros de cuotas; consulta empleados activos, socios y proveedores. Un error al refrescar el listado después de confirmar el cobro no invita a volver a cobrar.
6. Los errores SQL de Socios y los duplicados de cuotas se clasifican específicamente. Las validaciones monetarias de API funcionan con la configuración regional argentina; se corrigió un error de interpretación del punto decimal.
7. Notificaciones desactivadas por configuración. La reserva se registra antes del envío SMTP, evitando reintentos automáticos ante resultados inciertos. Las notificaciones Pendiente/Revisar necesitan conciliación manual. La zona horaria es Argentina Standard Time.
8. WinForms conserva .NET Framework 4.8 y sus formularios de Proveedores. Su proyecto usa PackageReference y la biblioteca compartida ofrece una variante netstandard2.0 compatible, además de la variante de servidor. WinForms fue agregado a la solución principal.
9. Tests migrados a .NET 8, sin las incompatibilidades de dependencias con net6.0. Se agregaron pruebas de reconstrucción histórica, cuotas, contratos API, cultura argentina y reservas de notificaciones.
10. La API usa registros de consola/depuración, sin depender de permisos de escritura en el registro de eventos de Windows.
11. Migraciones: 000 permite crear una base nueva y Proveedores; 002 conserva Socios y se detiene ante un esquema antiguo incompatible; los datos de ejemplo se separaron de 003 y usan dominios de prueba. 004 agrega el vínculo entre PagosCuota y Operaciones sin inventar relaciones históricas.
12. Se agregaron .gitignore raíz, README, comprobación de build/tests para GitHub Actions y un script local para retirar bin/obj del índice sin borrarlos.

## Estado de la base real

El usuario conectó desde SSMS y proporcionó el esquema: Socios conserva IDENTITY, fechas, estado y email; Proveedores contiene sus campos ampliados; ya existen Operaciones, PagosCuota y Notificaciones.

El usuario ejecutó el script 004 entregado y mostró el resultado: IdOperacion | int | PermiteNull = 1. Esto confirma la nueva columna. Los pagos históricos conservan sus datos y su relación de caja puede permanecer NULL. No se ejecutaron escrituras sobre la base desde herramientas del asistente.

La sesión del asistente sigue sin acceder a la instancia LocalDB del usuario. No se pudo ejecutar la prueba SQL contra un servidor accesible; tampoco se comprobó desde esta sesión la clave foránea ni un cobro real. La captura confirma la columna, no sustituye una prueba completa del circuito.

## Verificación realizada

- Copia de trabajo compilada con SDK 9.0.311: solución principal completa, incluidos API, web y WinForms. Resultado final: 0 errores y 8 advertencias de recomendaciones de estilo en pruebas existentes.
- Pruebas automatizadas: 39 totales, 38 aprobadas, 1 omitida, 0 fallidas. La omitida requiere una base SQL aislada.
- Comprobación de arranque de componentes WinForms en .NET Framework 4.8: se construyen los formularios, se verifican los siete campos y el listado, y se carga el repositorio SQL. Sin abrir ventanas ni acceder a datos.
- Pruebas HTTP sobre la API iniciada localmente: tarifa ARS 10.000, rechazo con 400 de correo/CBU inválidos y tipo de ingreso inexistente, y presencia de los endpoints nuevos en Swagger. No se enviaron correos ni se registraron movimientos reales.
- git diff --check no encontró errores de espacios; hubo avisos de conversión de finales de línea ya existentes.

## Pendientes antes de dar por validado el circuito completo

1. Probar en una base aislada el cobro de dos o más meses y la reversión del cobro entero si uno ya estaba pagado. El test de integración ya existe, requiere CHASSOFT_TEST_CONNECTION_STRING y rechaza una base que no comience con ChassoftTest_.
2. Verificar en el entorno del usuario la navegación y un alta/edición de proveedor y socio. WinForms debe probar también su conexión real a SQL; no basta con la construcción de formularios.
3. Git: los 289 archivos bin/obj siguen versionados. Windows denegó crear .git/index.lock incluso después de conceder acceso a la carpeta. No se alteró el índice. Desde una consola del usuario, scripts/PrepararGit.ps1 prepara exclusivamente esas eliminaciones del índice, conserva los archivos en disco y no hace commit ni push.
4. Comparar el estado con origin y revisar la selección final de archivos antes del commit. El remoto configurado sigue siendo https://github.com/Edgarna95/software_para_club_de_barrio.git, rama local main. No se consultó ni modificó el remoto.
5. Confirmación del usuario para publicar. No se creó ningún commit ni se subió nada.

## Funciones pendientes de una versión posterior

Bonificaciones familiares y por cantidad de meses, login y roles, catálogo histórico de tarifas y módulos nuevos equivalentes en escritorio. WinForms mantiene el módulo Proveedores existente; el menú de escritorio sigue siendo un borrador. Los correos SMTP reales no fueron probados y permanecen apagados.

## Archivos de referencia

Proyecto: C:\Users\Edgar\Desktop\Chassoft.
Documentación de inicio: README.md.
Migración aplicada por el usuario: Chassoft.DB/Migraciones/004_VincularCuotasCaja.sql.
Prueba SQL: Chassoft.Tests/Integracion/CajaSqlTests.cs.
Limpieza pendiente de índice: scripts/PrepararGit.ps1.
Se preservó una copia de las fuentes previas a esta tarea en el directorio de trabajo del chat. El trabajo que ya tenía el usuario sin confirmar en Git no se revirtió.
