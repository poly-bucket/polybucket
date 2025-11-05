# Environment Variables Reference

This document lists all configurable environment variables for PolyBucket.

## Configuration Priority

ASP.NET Core loads configuration in the following order (highest priority last):
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables (using `__` for nested configuration)

## Environment Variable Naming

Nested configuration uses double underscores (`__`):
- `AppSettings:Security:JwtSecret` → `AppSettings__Security__JwtSecret`
- `ConnectionStrings:DefaultConnection` → `ConnectionStrings__DefaultConnection`

## Required Variables

### Backend (API)

#### Database Configuration
- `ConnectionStrings__DefaultConnection` (Required)
  - Format: `Host=hostname;Port=5432;Database=dbname;Username=user;Password=pass;`
  - Example: `Host=db;Port=5432;Database=polybucket;Username=polybucket;Password=securepass;`

#### Admin User
- `Admin__Username` (Optional, defaults to "admin")
  - Username for the initial admin account
- `Admin__Email` (Optional, defaults to "admin@polybucket.com")
  - Email for the initial admin account
- `Admin__Password` (Optional, auto-generated if not set)
  - Password for the initial admin account. If not set, a secure password will be generated and logged.

#### JWT Security
- `AppSettings__Security__JwtSecret` or `JWT_SECRET` (Required in Production)
  - Minimum 32 characters
  - Used for signing JWT tokens
  - Example: `your-super-secret-key-minimum-32-characters-long`
- `AppSettings__Security__JwtIssuer` (Optional, defaults to "polybucket-api")
  - JWT token issuer identifier
- `AppSettings__Security__JwtAudience` (Optional, defaults to "polybucket-client")
  - JWT token audience identifier

#### CORS Configuration
- `CORS__ALLOWED_ORIGINS` (Required in Production)
  - Comma-separated list of allowed origins
  - Example: `https://yourdomain.com,https://www.yourdomain.com`
  - Or in appsettings.json: `"Cors": { "AllowedOrigins": ["https://yourdomain.com"] }`

#### Storage Configuration
- `AppSettings__Storage__Provider` (Optional, defaults to "MinIO")
  - Options: `MinIO`, `S3`, `AzureBlob`
- `AppSettings__Storage__Endpoint` (Required)
  - Internal endpoint/hostname for storage service
  - Example: `minio` (for Docker service name) or `s3.amazonaws.com`
- `AppSettings__Storage__ExternalEndpoint` (Required for presigned URLs)
  - External endpoint accessible from frontend
  - Example: `yourdomain.com` or `s3.amazonaws.com`
- `AppSettings__Storage__Port` (Optional, defaults to 9000)
  - Internal port for storage service
- `AppSettings__Storage__ExternalPort` (Optional, defaults to Storage Port)
  - External port for storage service
- `AppSettings__Storage__UseSSL` (Optional, defaults to false)
  - Use SSL for internal storage connections
- `AppSettings__Storage__ExternalUseSSL` (Optional, defaults to Storage UseSSL)
  - Use SSL for external storage connections
- `AppSettings__Storage__BucketName` (Required)
  - Storage bucket/container name
  - Example: `polybucket-uploads`
- `AppSettings__Storage__AccessKey` (Required)
  - Storage access key
- `AppSettings__Storage__SecretKey` (Required)
  - Storage secret key
- `AppSettings__Storage__Region` (Optional, defaults to "us-east-1")
  - Storage region (for S3-compatible services)

#### Frontend Configuration
- `AppSettings__Frontend__BaseUrl` (Optional)
  - Base URL of the frontend application
  - Used for generating email links and redirects
  - Example: `https://yourdomain.com`

#### Email Configuration (Optional)
- `AppSettings__Email__SmtpServer` (Optional)
  - SMTP server hostname
- `AppSettings__Email__SmtpPort` (Optional, defaults to 587)
  - SMTP server port
- `AppSettings__Email__SmtpUsername` (Optional)
  - SMTP authentication username
- `AppSettings__Email__SmtpPassword` (Optional)
  - SMTP authentication password
- `AppSettings__Email__UseSsl` (Optional, defaults to true)
  - Use SSL/TLS for SMTP
- `AppSettings__Email__FromEmail` (Optional)
  - From email address
- `AppSettings__Email__FromName` (Optional, defaults to "PolyBucket")
  - From name for emails

### Frontend (Nginx Container)

#### API Proxy
- `API_URL` (Optional, defaults to "http://localhost:11666")
  - Backend API URL for nginx proxy
  - Example: `http://api:11666` (Docker service name)
  - Example: `http://localhost:11666` (local development)

