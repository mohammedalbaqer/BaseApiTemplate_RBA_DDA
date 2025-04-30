using System;

namespace MyIdentityApi.Dtos;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

