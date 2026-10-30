USE [Chassoft];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

/*
  Migración 003 — Usuarios, PagosCuota, Notificaciones + Email en Socios
*/

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Agregar Email a Socios
    IF COL_LENGTH(N'dbo.Socios', N'Email') IS NULL
        ALTER TABLE dbo.Socios ADD Email NVARCHAR(254) NULL;

    -- 2. Agregar Email a Empleados (ya existe la columna en código, pero no en DB)
    IF COL_LENGTH(N'dbo.Empleados', N'Email') IS NULL
        ALTER TABLE dbo.Empleados ADD Email NVARCHAR(254) NULL;

    IF COL_LENGTH(N'dbo.Empleados', N'Telefono') IS NULL
        ALTER TABLE dbo.Empleados ADD Telefono NVARCHAR(50) NULL;

    -- 3. Tabla Usuarios (referencia futura para login v1.1)
    IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Usuarios
        (
            Id              INT             NOT NULL IDENTITY(1,1),
            IdEmpleado      INT             NOT NULL,
            NombreUsuario   NVARCHAR(100)   NOT NULL,
            Rol             NVARCHAR(50)    NOT NULL,
            Activo          BIT             NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
            CONSTRAINT PK_Usuarios              PRIMARY KEY (Id),
            CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
            CONSTRAINT UQ_Usuarios_IdEmpleado    UNIQUE (IdEmpleado),
            CONSTRAINT FK_Usuarios_Empleados     FOREIGN KEY (IdEmpleado)
                REFERENCES dbo.Empleados (Id),
            CONSTRAINT CK_Usuarios_Rol          CHECK (Rol IN ('Administrador','Directivo','Administrativo','Maestranza'))
        );
    END;

    -- 4. Tabla PagosCuota
    IF OBJECT_ID(N'dbo.PagosCuota', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PagosCuota
        (
            Id              INT             NOT NULL IDENTITY(1,1),
            IdSocio         INT             NOT NULL,
            Anio            SMALLINT        NOT NULL,
            Mes             TINYINT         NOT NULL,
            MontoAbonado    DECIMAL(10,2)   NOT NULL,
            FechaPago       DATETIME2(0)    NOT NULL CONSTRAINT DF_PagosCuota_Fecha DEFAULT (SYSUTCDATETIME()),
            IdEmpleado      INT             NOT NULL,
            CONSTRAINT PK_PagosCuota            PRIMARY KEY (Id),
            CONSTRAINT UQ_PagosCuota_SocioMes   UNIQUE (IdSocio, Anio, Mes),
            CONSTRAINT CK_PagosCuota_Mes        CHECK (Mes BETWEEN 1 AND 12),
            CONSTRAINT CK_PagosCuota_Monto      CHECK (MontoAbonado > 0),
            CONSTRAINT FK_PagosCuota_Socios     FOREIGN KEY (IdSocio)  REFERENCES dbo.Socios    (Id),
            CONSTRAINT FK_PagosCuota_Empleados  FOREIGN KEY (IdEmpleado) REFERENCES dbo.Empleados (Id)
        );
    END;

    -- 5. Tabla Notificaciones (registro de emails enviados)
    IF OBJECT_ID(N'dbo.Notificaciones', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Notificaciones
        (
            Id              INT             NOT NULL IDENTITY(1,1),
            IdSocio         INT             NOT NULL,
            Tipo            NVARCHAR(30)    NOT NULL,   -- 'RecordatorioCuota' | 'AvisoMora'
            Anio            SMALLINT        NOT NULL,
            Mes             TINYINT         NOT NULL,
            Destinatario    NVARCHAR(254)   NOT NULL,
            FechaEnvio      DATETIME2(0)    NOT NULL CONSTRAINT DF_Notif_Fecha DEFAULT (SYSUTCDATETIME()),
            Estado          NVARCHAR(20)    NOT NULL CONSTRAINT DF_Notif_Estado DEFAULT ('Enviado'),
            CONSTRAINT PK_Notificaciones        PRIMARY KEY (Id),
            CONSTRAINT CK_Notif_Tipo            CHECK (Tipo IN ('RecordatorioCuota','AvisoMora')),
            CONSTRAINT CK_Notif_Mes             CHECK (Mes BETWEEN 1 AND 12),
            -- Máximo 1 notificación de cada tipo por socio por mes
            CONSTRAINT UQ_Notif_SocioTipoMes    UNIQUE (IdSocio, Tipo, Anio, Mes),
            CONSTRAINT FK_Notif_Socios          FOREIGN KEY (IdSocio) REFERENCES dbo.Socios (Id)
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
