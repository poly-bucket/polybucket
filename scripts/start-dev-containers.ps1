# Detect container runtime
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $RUNTIME = "docker"
    Write-Host "Using Docker as container runtime"
} else {
    Write-Host "Error: Docker is not installed"
    exit 1
}

# Create a network for our services
docker network create polybucket-dev 2>$null

# Start PostgreSQL
docker run -d --network polybucket-dev `
    --name polybucket-postgres `
    -e POSTGRES_USER=polybucket `
    -e POSTGRES_PASSWORD=polybucket `
    -e POSTGRES_DB=polybucket `
    -v polybucket-postgres-data:/var/lib/postgresql/data `
    -p 5432:5432 `
    docker.io/library/postgres:16

# Start MinIO
docker run -d --network polybucket-dev `
    --name polybucket-minio `
    -e MINIO_ROOT_USER=minioadmin `
    -e MINIO_ROOT_PASSWORD=minioadmin `
    -v polybucket-minio-data:/data `
    -p 9000:9000 `
    -p 9001:9001 `
    docker.io/minio/minio server /data --console-address ":9001"

# Print status
Write-Host "Development containers started:"
docker ps

Write-Host ""
Write-Host "PostgreSQL is available at localhost:5432"
Write-Host "MinIO is available at:"
Write-Host "  - API: http://localhost:9000"
Write-Host "  - Console: http://localhost:9001"
Write-Host "  - Access Key: minioadmin"
Write-Host "  - Secret Key: minioadmin" 