#### MinIO Proxy
- `MINIO_URL` (Optional, defaults to "http://minio:9000")
  - MinIO URL for nginx proxy
  - Example: `http://minio:9000` (Docker service name)

### Frontend Build-Time Variables

These are set during `npm run build`:

- `VITE_API_URL` (Optional, defaults to "http://localhost:11666")
  - API base URL for frontend API client
  - Example: `https://api.yourdomain.com`
  - Note: If using nginx proxy, can use relative paths (omit this)

## Docker Compose Examples

### Minimal Production Configuration

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Host=db;Database=polybucket;Username=user;Password=pass
  - Admin__Password=secure-admin-password
  - AppSettings__Security__JwtSecret=your-32-char-minimum-secret-key
  - CORS__ALLOWED_ORIGINS=https://yourdomain.com
  - AppSettings__Storage__Endpoint=minio
  - AppSettings__Storage__ExternalEndpoint=yourdomain.com
  - AppSettings__Storage__BucketName=polybucket-uploads
  - AppSettings__Storage__AccessKey=minio-access-key
  - AppSettings__Storage__SecretKey=minio-secret-key
```

### Full Production Configuration

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://*:11666
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=polybucket;Username=polybucket;Password=securepass
  - Admin__Username=admin
  - Admin__Email=admin@yourdomain.com
  - Admin__Password=secure-admin-password
  - AppSettings__Security__JwtSecret=your-very-long-secret-key-minimum-32-characters
  - AppSettings__Security__JwtIssuer=polybucket-api
  - AppSettings__Security__JwtAudience=polybucket-client
  - AppSettings__Security__AccessTokenExpiryMinutes=60
  - AppSettings__Security__RefreshTokenExpiryDays=7
  - AppSettings__Frontend__BaseUrl=https://yourdomain.com
  - CORS__ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
  - AppSettings__Storage__Provider=MinIO
  - AppSettings__Storage__Endpoint=minio
  - AppSettings__Storage__ExternalEndpoint=yourdomain.com
  - AppSettings__Storage__Port=9000
  - AppSettings__Storage__ExternalPort=9000
  - AppSettings__Storage__UseSSL=false
  - AppSettings__Storage__ExternalUseSSL=true
  - AppSettings__Storage__BucketName=polybucket-uploads
  - AppSettings__Storage__AccessKey=minio-access-key
  - AppSettings__Storage__SecretKey=minio-secret-key
  - AppSettings__Storage__Region=us-east-1
  - AppSettings__Email__SmtpServer=smtp.gmail.com
  - AppSettings__Email__SmtpPort=587
  - AppSettings__Email__SmtpUsername=your-email@gmail.com
  - AppSettings__Email__SmtpPassword=your-app-password
  - AppSettings__Email__UseSsl=true
  - AppSettings__Email__FromEmail=noreply@yourdomain.com
  - AppSettings__Email__FromName=PolyBucket
```

## Development vs Production

### Development

In Development mode, CORS is permissive and allows:
- `http://localhost:3000`
- `http://localhost:3001`
- `http://localhost:3002`
- Various other localhost ports

Default values are used for many settings.

### Production

In Production mode:
- CORS must be explicitly configured via `CORS__ALLOWED_ORIGINS`
- JWT secret must be at least 32 characters
- All required storage settings must be provided
- Application will fail to start if required configuration is missing

## Validation

The application validates:
- JWT secret length (minimum 32 characters)
- CORS origins (must be configured in Production)
- Database connection string (must be provided)
- Storage configuration (endpoint, credentials, bucket name)

## Security Best Practices

1. **Never commit secrets** - Use environment variables or secure secret management
2. **Use strong passwords** - Generate secure random passwords
3. **Rotate secrets regularly** - Change JWT secrets periodically
4. **Limit CORS origins** - Only allow trusted domains
5. **Use HTTPS** - Always use SSL in production
6. **Secure storage credentials** - Use IAM roles when possible (AWS S3)

## Troubleshooting

### Configuration Not Loading

1. Check environment variable names (use `__` for nesting)
2. Verify variable is set: `echo $VARIABLE_NAME`
3. Check application logs for configuration errors
4. Verify `ASPNETCORE_ENVIRONMENT` is set correctly

### CORS Issues

1. Verify `CORS__ALLOWED_ORIGINS` includes your frontend domain
2. Check exact domain match (protocol, domain, port)
3. Ensure no trailing slashes in CORS origins
4. Check browser console for CORS error details

### Storage Connection Issues

1. Verify storage endpoint is accessible
2. Check credentials are correct
3. Ensure bucket exists and is accessible
4. Verify network connectivity between containers

