using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;

namespace Chassoft.Infrastructure.Repositories
{
    public class ProveedorRepositorySqlServer : IProveedorRepository
    {
        private const string Columnas = "Id, Nombre, Cuit, RazonSocial, TelefonoDeContacto, CorreoDeContacto, CondicionIVA, CbuAsociado";
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
                var id = Convert.ToInt32(command.ExecuteScalar());
                return new Proveedor(id, proveedor.Nombre, proveedor.Cuit, proveedor.RazonSocial,
                    proveedor.TelefonoDeContacto, proveedor.CorreoDeContacto,
                    proveedor.CondicionIVA, proveedor.CbuAsociado);
            }
        }

        public IEnumerable<Proveedor> ObtenerTodos()
        {
            var lista = new List<Proveedor>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT " + Columnas + " FROM dbo.Proveedores ORDER BY Id", connection))
            {
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

        public void Actualizar(Proveedor proveedor)
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
                command.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.Proveedores WHERE Id = @id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                connection.Open();
                command.ExecuteNonQuery();
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
                LeerOpcional(reader, 6), LeerOpcional(reader, 7));
        }

        private static string? LeerOpcional(IDataRecord reader, int columna)
        {
            return reader.IsDBNull(columna) ? null : reader.GetString(columna);
        }
    }
}
