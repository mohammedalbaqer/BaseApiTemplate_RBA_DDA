using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyIdentityApi.Data;
using MyIdentityApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyIdentityApi.Extensions
{
    public static class JwtBearerExtensions
    {
        public static void AddJwtBearer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))
                    ),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                        
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(userId))
                        {
                            context.Fail("Invalid token: missing user identifier");
                            return;
                        }

                        // Check if token is revoked
                        var tokenString = context.Request.Headers["Authorization"]
                            .ToString().Replace("Bearer ", "");

                        var isRevoked = await dbContext.RevokedTokens
                            .AnyAsync(rt => rt.Token == tokenString);

                        if (isRevoked)
                        {
                            context.Fail("Token has been revoked");
                            return;
                        }

                        // Verify user exists and is active
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<ApplicationUser>>();

                        var user = await userManager.FindByIdAsync(userId);
                        if (user == null)
                        {
                            context.Fail("User not found or inactive");
                            return;
                        }
                        
                    }
                };
            });
        }
    }
}