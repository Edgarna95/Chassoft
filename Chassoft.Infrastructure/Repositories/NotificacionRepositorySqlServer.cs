using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Repositories;
namespace Chassoft.Infrastructure.Repositories
{
    public class NotificacionRepositorySqlServer : INotificacionRepository
    {
        private readonly string _connectionString;
        public NotificacionRepositorySqlServer(string connectionString) => _connectionString = connectionString;
        public bool IntentarReservar(int idSocio, string tipo, int anio, int mes, string destinatario)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(
                @"INSERT INTO Notificaciones (IdSocio, Tipo, Anio, Mes, Destinatario, Estado)
                  SELECT @s, @t, @a, @m, @dest, 'Pendiente'
                  WHERE NOT EXISTS (SELECT 1 FROM Notificaciones WITH (UPDLOCK, HOLDLOCK)
                      WHERE IdSocio=@s AND Tipo=@t AND Anio=@a AND Mes=@m)", conn);
            Parametros(cmd, idSocio, tipo, anio, mes);
            cmd.Parameters.Add("@dest", SqlDbType.NVarChar, 254).Value = destinatario;
            try { return cmd.ExecuteNonQuery() == 1; }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { return false; }
        }
        public void MarcarResultado(int idSocio, string tipo, int anio, int mes, bool enviado)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(@"UPDATE Notificaciones SET Estado=@estado, FechaEnvio=SYSUTCDATETIME()
                WHERE IdSocio=@s AND Tipo=@t AND Anio=@a AND Mes=@m", conn);
            Parametros(cmd, idSocio, tipo, anio, mes);
            cmd.Parameters.Add("@estado", SqlDbType.NVarChar, 20).Value = enviado ? "Enviado" : "Revisar";
            cmd.ExecuteNonQuery();
        }
        private static void Parametros(SqlCommand cmd, int socio, string tipo, int anio, int mes)
        {
            cmd.Parameters.Add("@s", SqlDbType.Int).Value = socio;
            cmd.Parameters.Add("@t", SqlDbType.NVarChar, 30).Value = tipo;
            cmd.Parameters.Add("@a", SqlDbType.SmallInt).Value = anio;
            cmd.Parameters.Add("@m", SqlDbType.TinyInt).Value = mes;
        }
    }
}
