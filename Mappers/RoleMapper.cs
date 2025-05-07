using Microsoft.AspNetCore.Identity;
using MyIdentityApi.Dtos.Role;

namespace MyIdentityApi.Mappers;

public static class RoleMapper
{
    public static RoleResponseDto ToResponseDto(IdentityRole role)
    {
        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name
        };
    }

    public static IdentityRole ToEntity(CreateRoleDto dto)
    {
        return new IdentityRole
        {
            Name = dto.Name
        };
    }

    public static void UpdateEntity(IdentityRole role, UpdateRoleDto dto)
    {
        role.Name = dto.Name;
    }
}