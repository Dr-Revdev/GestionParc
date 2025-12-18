using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace GestiParc.Ui.Services;

/// <summary>
/// Service de stockage sécurisé du token JWT utilisant DPAPI (Data Protection API) de Windows
/// Le token est chiffré avec les credentials de l'utilisateur Windows courant
/// </summary>
public static class TokenStorage
{
    private static readonly string TokenFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GestiParc",
        ".token");

    /// <summary>
    /// Sauvegarde le token de manière sécurisée (chiffré avec DPAPI)
    /// </summary>
    public static void SaveToken(string token, string username)
    {
        try
        {
            // Créer le répertoire si nécessaire
            var directory = Path.GetDirectoryName(TokenFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Données à chiffrer (token + username pour validation)
            var data = $"{username}|{token}";
            var dataBytes = Encoding.UTF8.GetBytes(data);

            // Chiffrer avec DPAPI (scope: CurrentUser)
            var encryptedData = ProtectedData.Protect(
                dataBytes,
                null, // pas de salt additionnel
                DataProtectionScope.CurrentUser);

            // Écrire dans le fichier
            File.WriteAllBytes(TokenFilePath, encryptedData);
        }
        catch (Exception ex)
        {
            // Log l'erreur mais ne pas crasher l'application
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde du token: {ex.Message}");
        }
    }

    /// <summary>
    /// Récupère le token sauvegardé (déchiffré)
    /// </summary>
    /// <returns>Le token si valide, null sinon</returns>
    public static string? GetToken(string username)
    {
        try
        {
            if (!File.Exists(TokenFilePath))
                return null;

            // Lire le fichier
            var encryptedData = File.ReadAllBytes(TokenFilePath);

            // Déchiffrer avec DPAPI
            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);

            var data = Encoding.UTF8.GetString(decryptedData);
            var parts = data.Split('|');

            if (parts.Length != 2)
                return null;

            var storedUsername = parts[0];
            var token = parts[1];

            // Vérifier que le username correspond (sécurité additionnelle)
            if (storedUsername != username)
                return null;

            return token;
        }
        catch (Exception ex)
        {
            // Token corrompu ou impossible à déchiffrer
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération du token: {ex.Message}");
            ClearToken();
            return null;
        }
    }

    /// <summary>
    /// Supprime le token stocké (déconnexion)
    /// </summary>
    public static void ClearToken()
    {
        try
        {
            if (File.Exists(TokenFilePath))
            {
                File.Delete(TokenFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression du token: {ex.Message}");
        }
    }

    /// <summary>
    /// Vérifie si un token existe (sans le déchiffrer)
    /// </summary>
    public static bool HasToken()
    {
        return File.Exists(TokenFilePath);
    }
}
