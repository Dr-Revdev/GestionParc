using System.Configuration;
using GestiParc.Ui.Views.Auth;
using System.Net.Http;

namespace GestiParc.Ui;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();

            // Configurer la chaîne de connexion API
            var apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            if (string.IsNullOrEmpty(apiBaseUrl) || !Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
            {
                MessageBox.Show(
                    "L'URL de l'API n'est pas configurée\n\n" +
                    "Veuillez configurer la clé 'ApiBaseUrl' dans la App.config.\n",
                    "Configuration manquante",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Tester la connexion Ping API
            try
            {
                using var http = new HttpClient
                {
                    BaseAddress = baseUri,
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var response = http.GetAsync("api/ping").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        $"L'API est inaccessible (HTTP {(int)response.StatusCode}). \n\n" +
                        $"Vérifiez que l'API est démarrée et que l'URL est correcte :\n{baseUri}",
                        "Erreur de connexion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Impossible de contacter l'API.\n\n" +
                    $"URL: {baseUri}\n" +
                    $"Erreur : {ex.Message}\n\n" +
                    $"Veuillez vérifier que l'API est démarrée et accessible depuis ce PC.",
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
