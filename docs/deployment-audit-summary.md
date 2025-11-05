# Deployment Audit Summary

This document summarizes the changes made to prepare PolyBucket for home server deployment.

## Changes Made

### 1. Hardcoded Environment Variables Refactored

#### Backend (`appsettings.json`)
- **ConnectionStrings**: Removed hardcoded database connection string
- **Admin**: Removed hardcoded admin credentials (username, email, password)
- **Storage Settings**: Removed hardcoded MinIO endpoints, credentials, and bucket names
- **CORS**: Added empty array for CORS origins (must be configured via environment variables)
- **Email Settings**: Removed hardcoded SMTP credentials

All configuration values are now empty strings or empty arrays in `appsettings.json`, requiring environment variables to be set for production deployment.

#### Frontend (`nginx.conf`)
- **API Proxy**: Converted to template (`nginx.conf.template`) with environment variable substitution
- **MinIO Proxy**: Converted to use environment variable for MinIO URL
- **Docker Entrypoint**: Created `docker-entrypoint.sh` to handle environment variable substitution at container startup

### 2. CORS Configuration Enhanced

#### Changes:
- Production mode now supports environment variable `CORS__ALLOWED_ORIGINS` (comma-separated list)
- Can also be configured via `appsettings.json` under `Cors:AllowedOrigins`
- Application fails to start if CORS origins are not configured in production
- Development mode remains permissive for local development

#### Usage:
```bash
# Via environment variable
CORS__ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com

# Or in appsettings.json
{
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

### 3. Docker Configuration

#### Frontend Dockerfile
- Updated to use `nginx.conf.template` instead of hardcoded `nginx.conf`
- Added `docker-entrypoint.sh` for environment variable substitution
- Installs `gettext` package for `envsubst` utility
- Supports `API_URL` and `MINIO_URL` environment variables

#### Backend Dockerfile
- Already supports environment variables via ASP.NET Core configuration system
- No changes needed - configuration is read from environment variables automatically

### 4. Documentation Created

#### `docs/deployment.md`
- Complete deployment guide
- Docker build instructions
- Image export/import procedures
- Docker Compose configuration examples
- Reverse proxy setup guides
- Troubleshooting section

#### `docs/environment-variables.md`
- Complete reference of all environment variables
- Configuration priority explanation
- Variable naming conventions
- Examples for development and production
- Security best practices

## Additional Considerations

### 1. Database Migrations
- Migrations run automatically on application startup
- Ensure database is accessible before starting the API container
- Use health checks in docker-compose to ensure proper startup order

### 2. Storage Configuration
- **Internal Endpoint**: Used by backend to connect to storage (e.g., `minio` for Docker service name)
- **External Endpoint**: Used for presigned URLs accessible from frontend (e.g., `yourdomain.com`)
- Ensure external endpoint is accessible from the internet if using presigned URLs

### 3. Reverse Proxy Requirements
- Must configure CORS origins to match your domain
- Must handle SSL/TLS termination
- Should proxy `/api` requests to backend API
- Should proxy `/minio` requests if using MinIO (optional)

### 4. Security Considerations
- **JWT Secret**: Must be at least 32 characters in production
- **Admin Password**: Set via `Admin__Password` or auto-generated on first run
- **Database Password**: Use strong passwords
- **Storage Credentials**: Secure MinIO/S3 credentials
- **CORS Origins**: Only include trusted domains

### 5. Frontend API Configuration
- Frontend can use relative paths (`/api`) when served behind reverse proxy
- Or set `VITE_API_URL` at build time if using absolute URLs
- Nginx container handles API proxying when using Docker Compose

### 6. Logging
- Backend logs to console and file (`Logs/.log`)
- Logs directory is created automatically
- Consider log rotation for production

### 7. Health Checks
- Backend: `http://localhost:11666/health`
- Database: PostgreSQL health check via `pg_isready`
- MinIO: Health check on port 9000
- All configured in docker-compose

### 8. Port Configuration
- Backend API: Port 11666 (internal)
- Frontend: Port 80 (internal, exposed as 8080 in example)
- Database: Port 5432 (internal)
- MinIO: Ports 9000 (API) and 9001 (Console)

### 9. Volume Persistence
- Database: `postgres-data` volume
- MinIO: `minio-data` volume
- Backend logs: Stored in container (consider volume mount for persistence)

### 10. Network Configuration
- All services communicate via `polybucket-network` Docker network
- Frontend proxy uses service names (e.g., `api:11666`, `minio:9000`)
- External access handled by reverse proxy

## Testing Checklist

Before deploying to production:

- [ ] All environment variables are set correctly
- [ ] CORS origins match your frontend domain
- [ ] Database connection is working
- [ ] Storage service is accessible
- [ ] JWT secret is at least 32 characters
- [ ] Admin user can be created/login
- [ ] Frontend can reach backend API
- [ ] Reverse proxy is configured correctly
- [ ] SSL/TLS certificates are valid
- [ ] Health checks are passing
- [ ] Logs are being generated correctly

## Migration from Development to Production

1. **Update Configuration**:
   - Set all required environment variables
   - Configure CORS origins
   - Set secure passwords and secrets

2. **Build Images**:
   ```bash
   docker build -f Dockerfile.backend -t polybucket-api:latest --target runtime .
   docker build -f Dockerfile.frontend -t polybucket-frontend:latest --target production .
   ```

3. **Export and Transfer**:
   ```bash
   docker save polybucket-api:latest | gzip > polybucket-api-latest.tar.gz
   docker save polybucket-frontend:latest | gzip > polybucket-frontend-latest.tar.gz
   # Transfer to server
   ```

4. **Deploy**:
   - Load images on server
   - Create `.env` file with all variables
   - Start services with `docker-compose -f docker-compose.production.yml up -d`

5. **Verify**:
   - Check health endpoints
   - Test admin login
   - Verify CORS is working
   - Test file uploads

## Future Improvements

Consider implementing:
- Kubernetes deployment manifests
- CI/CD pipeline for automated builds
- Secret management (e.g., Docker Secrets, HashiCorp Vault)
- Automated backups
- Monitoring and alerting
- Log aggregation
- Health check dashboard

## Support

For issues or questions:
1. Check logs: `docker-compose logs -f`
2. Review environment variables documentation
3. Verify configuration values
4. Check network connectivity between containers

