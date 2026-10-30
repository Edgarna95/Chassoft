USE [Chassoft];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.PagosCuota', N'U') IS NULL OR OBJECT_ID(N'dbo.Operaciones', N'U') IS NULL
        THROW 50004, 'Faltan PagosCuota u Operaciones. Revisar las migraciones previas.', 1;
    IF COL_LENGTH(N'dbo.PagosCuota', N'IdOperacion') IS NULL
        ALTER TABLE dbo.PagosCuota ADD IdOperacion INT NULL;
    -- Los pagos históricos se conservan sin inventar su movimiento de caja.
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PagosCuota_Operacion' AND parent_object_id=OBJECT_ID(N'dbo.PagosCuota'))
        EXEC(N'ALTER TABLE dbo.PagosCuota WITH CHECK ADD CONSTRAINT FK_PagosCuota_Operacion FOREIGN KEY (IdOperacion) REFERENCES dbo.Operaciones(Id)');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
SELECT c.name AS Columna, TYPE_NAME(c.user_type_id) AS Tipo, c.is_nullable AS PermiteNull
FROM sys.columns c WHERE c.object_id=OBJECT_ID(N'dbo.PagosCuota') AND c.name=N'IdOperacion';
GO
