using System.Data;
using MySqlConnector;

namespace GestiParc.Infrastructure
{
    public static class DbFactory
    {
        // Remplie au démarrage (Program.cs)
        public static string ConnectionString { get; set; } = "";

        public static IDbConnection Create() => new MySqlConnection(ConnectionString);
    }
}
