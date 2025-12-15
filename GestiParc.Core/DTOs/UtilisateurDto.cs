namespace GestiParc.Core.DTOs;

public class UtilisateurDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Role { get; set; } = "USER";
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereConnexion { get; set; }
    public bool Actif { get; set; } = true;
}
