using Microsoft.AspNetCore.Http;

namespace MyIdentityApi.Dtos.User;

public class ProfileImageDto
{
    public IFormFile File { get; set; } = null!;
}