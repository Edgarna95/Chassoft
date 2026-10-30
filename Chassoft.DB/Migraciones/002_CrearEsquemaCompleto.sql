USE [Chassoft];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Migración 002: crea tablas faltantes, conserva las existentes.
   Se detiene ante un esquema antiguo de Socios que requiere conversión explícita. */

BEGIN TRY
    BEGIN TRANSACTION;

    -- No se recrea una tabla existente: se conservan filas, IDs y relaciones.
    IF OBJECT_ID(N'dbo.Socios', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Socios
        (
            Id INT NOT NULL IDENTITY(1,1) CONSTRAINT PK_Socios PRIMARY KEY,
            NombreCompleto NVARCHAR(200) NOT NULL,
            Dni NVARCHAR(20) NOT NULL CONSTRAINT UQ_Socios_Dni UNIQUE,
            FechaAlta DATE NOT NULL CONSTRAINT DF_Socios_FechaAlta DEFAULT (CAST(GETDATE() AS DATE)),
            Activo BIT NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1)
        );
    END
    ELSE IF ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.Socios'), N'Id', 'IsIdentity'), -1) <> 1
         OR COL_LENGTH(N'dbo.Socios', N'NombreCompleto') IS NULL
         OR COL_LENGTH(N'dbo.Socios', N'Dni') IS NULL
         OR COL_LENGTH(N'dbo.Socios', N'FechaAlta') IS NULL
         OR COL_LENGTH(N'dbo.Socios', N'Activo') IS NULL
    BEGIN
        THROW 50002, 'Socios tiene un esquema antiguo. Se detuvo la migración sin borrar datos. Requiere revisión y migración específica que preserve IDs y relaciones.', 1;
    END;

    -- ─────────────────────────────────────────────────────────────────────────
    -- 2. EMPLEADOS
    -- ─────────────────────────────────────────────────────────────────────────
    IF OBJECT_ID(N'dbo.Empleados', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Empleados
        (
            Id              INT             NOT NULL IDENTITY(1,1),
            NombreCompleto  NVARCHAR(200)   NOT NULL,
            Dni             NVARCHAR(20)    NOT NULL,
            Categoria       NVARCHAR(50)    NOT NULL,   -- 'Administrativo', 'Directivo', etc.
            FechaIngreso    DATE            NOT NULL,
            Activo          BIT             NOT NULL CONSTRAINT DF_Empleados_Activo DEFAULT (1),

            CONSTRAINT PK_Empleados         PRIMARY KEY (Id),
            CONSTRAINT UQ_Empleados_Dni     UNIQUE      (Dni)
        );
    END;

    -- ─────────────────────────────────────────────────────────────────────────
    -- 3. OPERACIONES — tabla base (datos comunes a Ingresos y Egresos)
    -- ─────────────────────────────────────────────────────────────────────────
    IF OBJECT_ID(N'dbo.Operaciones', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Operaciones
        (
            Id          INT             NOT NULL IDENTITY(1,1),
            Fecha       DATETIME2(0)    NOT NULL CONSTRAINT DF_Operaciones_Fecha DEFAULT (SYSUTCDATETIME()),
            Monto       DECIMAL(18,2)   NOT NULL,
            IdEmpleado  INT             NOT NULL,
            Tipo        NVARCHAR(10)    NOT NULL,   -- 'Ingreso' | 'Egreso'

            CONSTRAINT PK_Operaciones               PRIMARY KEY (Id),
            CONSTRAINT CK_Operaciones_Monto         CHECK (Monto > 0),
            CONSTRAINT CK_Operaciones_Tipo          CHECK (Tipo IN ('Ingreso', 'Egreso')),
            CONSTRAINT FK_Operaciones_Empleados     FOREIGN KEY (IdEmpleado)
                REFERENCES dbo.Empleados (Id)
        );
    END;

    -- ─────────────────────────────────────────────────────────────────────────
    -- 4. INGRESOS — detalle de cada cobro
    -- ─────────────────────────────────────────────────────────────────────────
    IF OBJECT_ID(N'dbo.Ingresos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Ingresos
        (
            Id          INT         NOT NULL IDENTITY(1,1),
            IdOperacion INT         NOT NULL,
            Tipo        INT         NOT NULL,       -- TipoIngreso: 1=Cuota, 2=Donacion
            IdSocio     INT         NULL,           -- obligatorio cuando Tipo = 1 (Cuota)

            CONSTRAINT PK_Ingresos                  PRIMARY KEY (Id),
            CONSTRAINT UQ_Ingresos_IdOperacion      UNIQUE      (IdOperacion),
            CONSTRAINT FK_Ingresos_Operaciones      FOREIGN KEY (IdOperacion)
                REFERENCES dbo.Operaciones (Id) ON DELETE CASCADE,
            CONSTRAINT FK_Ingresos_Socios           FOREIGN KEY (IdSocio)
                REFERENCES dbo.Socios (Id),
            -- Cuota requiere socio; Donación puede ser anónima
            CONSTRAINT CK_Ingresos_CuotaSocio       CHECK (Tipo <> 1 OR IdSocio IS NOT NULL)
        );
    END;

    -- ─────────────────────────────────────────────────────────────────────────
    -- 5. EGRESOS — detalle de cada pago
    -- ─────────────────────────────────────────────────────────────────────────
    IF OBJECT_ID(N'dbo.Egresos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Egresos
        (
            Id          INT         NOT NULL IDENTITY(1,1),
            IdOperacion INT         NOT NULL,
            Tipo        INT         NOT NULL,       -- TipoEgreso: 0=Servicios, 1=Proveedor, etc.
            IdProveedor INT         NOT NULL,

            CONSTRAINT PK_Egresos                   PRIMARY KEY (Id),
            CONSTRAINT UQ_Egresos_IdOperacion       UNIQUE      (IdOperacion),
            CONSTRAINT FK_Egresos_Operaciones       FOREIGN KEY (IdOperacion)
                REFERENCES dbo.Operaciones (Id) ON DELETE CASCADE,
            CONSTRAINT FK_Egresos_Proveedores       FOREIGN KEY (IdProveedor)
                REFERENCES dbo.Proveedores (Id)
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- Verificación post-migración
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    t.TABLE_NAME,
    COUNT(c.COLUMN_NAME) AS Columnas
FROM INFORMATION_SCHEMA.TABLES  t
JOIN INFORMATION_SCHEMA.COLUMNS c
    ON c.TABLE_NAME = t.TABLE_NAME AND c.TABLE_SCHEMA = t.TABLE_SCHEMA
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_SCHEMA = 'dbo'
GROUP BY t.TABLE_NAME
ORDER BY t.TABLE_NAME;
GO
