using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MyIdentityApi.Models;
using MyIdentityApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MyIdentityApi.Services;

public class RefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var refreshToken = new RefreshToken
        {
            Token = GenerateToken(),
            ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return await _context.RefreshTokens
            .Where(rt => rt.Token == token && rt.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        _context.RefreshTokens.Remove(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        if (userTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(userTokens);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var refreshToken = await GetRefreshTokenAsync(token);
        return refreshToken != null && !refreshToken.IsRevoked && refreshToken.ExpiryDate > DateTime.UtcNow;
    }

    // public async Task CleanupExpiredTokensAsync()
    // {
    //     var expiredTokens = await _context.RefreshTokens
    //         .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
    //         .ToListAsync();

    //     if (expiredTokens.Any())
    //     {
    //         _context.RefreshTokens.RemoveRange(expiredTokens);
    //         await _context.SaveChangesAsync();
    //     }
    // }

    private static string GenerateToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
