using MyIdentityApi.Models;
using MyIdentityApi.Dtos.Account;

namespace MyIdentityApi.Mappers;

public static class AccountMapper
{
    public static ApplicationUser ToEntity(RegisterDto dto)
    {
        return new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };
    }

    public static AuthResponseDto ToAuthResponseDto(string token, DateTime expiresAt, string refreshToken)
    {
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken
        };
    }
}