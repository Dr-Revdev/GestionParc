using System;
using System.IO;
using System.Text.Json;

namespace ProjetParc.Data;

/// <summary>
/// Gère la configuration de l'application (notamment le chemin de la base de données)
/// </summary>
public class AppConfig
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GestiParc"
    );
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    /// <summary>
    /// Chemin vers la base de données
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Vérifie si c'est le premier lancement (pas de fichier de configuration)
    /// </summary>
    public static bool IsFirstRun()
    {
        return !File.Exists(ConfigPath);
    }

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
    /// Supprime le fichier de configuration (utile pour les tests)
    /// </summary>
    public static void Reset()
    {
        if (File.Exists(ConfigPath))
        {
            File.Delete(ConfigPath);
        }
    }
}
