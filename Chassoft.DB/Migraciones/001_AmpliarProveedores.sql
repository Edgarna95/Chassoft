USE [Chassoft];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Solo agrega columnas. No elimina proveedores ni cambia datos existentes.
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
        THROW 50001, 'No existe dbo.Proveedores. Verificar la base y el esquema antes de continuar.', 1;

    IF COL_LENGTH(N'dbo.Proveedores', N'RazonSocial') IS NULL
        ALTER TABLE dbo.Proveedores ADD RazonSocial nvarchar(200) NULL;
    IF COL_LENGTH(N'dbo.Proveedores', N'TelefonoDeContacto') IS NULL
        ALTER TABLE dbo.Proveedores ADD TelefonoDeContacto nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.Proveedores', N'CorreoDeContacto') IS NULL
        ALTER TABLE dbo.Proveedores ADD CorreoDeContacto nvarchar(254) NULL;
    IF COL_LENGTH(N'dbo.Proveedores', N'CondicionIVA') IS NULL
        ALTER TABLE dbo.Proveedores ADD CondicionIVA nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.Proveedores', N'CbuAsociado') IS NULL
        ALTER TABLE dbo.Proveedores ADD CbuAsociado varchar(22) NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
-- Verificacion de estructura y cantidad, sin mostrar datos personales.
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE,
       COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS EsIdentity
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Proveedores'
ORDER BY ORDINAL_POSITION;

SELECT COUNT_BIG(*) AS CantidadProveedores FROM dbo.Proveedores;

-- Diagnostico de duplicados: no borra ni modifica registros.
SELECT COUNT_BIG(*) AS GruposDeCuitDuplicados
FROM (
    SELECT REPLACE(REPLACE(LTRIM(RTRIM(Cuit)), '-', ''), ' ', '') AS CuitNormalizado
    FROM dbo.Proveedores
    GROUP BY REPLACE(REPLACE(LTRIM(RTRIM(Cuit)), '-', ''), ' ', '')
    HAVING COUNT_BIG(*) > 1
) AS Duplicados;
GO
