using System;
using System.Configuration;
using System.Windows.Forms;
using ProjetParc.Data;
using ProjetParc.Views;
using ProjetParc.Views.Auth;

namespace ProjetParc;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();

            // Configurer la chaîne de connexion MySQL
            var connectionString = ConfigurationManager.AppSettings["MySqlConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show(
                    "La chaîne de connexion MySQL n'est pas configurée dans App.config.\n\n" +
                    "Veuillez configurer la clé 'MySqlConnection' dans la section <appSettings>.",
                    "Configuration manquante",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            DbFactory.ConnectionString = connectionString;

            // Tester la connexion
            try
            {
                using var connection = DbFactory.Create();
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Impossible de se connecter à la base de données MySQL.\n\n" +
                    $"Erreur : {ex.Message}\n\n" +
                    $"Veuillez vérifier que :\n" +
                    $"- Le serveur MySQL est démarré\n" +
                    $"- La chaîne de connexion est correcte\n" +
                    $"- La base de données 'gestiparc' existe",
                    "Erreur de connexion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Lancer l'application avec la page de connexion
            Application.Run(new LoginView());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur au démarrage de l'application :\n\n{ex.Message}",
                "Erreur fatale",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}