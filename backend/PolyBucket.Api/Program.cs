using Serilog;
using Serilog.Events;
using PolyBucket.Api.Extensions.Serilog;
using PolyBucket.Api.Extensions;

namespace PolyBucket.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: ConsoleTheme.CustomTheme)
#endif
            .WriteTo.File(
                path: "Logs/.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LoggingExtensions.SetLogLevel(builder.Configuration["Logging:LogLevel:Default"] ?? "Information"),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting PolyBucket API");
            
            builder.Host.UseSerilog();

            // Configure services using extension methods
            builder.Services
                .AddPolyBucketServices(builder.Configuration)
                .AddPolyBucketDatabase(builder.Configuration)
                .AddPolyBucketFeatures()
                .AddPolyBucketAuthentication(builder.Configuration)
                .AddPolyBucketCors()
                .AddPolyBucketHealthChecks(builder.Configuration)
                .AddPolyBucketOpenApi();

            // Add object storage
            builder.Services.AddObjectStorage(builder.Configuration);

            var app = builder.Build();

            // Configure application using extension methods
            await app.ConfigurePolyBucketApplicationAsync();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
} 