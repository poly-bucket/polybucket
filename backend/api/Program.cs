using Api.Extensions;
using Api.Extensions.Database;
using Api.Extensions.Environment;
using Core.Services;
using Database;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Database.Seeders;
using System.Security.Claims;
using Api.Controllers.Users.GetUserById.Persistance;
using Api.Controllers.Users.GetUserById.Domain;
using Api.Models;
using Api.Controllers.Authentication.Persistance;
using Api.Controllers.Authentication.Domain;
using Api.Controllers.Models.Domain;
using Api.Controllers.Models.Persistance;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using api.Controllers.Printers.Domain;
using Api.Controllers.Users.UserSettings.Domain;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Load the .env file
                DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 4));
                Validate.ValidateEnvironmentVariables();

                // Configure logging
#pragma warning disable CS8604
                Log.Logger = new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Debug()
#else
                    .MinimumLevel.Information()
#endif
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
#if DEBUG
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Debug,
                        theme: Extensions.Console.Console.Theme)
#endif
                    .WriteTo.File(
                        "Logs/.log",
                        outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u3}]{Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: Logging.SetLogLevel(Environment.GetEnvironmentVariable(Variables.LOG_LEVEL)),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
#pragma warning restore CS8604

                var builder = WebApplication.CreateBuilder(args);

                ConfigureDatabase(builder.Services);

                await ConfigureApi(builder);
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

        private static async Task ConfigureApi(WebApplicationBuilder builder)
        {
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Add plugins
            builder.Services.AddPlugins();

            // Add CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // Be careful with this in production!
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders("Content-Disposition")
                          .AllowCredentials();
                });
            });

            // Configure static files
            builder.Services.AddDirectoryBrowser();

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddTransient<GetUserByIdDataAccess>();
            builder.Services.AddTransient<GetUserByIdService>();
            builder.Services.AddTransient<CreateUserLoginDataAccess>();
            builder.Services.AddTransient<CreateUserLoginService>();

            // Models
            builder.Services.AddTransient<IGetModelsDataAccess, GetModelsDataAccess>();
            builder.Services.AddTransient<IGetModelsService, GetModelsService>();
            builder.Services.AddTransient<IGetModelByIdDataAccess, GetModelByIdDataAccess>();
            builder.Services.AddTransient<IGetModelByIdService, GetModelByIdService>();

            // Printers
            builder.Services.AddTransient<IGetPrintersService, GetPrintersService>();

            // User Settings
            builder.Services.AddTransient<IGetUserSettingsService, GetUserSettingsService>();
            builder.Services.AddTransient<IUpdateUserSettingsService, UpdateUserSettingsService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ??
                                throw new ArgumentNullException("JWT_SECRET"))),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };

                    // Add debug logging for token validation
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();

                            var userIdClaim = context.Principal?.FindFirst("userId")?.Value;
                            logger.LogInformation("Token validated for user: {UserId}", userIdClaim);

                            return Task.CompletedTask;
                        }
                    };
                });

            // Add response caching before building the app
            builder.Services.AddResponseCaching();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json",
                        $"{Environment.GetEnvironmentVariable(Variables.INSTANCE_NAME)} API v1");
                });
            }

            // Order matters! CORS should be early in the pipeline
            app.UseCors("AllowFrontend");
            app.UseStaticFiles();

            // Ensure files directory exists
            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "files");
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
                Log.Information("Created files directory at: {Path}", filesPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(filesPath),
                RequestPath = "/files",
                ServeUnknownFileTypes = true
            });

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Apply any pending migrations
            app.ApplyMigrations<Context>();

            // Seed development data
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<Context>();
                    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                    // Updated seeder instantiation and calls
                    var adminSeeder = new AdminSeeder(context, passwordHasher);
                    await adminSeeder.Seed();

                    var printerSeeder = new PrinterSeeder(context);
                    await printerSeeder.Seed();

                    var modelSeeder = new ModelSeeder(context);
                    modelSeeder.Seed();
                }
            }

            Log.Information("Api services configured.");

            await app.RunAsync();
        }

        private static void ConfigureDatabase(IServiceCollection services)
        {
            try
            {
                services.AddDbContext<Context>(
                    DbContextOptions => DbContextOptions
                        .UseMySql(Context.ConnectionString, ServerVersion.AutoDetect(Context.ConnectionString)));

                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
                    dbContext.Database.Migrate();
                }

                Log.Information("Database connection established.");
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
        }
    }
}