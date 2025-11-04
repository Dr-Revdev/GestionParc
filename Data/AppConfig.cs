using System;
using System.IO;
using System.Text.Json;

namespace ProjetParc.Data;

/// <summary>
/// Gère la configuration de l'application (préférences utilisateur, paramètres d'affichage, etc.)
/// La connexion MySQL est configurée dans App.config
/// </summary>
public class AppConfig
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GestiParc"
    );
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    // Réservé pour de futures options utilisateur
    // Exemples : thème, langue, colonnes affichées, etc.

    /// <summary>
    /// Charge la configuration depuis le fichier JSON
    /// </summary>
    public static AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            return new AppConfig();
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    /// <summary>
    /// Sauvegarde la configuration dans le fichier JSON
    /// </summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Impossible de sauvegarder la configuration: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Supprime le fichier de configuration (utile pour les tests ou réinitialisation)
    /// </summary>
    public static void Reset()
    {
        if (File.Exists(ConfigPath))
        {
            File.Delete(ConfigPath);
        }
    }
}
