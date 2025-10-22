using System.IO;
using Microsoft.Data.Sqlite;

namespace ProjetParc.Data;

/// <summary>
/// Classe statique gérant les connexions à la base de données SQLite
/// </summary>
public static class Database
{
    private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");
    private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

    /// <summary>
    /// Ouvre une nouvelle connexion à la base de données SQLite
    /// </summary>
    /// <returns>Une connexion SQLite ouverte et configurée</returns>
    public static SqliteConnection Open()
    {
        Directory.CreateDirectory(DataDir);
        var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");
        connexion.Open();
        using var pragma = connexion.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";
        pragma.ExecuteNonQuery();
        return connexion;
    }

    /// <summary>
    /// S'assure que la base de données est initialisée et que le répertoire existe
    /// </summary>
    public static void EnsureInitialized()
    {
        Directory.CreateDirectory(DataDir);
        if (!File.Exists(DbPath))
        {
            using var _ = Open();
        }

    }

}