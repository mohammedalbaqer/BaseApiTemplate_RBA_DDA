namespace MyIdentityApi.Dtos.User;

public class PasswordUpdateDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}