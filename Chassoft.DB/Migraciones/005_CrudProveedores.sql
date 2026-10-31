USE [Chassoft];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
        THROW 50005, 'No existe Proveedores. Revisar las migraciones anteriores.', 1;
    -- No se borran ni se fusionan duplicados existentes: se solicita revisión.
    -- TABLOCKX mantiene estable la comprobación hasta crear el índice.
    IF EXISTS (SELECT Cuit FROM dbo.Proveedores WITH (TABLOCKX, HOLDLOCK) GROUP BY Cuit HAVING COUNT(*) > 1)
        THROW 50006, 'Hay CUIT duplicados en Proveedores. Revisarlos antes de aplicar 005. No se modificaron datos.', 1;
    IF COL_LENGTH(N'dbo.Proveedores', N'Activo') IS NULL
        ALTER TABLE dbo.Proveedores ADD Activo BIT NOT NULL CONSTRAINT DF_Proveedores_Activo DEFAULT (1) WITH VALUES;
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal=1
        JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
        WHERE i.object_id=OBJECT_ID(N'dbo.Proveedores') AND i.is_unique=1 AND i.has_filter=0 AND c.name=N'Cuit'
          AND NOT EXISTS (SELECT 1 FROM sys.index_columns k WHERE k.object_id=i.object_id AND k.index_id=i.index_id AND k.key_ordinal>1)
    )
        CREATE UNIQUE INDEX UX_Proveedores_Cuit ON dbo.Proveedores(Cuit);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
SELECT c.name AS Columna, TYPE_NAME(c.user_type_id) AS Tipo, c.is_nullable AS PermiteNull
FROM sys.columns c WHERE c.object_id=OBJECT_ID(N'dbo.Proveedores') AND c.name=N'Activo';
GO
