using System;
using Microsoft.AspNetCore.Identity;

namespace MyIdentityApi.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}

