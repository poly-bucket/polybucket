using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Identity;
using Infrastructure.Services;
using Core.Configuration;

namespace Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<DbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                // Suppress the PendingModelChangesWarning which can cause migration issues
                .ConfigureWarnings(warnings => 
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            );

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ISystemSetupRepository, SystemSetupRepository>();
            
            // Register identity services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Register role services
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<RoleSeedService>();
            
            // Register other services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStorageService, LocalStorageService>();

            // Note: AppSettings configuration is done in Program.cs

            return services;
        }
    }
} 