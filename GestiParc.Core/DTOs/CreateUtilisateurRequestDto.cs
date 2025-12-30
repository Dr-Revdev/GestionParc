namespace GestiParc.Core.DTOs;

public sealed class CreateUtilisateurRequestDto
{
    public string Username { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Role { get; set; } = "USER";
    public bool Actif { get; set; } = true;

    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}
