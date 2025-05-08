namespace MyIdentityApi.Dtos.Account;

public class RevokedTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; }
}