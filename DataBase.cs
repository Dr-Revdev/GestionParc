using System.IO;
using Microsoft.Data.Sqlite;

namespace ProjetParc;

public static class Database
{
    private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");
    private static readonly string DbPath = Path.Combine(DataDir, "parcdatabase.db");

    public static SqliteConnection Open()
    {
        Directory.CreateDirectory(DataDir);
        var connection = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");
        connection.Open();
        return connection;
    }

    public static void EnsureInitialized()
    {
        Directory.CreateDirectory(DataDir);
        if (!File.Exists(DbPath))
        {
            using var _ = Open();
        }
    
    }

}