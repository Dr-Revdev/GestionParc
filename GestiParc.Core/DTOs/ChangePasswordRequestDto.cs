namespace GestiParc.Core.DTOs;

public sealed class ChangePasswordRequestDto
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmNewPassword { get; set; } = "";
}
