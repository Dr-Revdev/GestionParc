namespace GestiParc.Core.DTOs;

public sealed class ResetPasswordResponseDto
{
    public int UserId { get; set; }
    public string TemporaryPassword { get; set; } = "";
}
