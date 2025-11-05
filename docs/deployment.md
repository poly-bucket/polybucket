# PolyBucket Deployment Guide

This guide covers deploying PolyBucket on a home server using Docker.

## Prerequisites

- Docker and Docker Compose installed on your server
- A reverse proxy (e.g., Nginx, Traefik, Caddy) configured to route traffic to PolyBucket
- PostgreSQL database (can be containerized or external)
- MinIO or S3-compatible storage (can be containerized or external)

## Building Docker Images

### Using GitHub Actions (Recommended)

The repository includes a GitHub Actions workflow (`.github/workflows/docker-build.yml`) that automatically builds Docker images when you push to the main/master branch.

**How it works:**
- Images are automatically built and pushed to GitHub Container Registry (ghcr.io)
- Images are tagged with: `latest`, branch name, commit SHA, and semantic version (if you create a tag like `v1.0.0`)
- Pull requests build images but don't push them (for testing)

**Pulling images from GitHub Container Registry:**

```bash
# Login to GitHub Container Registry (first time only)
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Pull backend image
docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest

# Pull frontend image
docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest
```

**Note:** Replace `YOUR_USERNAME` with your GitHub username and `polybucket` with your repository name.

### Building Locally

#### Build Backend Image

```bash
docker build -f Dockerfile.backend -t polybucket-api:latest --target runtime .
```

#### Build Frontend Image

```bash
docker build -f Dockerfile.frontend -t polybucket-frontend:latest --target production .
```

### Export Images for Transfer

To save images for transfer to your server:

```bash
# Save backend image
docker save polybucket-api:latest | gzip > polybucket-api-latest.tar.gz

# Save frontend image
docker save polybucket-frontend:latest | gzip > polybucket-frontend-latest.tar.gz
```

### Load Images on Server

On your server, load the images:

```bash
# Load backend image
gunzip -c polybucket-api-latest.tar.gz | docker load

# Load frontend image
gunzip -c polybucket-frontend-latest.tar.gz | docker load
```

### Using Images from GitHub Container Registry

If you're using GitHub Actions, you can pull images directly on your server:

```bash
# Login to GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_USERNAME --password-stdin

# Pull latest images
docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest
docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest

# Tag for local use (optional)
docker tag ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest polybucket-api:latest
docker tag ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest polybucket-frontend:latest
```

## Docker Compose Configuration

Create a `docker-compose.production.yml` file for your deployment:

