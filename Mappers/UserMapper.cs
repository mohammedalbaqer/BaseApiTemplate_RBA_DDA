using MyIdentityApi.Models;
using MyIdentityApi.Dtos.User;

namespace MyIdentityApi.Mappers;

public static class UserMapper
{
    public static UserResponseDto ToResponseDto(ApplicationUser user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    public static void UpdateEntity(ApplicationUser user, UpdateUserDto dto)
    {
        user.Email = dto.Email;
        user.UserName = dto.Username;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
    }
}