USE [master];
GO
SET NOCOUNT ON;
-- Script de transición, separado de las migraciones de esquema.
-- No crea una base vacía, no elimina registros y no fuerza desconexiones.
IF DB_ID(N'Chassoft') IS NOT NULL
BEGIN
    IF DB_ID(N'GestionAdministrativaBarracas') IS NOT NULL
        THROW 50010, 'Existen ambas bases. Se detuvo el cambio: revisar cuál contiene los datos antes de continuar.', 1;
    PRINT 'La base Chassoft ya existe. No se renombró ninguna otra base.';
END
ELSE
BEGIN
    IF DB_ID(N'GestionAdministrativaBarracas') IS NULL
        THROW 50011, 'No se encuentra la base de origen. Verificar la instancia y el nombre antes de continuar.', 1;
    IF EXISTS (SELECT 1 FROM sys.dm_exec_sessions
               WHERE database_id=DB_ID(N'GestionAdministrativaBarracas')
                 AND is_user_process=1 AND session_id<>@@SPID)
        THROW 50012, 'Cerrar las aplicaciones y las consultas conectadas a la base de origen, y volver a ejecutar desde master. No se forzó ninguna desconexión.', 1;
    ALTER DATABASE [GestionAdministrativaBarracas] MODIFY NAME = [Chassoft];
END;
GO
SELECT name AS BaseDeDatos, state_desc AS Estado, user_access_desc AS Acceso
FROM sys.databases WHERE name=N'Chassoft';
GO
