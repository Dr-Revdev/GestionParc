using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ProjetParc.Data;

/// <summary>
/// Fichier de verrouillage pour empêcher l'accès concurrent
/// </summary>
public class LockFile
{
    public string User { get; set; }
    public string Machine { get; set; }
    public DateTime Timestamp { get; set; }
    public int ProcessId { get; set; }

    /// <summary>
    /// Crée un nouveau verrou avec les informations système actuelles
    /// </summary>
    public static LockFile Create()
    {
        return new LockFile
        {
            User = Environment.UserName,
            Machine = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            ProcessId = Environment.ProcessId
        };
    }

    /// <summary>
    /// Sauvegarde le verrou dans un fichier JSON
    /// </summary>
    public void Save(string lockFilePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(lockFilePath, json);
    }

    /// <summary>
    /// Charge un verrou depuis un fichier JSON
    /// </summary>
    public static LockFile Load(string lockFilePath)
    {
        try
        {
            if (!File.Exists(lockFilePath))
                return null;

            var json = File.ReadAllText(lockFilePath);
            return JsonSerializer.Deserialize<LockFile>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Vérifie si le verrou a expiré (plus de 4 heures)
    /// </summary>
    public bool IsExpired()
    {
        return (DateTime.UtcNow - Timestamp).TotalHours >= 4;
    }

    /// <summary>
    /// Vérifie si le processus qui a créé le verrou est encore actif
    /// </summary>
    public bool IsProcessAlive()
    {
        try
        {
            var process = Process.GetProcessById(ProcessId);
            return process != null && !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Génère une description lisible du verrou pour l'utilisateur
    /// </summary>
    public string GetDescription()
    {
        var localTime = Timestamp.ToLocalTime();
        var duration = DateTime.UtcNow - Timestamp;
        
        string durationText;
        if (duration.TotalMinutes < 60)
            durationText = $"{(int)duration.TotalMinutes} minute(s)";
        else
            durationText = $"{(int)duration.TotalHours} heure(s)";

        return $"Utilisateur : {User}\n" +
               $"Machine : {Machine}\n" +
               $"Depuis : {localTime:dd/MM/yyyy HH:mm}\n" +
               $"Durée : {durationText}";
    }
}
