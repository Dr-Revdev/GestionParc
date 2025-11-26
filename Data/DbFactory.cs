using System.Data;
using MySqlConnector;

namespace ProjetParc.Data
{
    public static class DbFactory
    {
        // Remplie au démarrage (Program.cs)
        public static string ConnectionString { get; set; } = "";

        public static IDbConnection Create() => new MySqlConnection(ConnectionString);
    }
}
