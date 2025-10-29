using System.IO;
using Microsoft.Data.Sqlite;

namespace ProjetParc.Data;

/// <summary>
/// Classe statique gérant les connexions à la base de données SQLite
/// </summary>
public static class Database
{
    private static string _dbPath = string.Empty;
    private static SharePointSyncManager _syncManager = new SharePointSyncManager();

    /// <summary>
    /// Obtient le gestionnaire de synchronisation SharePoint
    /// </summary>
    public static SharePointSyncManager SyncManager => _syncManager;

    /// <summary>
    /// Initialise le chemin de la base de données depuis la configuration
    /// </summary>
    public static void Initialize(string databasePath)
    {
        _dbPath = databasePath;
    }

    /// <summary>
    /// Obtient le chemin actuel de la base de données
    /// </summary>
    public static string GetDatabasePath()
    {
        if (string.IsNullOrEmpty(_dbPath))
        {
            throw new InvalidOperationException("La base de données n'a pas été initialisée. Appelez Database.Initialize() d'abord.");
        }
        return _dbPath;
    }

    /// <summary>
    /// Ouvre une nouvelle connexion à la base de données SQLite
    /// </summary>
    /// <returns>Une connexion SQLite ouverte et configurée</returns>
    public static SqliteConnection Open()
    {
        if (string.IsNullOrEmpty(_dbPath))
        {
            throw new InvalidOperationException("La base de données n'a pas été initialisée. Appelez Database.Initialize() d'abord.");
        }

        // Si le mode SharePoint est actif, utiliser le chemin local
        string actualDbPath = _syncManager.IsActive ? _syncManager.LocalWorkingPath : _dbPath;

        // Créer le répertoire si nécessaire
        var directory = Path.GetDirectoryName(actualDbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connexion = new SqliteConnection($"Data Source={actualDbPath};Cache=Shared;Foreign Keys=True;");
        connexion.Open();
        using var pragma = connexion.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=3000;";
        pragma.ExecuteNonQuery();
        return connexion;
    }

    /// <summary>
    /// S'assure que la base de données est initialisée et que le répertoire existe
    /// </summary>
    public static void EnsureInitialized()
    {
        if (string.IsNullOrEmpty(_dbPath))
        {
            throw new InvalidOperationException("La base de données n'a pas été initialisée. Appelez Database.Initialize() d'abord.");
        }

        // Si c'est un chemin SharePoint, initialiser la synchronisation
        if (SharePointSyncManager.IsSharePointPath(_dbPath))
        {
            _syncManager.Initialize(_dbPath);
        }

        // Utiliser le chemin approprié (local si SharePoint, sinon original)
        string actualDbPath = _syncManager.IsActive ? _syncManager.LocalWorkingPath : _dbPath;

        var directory = Path.GetDirectoryName(actualDbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Vérifier si les tables existent déjà
        bool needsInitialization = false;
        
        try
        {
            using var testConnection = Open();
            using var command = testConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Agents'";
            var result = command.ExecuteScalar();
            needsInitialization = result == null || (long)result == 0;
        }
        catch
        {
            needsInitialization = true;
        }

        // Créer le schéma si nécessaire
        if (needsInitialization)
        {
            using var connection = Open();
            CreateSchema(connection);
        }
    }

    /// <summary>
    /// Crée le schéma complet de la base de données
    /// </summary>
    private static void CreateSchema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        
        // Créer toutes les tables
        command.CommandText = @"
            BEGIN TRANSACTION;

            -- Table Equipes
            CREATE TABLE IF NOT EXISTS Equipes (
                id   INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE
            );

            -- Table Sites
            CREATE TABLE IF NOT EXISTS Sites (
                id   INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE
            );

            -- Table equipment_type
            CREATE TABLE IF NOT EXISTS equipment_type (
                id   INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE
            );

            -- Table Agents
            CREATE TABLE IF NOT EXISTS Agents (
                idrh         TEXT PRIMARY KEY,
                nom          TEXT,
                prenom       TEXT,
                email        TEXT,
                equipe_id    INTEGER NULL,
                site_id      INTEGER NULL,
                heberge      INTEGER NOT NULL DEFAULT 0,
                commentaire  TEXT,
                FOREIGN KEY (equipe_id) REFERENCES Equipes(id)
                    ON DELETE SET NULL
                    ON UPDATE CASCADE,
                FOREIGN KEY (site_id)   REFERENCES Sites(id)
                    ON DELETE SET NULL
                    ON UPDATE CASCADE
            );

            -- Table Equipements
            CREATE TABLE IF NOT EXISTS Equipements (
                id_equipement TEXT PRIMARY KEY,
                type_id       INTEGER NOT NULL,
                nom           TEXT,
                code_parc     TEXT,
                numero_serie  TEXT,
                marque        TEXT,
                commentaire   TEXT,
                etat_pret     INTEGER NOT NULL DEFAULT 0,
                idrh          TEXT NULL,
                date_rendu_dsem TEXT NULL,
                FOREIGN KEY (type_id) REFERENCES equipment_type(id)
                    ON DELETE RESTRICT
                    ON UPDATE CASCADE,
                FOREIGN KEY (idrh)     REFERENCES Agents(idrh)
                    ON DELETE SET NULL
                    ON UPDATE CASCADE
            );

            -- Table Travail
            CREATE TABLE IF NOT EXISTS Travail (
                idrh    TEXT    NOT NULL,
                site_id INTEGER NOT NULL,
                PRIMARY KEY (idrh, site_id),
                FOREIGN KEY (idrh)   REFERENCES Agents(idrh)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE,
                FOREIGN KEY (site_id) REFERENCES Sites(id)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE
            );

            -- Index pour optimiser les performances
            CREATE INDEX IF NOT EXISTS idx_agents_equipe     ON Agents(equipe_id);
            CREATE INDEX IF NOT EXISTS idx_agents_site       ON Agents(site_id);
            CREATE INDEX IF NOT EXISTS idx_equipements_idrh  ON Equipements(idrh);
            CREATE INDEX IF NOT EXISTS idx_equipements_type  ON Equipements(type_id);
            CREATE INDEX IF NOT EXISTS idx_travail_site      ON Travail(site_id);

            -- Données initiales pour equipment_type
            INSERT INTO equipment_type (id, name) VALUES 
                (1, 'PC'),
                (2, 'Ecran'),
                (3, 'Imprimante'),
                (4, 'Routeur'),
                (5, 'Switch'),
                (6, 'Inconnu');

            COMMIT;
        ";
        
        command.ExecuteNonQuery();
    }
}