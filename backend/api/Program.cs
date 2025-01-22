using dotenv.net;
using Serilog.Events;
using Serilog;
using Api.Extensions;
using Api.Extensions.Environment;
using Microsoft.EntityFrameworkCore;
using Database;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load the .env file
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 4)); // this would only search 2 directories up from the executing directory.
            //Validate.ValidateEnvironmentVariables();
            // Configure logging
            // Log everything to console
#pragma warning disable CS8604 // Possible null reference argument. Env vars won't be null past our validation methods.
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
#if DEBUG
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Debug, theme: Extensions.Console.Console.Theme)
#endif
                .WriteTo.File(
                    "Logs/.log",
                    outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u3}]{Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: Logging.SetLogLevel(Environment.GetEnvironmentVariable(Variables.LOG_LEVEL)),
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
#pragma warning restore CS8604 // Possible null reference argument. Env vars won't be null past our validation methods.

            var builder = WebApplication.CreateBuilder(args);

            ConfigureDatabase(builder.Services);

            ConfigureApi(builder);
        }

        private static void ConfigureApi(WebApplicationBuilder builder)
        {
            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.UseResponseCaching();

            app.UseRouting();

            app.Run();

            Log.Information("Api services configured.");
        }

        private static void ConfigureDatabase(IServiceCollection services)
        {
            try
            {
                ServerVersion version = ServerVersion.AutoDetect(Context.ConnectionString);
                services.AddDbContext<Context>(options =>
                {
                    options.UseMySql(Context.ConnectionString, version);
                });
            }
            catch (MySqlConnector.MySqlException connectionException)
            {
                Log.Error(connectionException, "Failed to connect to database. Check your environment database settings.");
                throw;
            }
            catch (Exception configureDatabaseException)
            {
                Log.Error(configureDatabaseException, "Failed to configure database services.");
                throw;
            }

            Log.Information("Database connection established.");
        }
    }
}