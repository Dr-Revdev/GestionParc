using System;
using System.Windows.Forms;
using ProjetParc.Data;
using ProjetParc.Views;
using ProjetParc.Views.FirstRun;

namespace ProjetParc;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();

            // Gestionnaire de fermeture pour synchroniser avec SharePoint
            Application.ApplicationExit += OnApplicationExit;

            // Vérifier si c'est le premier lancement
            if (AppConfig.IsFirstRun())
            {
                // Afficher la fenêtre de configuration du premier lancement
                using var firstRunView = new FirstRunView();
                var result = firstRunView.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrEmpty(firstRunView.SelectedDatabasePath))
                {
                    MessageBox.Show(
                        "Configuration annulée. L'application va se fermer.",
                        "Configuration requise",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                // Sauvegarder la configuration
                var config = new AppConfig
                {
                    DatabasePath = firstRunView.SelectedDatabasePath
                };
                config.Save();

                // Initialiser la base de données
                Database.Initialize(config.DatabasePath);
            }
            else
            {
                // Charger la configuration existante
                var config = AppConfig.Load();

                if (string.IsNullOrEmpty(config.DatabasePath))
                {
                    MessageBox.Show(
                        "Configuration invalide. Veuillez reconfigurer l'application.",
                        "Erreur de configuration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    AppConfig.Reset();
                    return;
                }

                // Initialiser la base de données
                Database.Initialize(config.DatabasePath);
            }

            // S'assurer que la base de données est initialisée
            try
            {
                Database.EnsureInitialized();
            }
            catch (SharePointLockException lockEx)
            {
                // Base verrouillée par un autre utilisateur
                var lockInfo = lockEx.Lock;
                string message = $"La base de données est actuellement utilisée par :\n\n{lockInfo.GetDescription()}\n\n";
                
                if (lockInfo.IsExpired() || !lockInfo.IsProcessAlive())
                {
                    message += "Ce verrou semble obsolète (processus terminé ou expiré).\n\nVoulez-vous forcer l'ouverture ?";
                    
                    var result = MessageBox.Show(
                        message,
                        "Base de données verrouillée",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        // Forcer la suppression du verrou
                        var config = AppConfig.Load();
                        if (SharePointSyncManager.ForceRemoveLock(config.DatabasePath))
                        {
                            // Réessayer l'initialisation
                            Database.EnsureInitialized();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Impossible de forcer l'ouverture. Veuillez réessayer plus tard.",
                                "Erreur",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }
                    }
                    else
                    {
                        return; // Quitter l'application
                    }
                }
                else
                {
                    message += "Veuillez attendre que l'autre utilisateur termine avant de relancer l'application.";
                    MessageBox.Show(
                        message,
                        "Base de données verrouillée",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }
            }

            // Lancer l'application principale
            Application.Run(new WelcomePage());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
        }
    }

    /// <summary>
    /// Gestionnaire d'événement à la fermeture de l'application
    /// Nettoie le fichier de verrouillage SharePoint si nécessaire
    /// Note: La confirmation de sauvegarde est gérée par WelcomePage.OnFormClosing()
    /// </summary>
    private static void OnApplicationExit(object sender, EventArgs e)
    {
        try
        {
            // Nettoyage du fichier de verrouillage SharePoint
            // La sauvegarde a déjà été gérée par FormClosing si l'utilisateur l'a confirmée
            Database.SyncManager.Cleanup();
        }
        catch (SharePointSyncException syncEx)
        {
            MessageBox.Show(
                $"Erreur lors de la synchronisation vers SharePoint :\n\n{syncEx.Message}\n\nVos modifications locales sont conservées dans :\n{Database.SyncManager.LocalWorkingPath}",
                "Erreur de synchronisation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur inattendue lors de la fermeture :\n\n{ex.Message}",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}