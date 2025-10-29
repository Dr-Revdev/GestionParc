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
    /// Nettoie les ressources à la fermeture de l'application
    /// </summary>
    private static void OnApplicationExit(object sender, EventArgs e)
    {
        try
        {
            Database.SyncManager.Cleanup();
        }
        catch (SharePointSyncException syncEx)
        {
            MessageBox.Show(
                $"ATTENTION : Vos modifications n'ont PAS été sauvegardées sur SharePoint.\n\n" +
                $"Erreur : {syncEx.Message}\n\n" +
                $"Vos données sont conservées localement dans :\n{Database.SyncManager.LocalWorkingPath}\n\n" +
                $"Reconnectez-vous au réseau et relancez l'application pour synchroniser.",
                "Sauvegarde SharePoint échouée",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur inattendue lors de la fermeture :\n\n{ex.Message}\n\n" +
                $"L'application va se fermer. Vos données locales sont conservées.",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}