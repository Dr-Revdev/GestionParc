using System;
using System.Windows.Forms;
using ProjetParc.Data;
using ProjetParc.Views;

namespace ProjetParc;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Database.EnsureInitialized(); // :contentReference[oaicite:0]{index=0}
            ApplicationConfiguration.Initialize();
            Application.Run(new WelcomePage());     // :contentReference[oaicite:1]{index=1}
        }
        catch (Exception ex)
        {
            MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
        }
        // Database.EnsureInitialized();
        // ApplicationConfiguration.Initialize();
        // Application.Run(new WelcomePage());
    }
}