USE master;
GO
IF DB_ID(N'Chassoft') IS NULL
    EXEC(N'CREATE DATABASE [Chassoft]');
GO
USE [Chassoft];
GO
IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Proveedores
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Proveedores PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Cuit NVARCHAR(20) NOT NULL
    );
END;
GO
