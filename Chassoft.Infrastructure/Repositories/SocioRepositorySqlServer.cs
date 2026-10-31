using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositorios;
using Chassoft.Dominio.Excepciones;
namespace Chassoft.Infrastructure.Repositories
{
    public class SocioRepositorySqlServer : ISocioRepository
    {
        private const string Columnas = "Id, NombreCompleto, Dni, FechaAlta, Activo, Email";
        private readonly string _connectionString;
        public SocioRepositorySqlServer(string connectionString) => _connectionString = connectionString;

        public Socio Agregar(Socio socio)
        {
            if (socio == null) throw new ArgumentNullException(nameof(socio));
            if (socio.Id != 0) throw new ArgumentException("El socio ya tiene identidad.");
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"INSERT INTO dbo.Socios (NombreCompleto, Dni, FechaAlta, Activo, Email)
                OUTPUT INSERTED.Id, INSERTED.NombreCompleto, INSERTED.Dni, INSERTED.FechaAlta, INSERTED.Activo, INSERTED.Email
                VALUES (@nombre, @dni, @fechaAlta, 1, @email)", conn);
            Datos(cmd, socio);
            cmd.Parameters.Add("@fechaAlta", SqlDbType.Date).Value = socio.FechaAlta;
            conn.Open();
            try { using var lector = cmd.ExecuteReader(); lector.Read(); return LeerSocio(lector); }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            { throw new RegistroDuplicadoException("Ya existe un socio registrado con el DNI ingresado. Si está inactivo, reactivalo."); }
        }
        public bool Actualizar(Socio socio)
        {
            if (socio == null) throw new ArgumentNullException(nameof(socio));
            if (socio.Id <= 0) throw new ArgumentException("El socio debe tener identidad.");
            using var conn = new SqlConnection(_connectionString);
            // FechaAlta y Activo no se cambian al editar datos personales.
            using var cmd = new SqlCommand("UPDATE dbo.Socios SET NombreCompleto=@nombre, Dni=@dni, Email=@email WHERE Id=@id", conn);
            Datos(cmd, socio); cmd.Parameters.Add("@id", SqlDbType.Int).Value = socio.Id;
            conn.Open();
            try { return cmd.ExecuteNonQuery() > 0; }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            { throw new RegistroDuplicadoException("Ya existe otro socio con el DNI ingresado."); }
        }
        public bool DarDeBaja(int id) => CambiarEstado(id, false);
        public bool Reactivar(int id) => CambiarEstado(id, true);
        private bool CambiarEstado(int id, bool activo)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UPDATE dbo.Socios SET Activo=@activo WHERE Id=@id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
            conn.Open(); return cmd.ExecuteNonQuery() > 0;
        }
        public Socio? ObtenerPorId(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT " + Columnas + " FROM dbo.Socios WHERE Id=@id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            conn.Open(); using var lector = cmd.ExecuteReader();
            return lector.Read() ? LeerSocio(lector) : null;
        }
        public Socio? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || dni.Trim().Length > 20) throw new ArgumentException("DNI obligatorio, hasta 20 caracteres.");
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT " + Columnas + " FROM dbo.Socios WHERE Dni=@dni", conn);
            cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dni.Trim();
            conn.Open(); using var lector = cmd.ExecuteReader();
            return lector.Read() ? LeerSocio(lector) : null;
        }
        public IEnumerable<Socio> ObtenerTodos(bool? activo = null)
        {
            var lista = new List<Socio>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT " + Columnas + " FROM dbo.Socios WHERE (@activo IS NULL OR Activo=@activo) ORDER BY NombreCompleto, Id", conn);
            cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = (object?)activo ?? DBNull.Value;
            conn.Open(); using var lector = cmd.ExecuteReader();
            while (lector.Read()) lista.Add(LeerSocio(lector));
            return lista;
        }
        public IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes)
        {
            var lista = new List<Socio>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"SELECT s.Id, s.NombreCompleto, s.Dni, s.FechaAlta, s.Activo, s.Email
                FROM dbo.Socios s WHERE s.Activo=1 AND NOT EXISTS (
                    SELECT 1 FROM dbo.PagosCuota p WHERE p.IdSocio=s.Id AND p.Anio=@anio AND p.Mes=@mes)
                ORDER BY s.NombreCompleto, s.Id", conn);
            cmd.Parameters.Add("@anio", SqlDbType.SmallInt).Value = anio;
            cmd.Parameters.Add("@mes", SqlDbType.TinyInt).Value = mes;
            conn.Open(); using var lector = cmd.ExecuteReader();
            while (lector.Read()) lista.Add(LeerSocio(lector));
            return lista;
        }
        private static void Datos(SqlCommand cmd, Socio socio)
        {
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 200).Value = socio.NombreCompleto;
            cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = socio.Dni;
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = (object?)socio.Email ?? DBNull.Value;
        }
        private static Socio LeerSocio(IDataRecord r) => new Socio(r.GetInt32(0), r.GetString(1), r.GetString(2),
            r.GetDateTime(3), r.GetBoolean(4), r.IsDBNull(5) ? null : r.GetString(5));
    }
}