```yaml
services:
  api:
    image: polybucket-api:latest
    container_name: polybucket-api
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://*:11666
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=polybucket;Username=${DB_USER:-polybucket};Password=${DB_PASSWORD}
      - Admin__Username=${ADMIN_USERNAME:-admin}
      - Admin__Email=${ADMIN_EMAIL}
      - Admin__Password=${ADMIN_PASSWORD}
      - AppSettings__Security__JwtSecret=${JWT_SECRET}
      - AppSettings__Security__JwtIssuer=${JWT_ISSUER:-polybucket-api}
      - AppSettings__Security__JwtAudience=${JWT_AUDIENCE:-polybucket-client}
      - AppSettings__Frontend__BaseUrl=${FRONTEND_URL:-https://yourdomain.com}
      - AppSettings__Storage__Provider=${STORAGE_PROVIDER:-MinIO}
      - AppSettings__Storage__Endpoint=${STORAGE_ENDPOINT:-minio}
      - AppSettings__Storage__ExternalEndpoint=${STORAGE_EXTERNAL_ENDPOINT}
      - AppSettings__Storage__Port=${STORAGE_PORT:-9000}
      - AppSettings__Storage__ExternalPort=${STORAGE_EXTERNAL_PORT:-9000}
      - AppSettings__Storage__UseSSL=${STORAGE_USE_SSL:-false}
      - AppSettings__Storage__ExternalUseSSL=${STORAGE_EXTERNAL_USE_SSL:-false}
      - AppSettings__Storage__BucketName=${STORAGE_BUCKET_NAME:-polybucket-uploads}
      - AppSettings__Storage__AccessKey=${STORAGE_ACCESS_KEY}
      - AppSettings__Storage__SecretKey=${STORAGE_SECRET_KEY}
      - AppSettings__Storage__Region=${STORAGE_REGION:-us-east-1}
      - CORS__ALLOWED_ORIGINS=${CORS_ALLOWED_ORIGINS}
    depends_on:
      db:
        condition: service_healthy
    networks:
      - polybucket-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:11666/health"]
      interval: 30s
      timeout: 10s
      retries: 5

  frontend:
    image: polybucket-frontend:latest
    container_name: polybucket-frontend
    restart: unless-stopped
    environment:
      - API_URL=${API_URL:-http://api:11666}
      - MINIO_URL=${MINIO_URL:-http://minio:9000}
    ports:
      - "8080:80"
    networks:
      - polybucket-network

  db:
    image: postgres:latest
    container_name: polybucket-db
    restart: unless-stopped
    environment:
      POSTGRES_DB: polybucket
      POSTGRES_USER: ${DB_USER:-polybucket}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - polybucket-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-polybucket} -d polybucket"]
      interval: 10s
      timeout: 5s
      retries: 5

  minio:
    image: minio/minio:latest
    container_name: polybucket-minio
    restart: unless-stopped
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER:-minioadmin}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    volumes:
      - minio-data:/data
    command: server /data --console-address ":9001"
    networks:
      - polybucket-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 30s
      timeout: 20s
      retries: 3

networks:
  polybucket-network:
    name: polybucket-network
    driver: bridge

volumes:
  postgres-data:
  minio-data:
```

## Environment Variables

Create a `.env` file in the same directory as your `docker-compose.production.yml`:

```env
# Database Configuration
DB_USER=polybucket
DB_PASSWORD=your-secure-database-password

# Admin User Configuration
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@yourdomain.com
ADMIN_PASSWORD=your-secure-admin-password

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters-long
JWT_ISSUER=polybucket-api
JWT_AUDIENCE=polybucket-client

# Frontend Configuration
FRONTEND_URL=https://yourdomain.com

# CORS Configuration (comma-separated list)
CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com

# Storage Configuration
STORAGE_PROVIDER=MinIO
STORAGE_ENDPOINT=minio
STORAGE_EXTERNAL_ENDPOINT=yourdomain.com
STORAGE_PORT=9000
STORAGE_EXTERNAL_PORT=9000
STORAGE_USE_SSL=false
STORAGE_EXTERNAL_USE_SSL=true
STORAGE_BUCKET_NAME=polybucket-uploads
STORAGE_ACCESS_KEY=your-minio-access-key
STORAGE_SECRET_KEY=your-minio-secret-key
STORAGE_REGION=us-east-1

# Frontend Proxy Configuration
API_URL=http://api:11666
MINIO_URL=http://minio:9000
```

## Reverse Proxy Configuration

### Using Nginx as Reverse Proxy

Example Nginx configuration:

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /path/to/ssl/cert.pem;
    ssl_certificate_key /path/to/ssl/key.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Important**: Set `CORS_ALLOWED_ORIGINS` to match your domain (e.g., `https://yourdomain.com`).

## Deployment Steps

### Option 1: Using GitHub Actions (Recommended)

1. **Push to GitHub**:
   - Push your changes to the main/master branch
   - GitHub Actions will automatically build and push images to GitHub Container Registry

2. **On Server - Pull Images**:
   ```bash
   # Login to GitHub Container Registry
   echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_USERNAME --password-stdin
   
   # Pull latest images
   docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest
   docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest
   
   # Tag for local use
   docker tag ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest polybucket-api:latest
   docker tag ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest polybucket-frontend:latest
   ```

3. **Create Environment File**:
   ```bash
   # Create .env file with your configuration
   nano .env
   ```

