using Microsoft.Extensions.DependencyInjection;
using MyIdentityApi.Services;
using Microsoft.AspNetCore.Identity;
using MyIdentityApi.Models;
using MyIdentityApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MyIdentityApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<JwtTokenService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<FileService>();

        return services;
    }

    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var postgreSqlConnection = configuration.GetConnectionString("PostgreSqlConnection");
        var sqlServerConnection = configuration.GetConnectionString("SqlServerConnection");

        if (postgreSqlConnection != null)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(postgreSqlConnection));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(sqlServerConnection));
        }

        return services;
    }

    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}