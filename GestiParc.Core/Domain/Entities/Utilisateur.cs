namespace GestiParc.Core.Domain.Entities;

/// <summary>
/// Représente un utilisateur du système avec ses informations d'authentification
/// </summary>
public class Utilisateur
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Role { get; set; } = "USER"; // "ADMIN" ou "USER"
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereConnexion { get; set; }
    public bool Actif { get; set; } = true;

    /// <summary>
    /// Retourne le nom complet de l'utilisateur
    /// </summary>
    public string NomComplet => $"{Prenom} {Nom}";

    /// <summary>
    /// Vérifie si l'utilisateur a le rôle administrateur
    /// </summary>
    public bool EstAdmin => Role == "ADMIN";
}
