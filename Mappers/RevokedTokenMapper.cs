using MyIdentityApi.Models;
using MyIdentityApi.Dtos.Account;

namespace MyIdentityApi.Mappers;

public static class RevokedTokenMapper
{
    public static RevokedToken ToEntity(string token, string userId)
    {
        return new RevokedToken
        {
            Token = token,
            UserId = userId,
            RevokedAt = DateTime.UtcNow
        };
    }
}