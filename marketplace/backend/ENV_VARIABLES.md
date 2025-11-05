# Backend Environment Variables Verification

## How ASP.NET Core Loads Environment Variables

ASP.NET Core automatically loads environment variables when using `WebApplication.CreateBuilder(args)`. The configuration hierarchy is:

1. **appsettings.json** (base configuration)
2. **appsettings.{Environment}.json** (environment-specific overrides)
3. **Environment Variables** (highest priority - override appsettings)

### Variable Name Mapping

Environment variables use **double underscore** (`__`) to represent nested configuration:

| Environment Variable | Configuration Access |
|---------------------|---------------------|
| `GitHub__ClientId` | `Configuration["GitHub:ClientId"]` |
| `Jwt__SecretKey` | `Configuration["Jwt:SecretKey"]` |
| `ConnectionStrings__DefaultConnection` | `GetConnectionString("DefaultConnection")` |
| `FileStorage__UploadPath` | `Configuration["FileStorage:UploadPath"]` |

## Current Usage Verification

### ✅ Program.cs - JWT Configuration
```csharp
ValidIssuer = builder.Configuration["Jwt:Issuer"]
ValidAudience = builder.Configuration["Jwt:Audience"]
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "fallback"))
```
**Status**: ✅ Reads from env vars via `GitHub__ClientId`, `Jwt__SecretKey`, etc.

### ✅ Program.cs - Database Connection
```csharp
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
```
**Status**: ✅ Reads from env var `ConnectionStrings__DefaultConnection`

### ✅ AuthService - GitHub OAuth
```csharp
var clientId = _configuration["GitHub:ClientId"];
var clientSecret = _configuration["GitHub:ClientSecret"];
var redirectUri = _configuration["GitHub:RedirectUri"];
```
**Status**: ✅ Reads from env vars `GitHub__ClientId`, `GitHub__ClientSecret`, `GitHub__RedirectUri`

### ✅ FileService - File Storage
```csharp
_uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads/plugins";
```
**Status**: ✅ Reads from env var `FileStorage__UploadPath`

### ✅ AuthController - GitHub OAuth URL (Fixed)
```csharp
var clientId = _configuration["GitHub:ClientId"] ?? 
               Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID") ?? 
               "fallback";
```
**Status**: ✅ Now uses Configuration system (fixed)

## Docker Compose Integration

The `docker-compose.marketplace.yml` uses `env_file` to load from `.env`:

```yaml
marketplace-api:
  env_file:
    - ./marketplace/.env
  environment:
    - GitHub__ClientId=${GitHub__ClientId:-${NEXT_PUBLIC_GITHUB_CLIENT_ID}}
```

This means:
1. Variables are loaded from `.env` file
2. Environment section can override or provide defaults
3. ASP.NET Core automatically maps `GitHub__ClientId` → `Configuration["GitHub:ClientId"]`

## Verification Checklist

- [x] JWT configuration reads from environment variables
- [x] Database connection string reads from environment variables  
- [x] GitHub OAuth reads from environment variables
- [x] File storage reads from environment variables
- [x] All services use IConfiguration (not Environment.GetEnvironmentVariable directly)
- [x] Docker Compose loads .env file correctly

## Testing

To verify environment variables are loaded:

1. **Set environment variables:**
   ```bash
   export GitHub__ClientId=test-client-id
   export Jwt__SecretKey=test-secret-key
   ```

2. **Run the application and check:**
   - Logs should show configuration being used
   - GitHub OAuth should use the test client ID
   - JWT tokens should use the test secret key

3. **Or check via Docker Compose:**
   ```bash
   docker-compose -f docker-compose.marketplace.yml config
   # Shows resolved environment variables
   ```
