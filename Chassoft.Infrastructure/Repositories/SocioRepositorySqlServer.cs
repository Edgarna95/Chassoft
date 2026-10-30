using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositorios;

namespace Chassoft.Infrastructure.Repositories
{
    public class SocioRepositorySqlServer : ISocioRepository
    {
        private readonly string _connectionString;

        public SocioRepositorySqlServer(string connectionString)
            => _connectionString = connectionString;

        public void Agregar(Socio socio)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                @"INSERT INTO Socios (NombreCompleto, Dni, FechaAlta, Activo, Email)
                  VALUES (@nombre, @dni, @fechaAlta, @activo, @email)", conn);
            cmd.Parameters.Add("@nombre",   SqlDbType.NVarChar, 200).Value = socio.NombreCompleto;
            cmd.Parameters.Add("@dni",      SqlDbType.NVarChar, 20).Value  = socio.Dni;
            cmd.Parameters.Add("@fechaAlta",SqlDbType.Date).Value           = socio.FechaAlta;
            cmd.Parameters.Add("@activo",   SqlDbType.Bit).Value            = socio.Activo;
            cmd.Parameters.Add("@email",    SqlDbType.NVarChar, 254).Value  = (object?)socio.Email ?? DBNull.Value;
            try { cmd.ExecuteNonQuery(); }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { throw new ArgumentException("Ya existe un socio registrado con el DNI ingresado"); }
        }

        public void DarDeBaja(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("UPDATE Socios SET Activo = 0 WHERE Id = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            var filas = cmd.ExecuteNonQuery();
            if (filas == 0) throw new ArgumentException($"No existe un socio con Id {id}");
        }

        public Socio? ObtenerPorId(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "SELECT Id, NombreCompleto, Dni, FechaAlta, Activo, Email FROM Socios WHERE Id = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            using var r = cmd.ExecuteReader();
            return r.Read() ? LeerSocio(r) : null;
        }

        public Socio? ObtenerPorDni(string dni)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "SELECT Id, NombreCompleto, Dni, FechaAlta, Activo, Email FROM Socios WHERE Dni = @dni", conn);
            cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dni;
            using var r = cmd.ExecuteReader();
            return r.Read() ? LeerSocio(r) : null;
        }

        public IEnumerable<Socio> ObtenerTodos()
        {
            var lista = new List<Socio>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "SELECT Id, NombreCompleto, Dni, FechaAlta, Activo, Email FROM Socios ORDER BY NombreCompleto", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerSocio(r));
            return lista;
        }

        public IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes)
        {
            var lista = new List<Socio>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                @"SELECT s.Id, s.NombreCompleto, s.Dni, s.FechaAlta, s.Activo, s.Email
                  FROM Socios s
                  WHERE s.Activo = 1
                    AND NOT EXISTS (
                        SELECT 1 FROM PagosCuota p
                        WHERE p.IdSocio = s.Id AND p.Anio = @anio AND p.Mes = @mes
                    )
                  ORDER BY s.NombreCompleto", conn);
            cmd.Parameters.Add("@anio", SqlDbType.SmallInt).Value = anio;
            cmd.Parameters.Add("@mes",  SqlDbType.TinyInt).Value  = mes;
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerSocio(r));
            return lista;
        }

        private static Socio LeerSocio(IDataRecord r) =>
            new Socio(r.GetInt32(0), r.GetString(1), r.GetString(2),
                      r.GetDateTime(3), r.GetBoolean(4),
                      r.IsDBNull(5) ? null : r.GetString(5));
    }
}
