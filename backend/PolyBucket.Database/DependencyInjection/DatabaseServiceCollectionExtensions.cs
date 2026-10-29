using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PolyBucket.Database.Persistence;
using PolyBucket.Database.Settings;

namespace PolyBucket.Database.DependencyInjection;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddPolyBucketDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<DatabaseSettings>, DatabaseSettingsValidator>();
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration("Database")
            .ValidateOnStart();
        services.AddDbContext<PolyBucketDbContext>((sp, options) =>
            options.UseNpgsql(
                sp.GetRequiredService<IOptions<DatabaseSettings>>().Value.BuildConnectionString(),
                npgsql => npgsql.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!)));

        return services;
    }
}
