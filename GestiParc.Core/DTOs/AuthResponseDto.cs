namespace GestiParc.Core.DTOs;

public sealed class AuthResponseDto
{
    public UtilisateurDto User { get; set; } = new UtilisateurDto();
    public string Token { get; set; } = "";
    public int ExpiresIn { get; set; }
}