4. **Start Services**:
   ```bash
   docker-compose -f docker-compose.production.yml up -d
   ```

5. **Check Logs**:
   ```bash
   docker-compose -f docker-compose.production.yml logs -f
   ```

### Option 2: Manual Build and Transfer

1. **Build Images Locally**:
   ```bash
   docker build -f Dockerfile.backend -t polybucket-api:latest --target runtime .
   docker build -f Dockerfile.frontend -t polybucket-frontend:latest --target production .
   ```

2. **Export Images**:
   ```bash
   docker save polybucket-api:latest | gzip > polybucket-api-latest.tar.gz
   docker save polybucket-frontend:latest | gzip > polybucket-frontend-latest.tar.gz
   ```

3. **Transfer to Server**:
   ```bash
   scp polybucket-api-latest.tar.gz user@server:/path/to/deploy/
   scp polybucket-frontend-latest.tar.gz user@server:/path/to/deploy/
   ```

4. **On Server - Load Images**:
   ```bash
   gunzip -c polybucket-api-latest.tar.gz | docker load
   gunzip -c polybucket-frontend-latest.tar.gz | docker load
   ```

5. **Create Environment File**:
   ```bash
   # Create .env file with your configuration
   nano .env
   ```

6. **Start Services**:
   ```bash
   docker-compose -f docker-compose.production.yml up -d
   ```

7. **Check Logs**:
   ```bash
   docker-compose -f docker-compose.production.yml logs -f
   ```

## Frontend Configuration

The frontend is built with environment variables at build time. For production deployment with a reverse proxy:

1. **Build with Production API URL**:
   ```bash
   VITE_API_URL=https://api.yourdomain.com npm run build
   ```

2. **Or use the nginx proxy** (recommended):
   - The frontend nginx container proxies `/api` requests to the backend
   - Set `API_URL` environment variable in docker-compose to point to your backend service
   - The frontend will use relative paths for API calls

## Health Checks

- Backend health check: `http://localhost:11666/health`
- Frontend: Serves static files on port 80
- Database: PostgreSQL health check via `pg_isready`
- MinIO: Health check on port 9000

## Troubleshooting

### CORS Errors

If you see CORS errors:
1. Ensure `CORS_ALLOWED_ORIGINS` includes your frontend domain
2. Check that the domain matches exactly (including protocol: `https://`)
3. Verify the backend is running in Production mode

### Database Connection Issues

1. Verify database credentials in `.env`
2. Check network connectivity between containers
3. Ensure database is healthy: `docker-compose ps`

### Storage Connection Issues

1. Verify MinIO credentials
2. Check `STORAGE_EXTERNAL_ENDPOINT` is accessible from the frontend
3. Ensure MinIO bucket exists and is accessible

### Frontend Can't Reach API

1. Verify `API_URL` environment variable in frontend container
2. Check nginx logs: `docker logs polybucket-frontend`
3. Ensure reverse proxy is configured correctly

## Security Considerations

1. **Never commit `.env` files** - Add to `.gitignore`
2. **Use strong passwords** - Generate secure passwords for database, admin, and MinIO
3. **Use HTTPS** - Always use SSL/TLS in production
4. **Rotate JWT secrets** - Regularly rotate JWT secrets
5. **Firewall rules** - Only expose necessary ports
6. **Backup database** - Regularly backup PostgreSQL data

## Updating the Application

1. Build new images locally
2. Export and transfer to server
3. Load new images
4. Restart services:
   ```bash
   docker-compose -f docker-compose.production.yml down
   docker-compose -f docker-compose.production.yml up -d
   ```

## Backup and Restore

### Backup Database

```bash
docker exec polybucket-db pg_dump -U polybucket polybucket > backup.sql
```

### Restore Database

```bash
docker exec -i polybucket-db psql -U polybucket polybucket < backup.sql
```

### Backup MinIO Data

```bash
docker exec polybucket-minio mc mirror /data /backup
```

