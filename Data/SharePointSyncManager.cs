using System;
using System.IO;
using System.Threading;

namespace ProjetParc.Data
{
    /// <summary>
    /// Gère la synchronisation de bases SQLite sur SharePoint/OneDrive
    /// </summary>
    public class SharePointSyncManager
    {
        private string _sharePointPath;
        private string _localWorkingPath;
        private string _sharePointLockPath;
        private bool _isActive = false;

        /// <summary>
        /// Chemin local de travail dans %LOCALAPPDATA%
        /// </summary>
        public string LocalWorkingPath => _localWorkingPath;

        /// <summary>
        /// Indique si le mode SharePoint est actif
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Détecte si un chemin pointe vers SharePoint/OneDrive
        /// </summary>
        public static bool IsSharePointPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string normalizedPath = path.Replace("\\", "/").ToLowerInvariant();
            return normalizedPath.Contains("/onedrive") || 
                   normalizedPath.Contains("/sharepoint") ||
                   normalizedPath.Contains("onedrive.live.com");
        }

        /// <summary>
        /// Génère le chemin local de travail dans %LOCALAPPDATA%
        /// </summary>
        public static string GetLocalWorkingPath(string originalFilename = "bddGestiParc.db")
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string workingDir = Path.Combine(localAppData, "GestiParc", "working");
            
            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);

            return Path.Combine(workingDir, originalFilename);
        }

    /// <summary>
    /// Initialise le gestionnaire et crée le verrou si nécessaire
    /// </summary>
    public bool Initialize(string sharePointDbPath)
    {
        if (!IsSharePointPath(sharePointDbPath))
        {
            _isActive = false;
            return true;
        }

        _sharePointPath = sharePointDbPath;
        string sharePointFolder = Path.GetDirectoryName(_sharePointPath);
        string dbFilename = Path.GetFileName(_sharePointPath);
        
        _sharePointLockPath = Path.Combine(sharePointFolder, Path.GetFileNameWithoutExtension(dbFilename) + ".lock");
        _localWorkingPath = GetLocalWorkingPath(dbFilename);

        LockFile existingLock = CheckLock();
        if (existingLock != null)
            throw new SharePointLockException(existingLock);

        CopyToLocal();
        CreateLock();

        _isActive = true;
        return true;
    }        
    
    /// <summary>
    /// Vérifie si un verrou existe et s'il est encore valide
    /// </summary>
    private LockFile CheckLock()
        {
            if (!File.Exists(_sharePointLockPath))
                return null;

            try
            {
                LockFile lockFile = LockFile.Load(_sharePointLockPath);

                if (!lockFile.IsExpired() && lockFile.IsProcessAlive())
                    return lockFile;

                File.Delete(_sharePointLockPath);
                return null;
            }
            catch
            {
                try { File.Delete(_sharePointLockPath); } catch { }
                return null;
            }
        }

        /// <summary>
        /// Copie la base depuis SharePoint vers le répertoire local
        /// </summary>
        private void CopyToLocal()
        {
            if (!File.Exists(_sharePointPath))
                return;

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Copy(_sharePointPath, _localWorkingPath, overwrite: true);
                    return;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    Thread.Sleep(500);
                }
            }
            
            throw new SharePointSyncException("Impossible de copier la base depuis SharePoint vers le répertoire local");
        }

    /// <summary>
    /// Synchronise la base locale vers SharePoint avec checkpoint WAL
    /// </summary>
    public void CopyToSharePoint()
    {
        if (!_isActive)
            return;

        if (!File.Exists(_localWorkingPath))
            throw new SharePointSyncException("La base locale n'existe pas, impossible de synchroniser");

        // Forcer l'écriture du WAL dans le fichier principal
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_localWorkingPath}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            command.ExecuteNonQuery();
        }

        int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                File.Copy(_localWorkingPath, _sharePointPath, overwrite: true);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                Thread.Sleep(500);
            }
        }
        
        throw new SharePointSyncException("Impossible de synchroniser la base vers SharePoint après 3 tentatives");
    }

    /// <summary>
    /// Crée le fichier verrou sur SharePoint
    /// </summary>
    private void CreateLock()
    {
        try
        {
            LockFile lockFile = LockFile.Create();
            lockFile.Save(_sharePointLockPath);
        }
        catch (Exception ex)
        {
            throw new SharePointSyncException("Impossible de créer le fichier verrou", ex);
        }
    }

    /// <summary>
    /// Supprime le fichier verrou
    /// </summary>
    private void RemoveLock()
    {
        if (!File.Exists(_sharePointLockPath))
            return;

        try
        {
            File.Delete(_sharePointLockPath);
        }
        catch
        {
            // Le verrou expirera automatiquement après 4h
        }
    }

    /// <summary>
    /// Synchronise et nettoie avant la fermeture
    /// </summary>
    public void Cleanup()
    {
        if (!_isActive)
            return;

        try
        {
            CopyToSharePoint();
            RemoveLock();
        }
        catch (Exception ex)
        {
            throw new SharePointSyncException("Erreur lors de la synchronisation finale", ex);
        }
        finally
        {
            _isActive = false;
        }
    }

    /// <summary>
    /// Force la suppression d'un verrou expiré
    /// </summary>
    public static bool ForceRemoveLock(string sharePointDbPath)
        {
            if (!IsSharePointPath(sharePointDbPath))
                return false;

            string sharePointFolder = Path.GetDirectoryName(sharePointDbPath);
            string dbFilename = Path.GetFileName(sharePointDbPath);
            string lockPath = Path.Combine(sharePointFolder, Path.GetFileNameWithoutExtension(dbFilename) + ".lock");

            if (!File.Exists(lockPath))
                return false;

            try
            {
                LockFile lockFile = LockFile.Load(lockPath);
                
                if (lockFile.IsExpired() || !lockFile.IsProcessAlive())
                {
                    File.Delete(lockPath);
                    return true;
                }

                return false;
            }
            catch
            {
                try
                {
                    File.Delete(lockPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Exception levée quand la base est verrouillée par un autre utilisateur
    /// </summary>
    public class SharePointLockException : Exception
    {
        public LockFile Lock { get; }

        public SharePointLockException(LockFile lockFile) 
            : base($"La base de données est verrouillée : {lockFile.GetDescription()}")
        {
            Lock = lockFile;
        }
    }

    /// <summary>
    /// Exception levée lors d'erreurs de synchronisation SharePoint
    /// </summary>
    public class SharePointSyncException : Exception
    {
        public SharePointSyncException(string message) : base(message) { }
        public SharePointSyncException(string message, Exception innerException) : base(message, innerException) { }
    }
}
