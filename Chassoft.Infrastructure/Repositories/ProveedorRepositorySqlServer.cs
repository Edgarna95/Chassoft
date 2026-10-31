using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;
using Chassoft.Dominio.Excepciones;

namespace Chassoft.Infrastructure.Repositories
{
    public class ProveedorRepositorySqlServer : IProveedorRepository
    {
        private const string Columnas = "Id, Nombre, Cuit, RazonSocial, TelefonoDeContacto, CorreoDeContacto, CondicionIVA, CbuAsociado, Activo";
        private readonly string _connectionString;

        public ProveedorRepositorySqlServer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Proveedor Agregar(Proveedor proveedor)
        {
            if (proveedor == null) throw new ArgumentNullException(nameof(proveedor));
            if (proveedor.Id != 0) throw new ArgumentException("El proveedor ya tiene identidad", nameof(proveedor));
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"INSERT INTO dbo.Proveedores
                (Nombre, Cuit, RazonSocial, TelefonoDeContacto, CorreoDeContacto, CondicionIVA, CbuAsociado)
                OUTPUT INSERTED.Id
                VALUES (@nombre, @cuit, @razonSocial, @telefono, @correo, @condicionIVA, @cbu)", connection))
            {
                AgregarParametros(command, proveedor);
                connection.Open();
                int id;
                try { id = Convert.ToInt32(command.ExecuteScalar()); }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                { throw new RegistroDuplicadoException("Ya existe un proveedor registrado con el CUIT ingresado. Si está inactivo, reactivalo."); }
                return new Proveedor(id, proveedor.Nombre, proveedor.Cuit, proveedor.RazonSocial,
                    proveedor.TelefonoDeContacto, proveedor.CorreoDeContacto,
                    proveedor.CondicionIVA, proveedor.CbuAsociado);
            }
        }

        public IEnumerable<Proveedor> ObtenerTodos(bool? activo = null)
        {
            var lista = new List<Proveedor>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT " + Columnas + " FROM dbo.Proveedores WHERE (@activo IS NULL OR Activo=@activo) ORDER BY Nombre, Id", connection))
            {
                command.Parameters.Add("@activo", SqlDbType.Bit).Value = (object?)activo ?? DBNull.Value;
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) lista.Add(LeerProveedor(reader));
            }
            return lista;
        }

        public Proveedor? ObtenerPorCuit(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit) || cuit.Trim().Length > 20)
                throw new ArgumentException("El CUIT es obligatorio y admite hasta 20 caracteres", nameof(cuit));
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT " + Columnas + " FROM dbo.Proveedores WHERE Cuit = @cuit", connection))
            {
                command.Parameters.Add("@cuit", SqlDbType.NVarChar, 20).Value = cuit.Trim();
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read() ? LeerProveedor(reader) : null;
            }
        }

        public Proveedor? ObtenerPorId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT " + Columnas + " FROM dbo.Proveedores WHERE Id = @id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read() ? LeerProveedor(reader) : null;
            }
        }

        public bool Actualizar(Proveedor proveedor)
        {
            if (proveedor == null) throw new ArgumentNullException(nameof(proveedor));
            if (proveedor.Id <= 0) throw new ArgumentException("El proveedor debe tener identidad para actualizarse", nameof(proveedor));
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"UPDATE dbo.Proveedores SET
                Nombre = @nombre, Cuit = @cuit, RazonSocial = @razonSocial,
                TelefonoDeContacto = @telefono, CorreoDeContacto = @correo,
                CondicionIVA = @condicionIVA, CbuAsociado = @cbu
                WHERE Id = @id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = proveedor.Id;
                AgregarParametros(command, proveedor);
                connection.Open();
                try { return command.ExecuteNonQuery() > 0; }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                { throw new RegistroDuplicadoException("Ya existe otro proveedor con el CUIT ingresado."); }
            }
        }

        public bool DarDeBaja(int id) => CambiarEstado(id, false);
        public bool Reactivar(int id) => CambiarEstado(id, true);
        private bool CambiarEstado(int id, bool activo)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("UPDATE dbo.Proveedores SET Activo=@activo WHERE Id=@id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        private static void AgregarParametros(SqlCommand command, Proveedor proveedor)
        {
            command.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = proveedor.Nombre;
            command.Parameters.Add("@cuit", SqlDbType.NVarChar, 20).Value = proveedor.Cuit;
            command.Parameters.Add("@razonSocial", SqlDbType.NVarChar, 200).Value = (object?)proveedor.RazonSocial ?? DBNull.Value;
            command.Parameters.Add("@telefono", SqlDbType.NVarChar, 50).Value = (object?)proveedor.TelefonoDeContacto ?? DBNull.Value;
            command.Parameters.Add("@correo", SqlDbType.NVarChar, 254).Value = (object?)proveedor.CorreoDeContacto ?? DBNull.Value;
            command.Parameters.Add("@condicionIVA", SqlDbType.NVarChar, 50).Value = (object?)proveedor.CondicionIVA ?? DBNull.Value;
            command.Parameters.Add("@cbu", SqlDbType.VarChar, 22).Value = (object?)proveedor.CbuAsociado ?? DBNull.Value;
        }

        private static Proveedor LeerProveedor(IDataRecord reader)
        {
            return new Proveedor(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                LeerOpcional(reader, 3), LeerOpcional(reader, 4), LeerOpcional(reader, 5),
                LeerOpcional(reader, 6), LeerOpcional(reader, 7), reader.GetBoolean(8));
        }

        private static string? LeerOpcional(IDataRecord reader, int columna)
        {
            return reader.IsDBNull(columna) ? null : reader.GetString(columna);
        }
    }
}
