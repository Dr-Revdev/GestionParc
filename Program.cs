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
            Database.EnsureInitialized();

            // Lancer l'application principale
            Application.Run(new WelcomePage());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
        }
    }
}