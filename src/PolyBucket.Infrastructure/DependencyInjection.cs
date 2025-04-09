using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Core.Interfaces;
using PolyBucket.Infrastructure.Data;
using PolyBucket.Infrastructure.Data.Repositories;
using PolyBucket.Infrastructure.Identity;
using PolyBucket.Infrastructure.Services;

namespace PolyBucket.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("PolyBucket.Api")));

            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Add identity services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            // Email service
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
} 