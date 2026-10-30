USE [Chassoft];
GO
-- SOLO para bases de demostración. No ejecutar sobre la base real.
-- ─── SEED DATA ───────────────────────────────────────────────────────────────
-- Solo inserta si no existen (idempotente)

-- Empleados
IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE Dni = '20111222')
    INSERT INTO dbo.Empleados (NombreCompleto, Dni, Categoria, FechaIngreso, Activo, Email)
    VALUES ('Roberto Mendez', '20111222', 'Directivo', '2020-03-01', 1, 'directivo@chassoft.test');

IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE Dni = '27333444')
    INSERT INTO dbo.Empleados (NombreCompleto, Dni, Categoria, FechaIngreso, Activo, Email)
    VALUES ('Laura Gomez', '27333444', 'Administrativo', '2021-06-15', 1, 'laura@chassoft.test');

IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE Dni = '30555666')
    INSERT INTO dbo.Empleados (NombreCompleto, Dni, Categoria, FechaIngreso, Activo, Email)
    VALUES ('Carlos Ruiz', '30555666', 'Maestranza', '2022-01-10', 1, 'carlos@chassoft.test');

IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE Dni = '99999999')
    INSERT INTO dbo.Empleados (NombreCompleto, Dni, Categoria, FechaIngreso, Activo, Email)
    VALUES ('Admin Sistema', '99999999', 'Administrativo', '2020-01-01', 1, 'sysadmin@chassoft.test');

-- Usuarios (vinculados a empleados)
INSERT INTO dbo.Usuarios (IdEmpleado, NombreUsuario, Rol, Activo)
SELECT e.Id, 'directivo', 'Directivo', 1
FROM dbo.Empleados e WHERE e.Dni = '20111222'
  AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.NombreUsuario = 'directivo');

INSERT INTO dbo.Usuarios (IdEmpleado, NombreUsuario, Rol, Activo)
SELECT e.Id, 'lgomez', 'Administrativo', 1
FROM dbo.Empleados e WHERE e.Dni = '27333444'
  AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.NombreUsuario = 'lgomez');

INSERT INTO dbo.Usuarios (IdEmpleado, NombreUsuario, Rol, Activo)
SELECT e.Id, 'cruiz', 'Maestranza', 1
FROM dbo.Empleados e WHERE e.Dni = '30555666'
  AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.NombreUsuario = 'cruiz');

INSERT INTO dbo.Usuarios (IdEmpleado, NombreUsuario, Rol, Activo)
SELECT e.Id, 'admin', 'Administrador', 1
FROM dbo.Empleados e WHERE e.Dni = '99999999'
  AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.NombreUsuario = 'admin');

-- Socios de prueba
IF NOT EXISTS (SELECT 1 FROM dbo.Socios WHERE Dni = '25100200')
    INSERT INTO dbo.Socios (NombreCompleto, Dni, FechaAlta, Activo, Email)
    VALUES ('Martin Sosa', '25100200', '2022-04-10', 1, 'martin@example.test');

IF NOT EXISTS (SELECT 1 FROM dbo.Socios WHERE Dni = '28300400')
    INSERT INTO dbo.Socios (NombreCompleto, Dni, FechaAlta, Activo, Email)
    VALUES ('Ana Blanco', '28300400', '2021-09-01', 1, 'ana@example.test');

IF NOT EXISTS (SELECT 1 FROM dbo.Socios WHERE Dni = '32500600')
    INSERT INTO dbo.Socios (NombreCompleto, Dni, FechaAlta, Activo, Email)
    VALUES ('Diego Vera', '32500600', '2020-11-20', 1, 'diego@example.test');

-- PagosCuota: Martin Sosa pagó octubre 2026 (al día)
INSERT INTO dbo.PagosCuota (IdSocio, Anio, Mes, MontoAbonado, FechaPago, IdEmpleado)
SELECT s.Id, 2026, 10, 5000.00, '2026-10-03 10:00:00', e.Id
FROM dbo.Socios s, dbo.Empleados e
WHERE s.Dni = '25100200' AND e.Dni = '27333444'
  AND NOT EXISTS (SELECT 1 FROM dbo.PagosCuota p WHERE p.IdSocio = s.Id AND p.Anio = 2026 AND p.Mes = 10);

-- Martin Sosa pagó septiembre 2026 también
INSERT INTO dbo.PagosCuota (IdSocio, Anio, Mes, MontoAbonado, FechaPago, IdEmpleado)
SELECT s.Id, 2026, 9, 5000.00, '2026-09-04 09:30:00', e.Id
FROM dbo.Socios s, dbo.Empleados e
WHERE s.Dni = '25100200' AND e.Dni = '27333444'
  AND NOT EXISTS (SELECT 1 FROM dbo.PagosCuota p WHERE p.IdSocio = s.Id AND p.Anio = 2026 AND p.Mes = 9);

-- Ana Blanco pagó septiembre pero NO octubre (vencida 1 mes)
INSERT INTO dbo.PagosCuota (IdSocio, Anio, Mes, MontoAbonado, FechaPago, IdEmpleado)
SELECT s.Id, 2026, 9, 5000.00, '2026-09-07 11:00:00', e.Id
FROM dbo.Socios s, dbo.Empleados e
WHERE s.Dni = '28300400' AND e.Dni = '27333444'
  AND NOT EXISTS (SELECT 1 FROM dbo.PagosCuota p WHERE p.IdSocio = s.Id AND p.Anio = 2026 AND p.Mes = 9);

-- Diego Vera no pagó ni septiembre ni octubre (moroso 2 meses) — sin inserts de PagosCuota

GO

-- Verificación
SELECT 'Empleados' AS Tabla, COUNT(*) AS Total FROM dbo.Empleados
UNION ALL SELECT 'Usuarios',       COUNT(*) FROM dbo.Usuarios
UNION ALL SELECT 'Socios',         COUNT(*) FROM dbo.Socios
UNION ALL SELECT 'PagosCuota',     COUNT(*) FROM dbo.PagosCuota
UNION ALL SELECT 'Notificaciones', COUNT(*) FROM dbo.Notificaciones;
GO
