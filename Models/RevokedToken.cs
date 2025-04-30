using MyIdentityApi.Models;

public class RevokedToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; }
    public ApplicationUser? User { get; set; }
}