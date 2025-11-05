using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/marketplace-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "PolyBucket Marketplace API",
        Description = "A comprehensive API for managing plugins, users, reviews, and installations in the PolyBucket marketplace ecosystem. " +
                      "This API enables developers to browse, install, and manage plugins, as well as integrate marketplace functionality into their own applications.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PolyBucket Marketplace",
            Email = "support@polybucket.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Group endpoints by tags
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
    options.DocInclusionPredicate((name, api) => true);

    // Enable annotations
    options.EnableAnnotations();
});

// Database
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<MarketplaceUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<MarketplaceDbContext>()
.AddDefaultTokenProviders();

// Authorization Policies
var adminUsernamesConfig = builder.Configuration["GitHub:AdminUsernames"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            var githubIdClaim = user.FindFirst("github_id")?.Value;
            var githubUsernameClaim = user.FindFirst("github_username")?.Value;
            
            // Check if user is site admin
            // Admin GitHub usernames can be configured via environment variable GitHub__AdminUsernames (comma-separated)
            return adminUsernamesConfig.Contains(githubUsernameClaim ?? "") ||
                   user.IsInRole("SiteAdmin");
        }));

    options.AddPolicy("RequireAdminOrModerator", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            return user.IsInRole("SiteAdmin") || user.IsInRole("Moderator");
        }));

    options.AddPolicy("RequireDeveloperOrHigher", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            return user.IsInRole("SiteAdmin") || 
                   user.IsInRole("Moderator") || 
                   user.IsInRole("Developer");
        }));

    options.AddPolicy("RequireAuthenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                          "http://localhost:3001", 
                          "http://localhost:3002", 
                          "http://localhost:32768",
                          "http://host.docker.internal:32768",
                          "http://polybucket-client-1:3000",
                          "http://127.0.0.1:32768",
                          "http://localhost:10101",
                          "http://localhost:10110",
                          "http://localhost:11666",
                          "http://127.0.0.1:10110"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

// JWT Authentication
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey must be configured via Jwt__SecretKey environment variable or appsettings.json")))
    };
});

// Services
builder.Services.AddScoped<IPluginService, PluginService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", true) || app.Environment.IsDevelopment();

if (enableSwagger)
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PolyBucket Marketplace API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "PolyBucket Marketplace API Documentation";
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
        options.EnableValidator();
        options.SupportedSubmitMethods(new[] { Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Patch });
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database schema is created (only if connection is available)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        context.Database.EnsureCreated();
        Log.Logger.Information("Database schema ensured");
    }
}
catch (Exception ex)
{
    Log.Logger.Warning(ex, "Could not connect to database during startup. Database will be created when first accessed.");
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }
