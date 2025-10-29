using System;
using System.IO;
using System.Threading;

namespace ProjetParc.Data
{
    /// <summary>
    /// Gestionnaire de synchronisation pour bases de données SQLite stockées sur SharePoint/OneDrive
    /// Gère les copies locales, les verrous et la synchronisation bidirectionnelle
    /// </summary>
    public class SharePointSyncManager
    {
        private string _sharePointPath;
        private string _localWorkingPath;
        private string _sharePointLockPath;
        private bool _isActive = false;

        /// <summary>
        /// Chemin local de travail (dans %LOCALAPPDATA%\GestiParc\working\)
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
    /// Initialise le gestionnaire de synchronisation
    /// </summary>
    /// <param name="sharePointDbPath">Chemin complet vers la base de données sur SharePoint</param>
    /// <returns>True si l'initialisation a réussi, False si la base est verrouillée</returns>
    /// <exception cref="SharePointLockException">Levée si la base est verrouillée et non expirée</exception>
    public bool Initialize(string sharePointDbPath)
    {
        if (!IsSharePointPath(sharePointDbPath))
        {
            // Chemin local normal, pas besoin de sync
            _isActive = false;
            return true;
        }

        _sharePointPath = sharePointDbPath;
        string sharePointFolder = Path.GetDirectoryName(_sharePointPath);
        string dbFilename = Path.GetFileName(_sharePointPath);
        
        _sharePointLockPath = Path.Combine(sharePointFolder, Path.GetFileNameWithoutExtension(dbFilename) + ".lock");
        _localWorkingPath = GetLocalWorkingPath(dbFilename);

        // Vérifier le verrou existant
        LockFile existingLock = CheckLock();
        if (existingLock != null)
        {
            // Base déjà verrouillée
            throw new SharePointLockException(existingLock);
        }

        // Copier la base vers le répertoire local
        CopyToLocal();

        // Créer le verrou
        CreateLock();

        _isActive = true;
        return true;
    }        /// <summary>
        /// Vérifie si un verrou existe et s'il est valide
        /// </summary>
        /// <returns>Le verrou actif si trouvé, null sinon</returns>
        private LockFile CheckLock()
        {
            if (!File.Exists(_sharePointLockPath))
                return null;

            try
            {
                LockFile lockFile = LockFile.Load(_sharePointLockPath);

                // Vérifier si le verrou est toujours valide
                if (!lockFile.IsExpired() && lockFile.IsProcessAlive())
                {
                    return lockFile; // Verrou actif
                }

                // Verrou expiré ou processus mort, on peut le supprimer
                File.Delete(_sharePointLockPath);
                return null;
            }
            catch
            {
                // Fichier corrompu, on le supprime
                try { File.Delete(_sharePointLockPath); } catch { }
                return null;
            }
        }

        /// <summary>
        /// Copie la base de données de SharePoint vers le répertoire local
        /// </summary>
        private void CopyToLocal()
        {
            if (!File.Exists(_sharePointPath))
            {
                // Base n'existe pas encore sur SharePoint, on va la créer localement
                return;
            }

            try
            {
                // Copie avec retry (SharePoint peut être lent)
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
                        Thread.Sleep(500); // Attendre 500ms avant de réessayer
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SharePointSyncException("Impossible de copier la base depuis SharePoint vers le répertoire local", ex);
            }
        }

    /// <summary>
    /// Copie la base de données locale vers SharePoint
    /// </summary>
    public void CopyToSharePoint()
    {
        if (!_isActive)
            return;

        if (!File.Exists(_localWorkingPath))
        {
            throw new SharePointSyncException("La base locale n'existe pas, impossible de synchroniser");
        }

        try
        {
            // IMPORTANT : Forcer SQLite à écrire toutes les modifications dans le fichier principal
            // avant la copie (flush du WAL)
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_localWorkingPath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                command.ExecuteNonQuery();
            }

            // Créer un backup temporaire sur SharePoint
            string backupPath = _sharePointPath + ".backup";
            if (File.Exists(_sharePointPath))
            {
                File.Copy(_sharePointPath, backupPath, overwrite: true);
            }

            // Copie atomique avec retry
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Copy(_localWorkingPath, _sharePointPath, overwrite: true);
                    
                    // Succès, supprimer le backup
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    
                    return;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    Thread.Sleep(500);
                }
            }
        }
        catch (Exception ex)
        {
            throw new SharePointSyncException("Impossible de synchroniser la base vers SharePoint", ex);
        }
    }        /// <summary>
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
        /// Supprime le fichier verrou sur SharePoint
        /// </summary>
        private void RemoveLock()
        {
            if (File.Exists(_sharePointLockPath))
            {
                try
                {
                    File.Delete(_sharePointLockPath);
                }
                catch
                {
                    // Si on ne peut pas supprimer, ce n'est pas grave
                    // Le verrou expirera automatiquement après 4h
                }
            }
        }

        /// <summary>
        /// Nettoie et synchronise avant fermeture de l'application
        /// </summary>
        public void Cleanup()
        {
            if (!_isActive)
                return;

            try
            {
                // Synchroniser vers SharePoint
                CopyToSharePoint();

                // Supprimer le verrou
                RemoveLock();

                // Optionnel : supprimer la copie locale
                // if (File.Exists(_localWorkingPath))
                //     File.Delete(_localWorkingPath);
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
        /// Force la suppression d'un verrou expiré (à utiliser avec précaution)
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
                
                // Vérifier que le verrou est bien expiré ou que le processus est mort
                if (lockFile.IsExpired() || !lockFile.IsProcessAlive())
                {
                    File.Delete(lockPath);
                    return true;
                }

                return false; // Verrou toujours actif, on ne peut pas forcer
            }
            catch
            {
                // Fichier corrompu, on le supprime
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
