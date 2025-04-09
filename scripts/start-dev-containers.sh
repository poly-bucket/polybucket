#!/bin/bash

# Create a pod for our services
podman pod create --name polybucket-dev -p 5432:5432 -p 9000:9000 -p 9001:9001

# Start PostgreSQL
podman run -d --pod polybucket-dev \
  --name polybucket-postgres \
  -e POSTGRES_USER=polybucket \
  -e POSTGRES_PASSWORD=polybucket \
  -e POSTGRES_DB=polybucket \
  -v polybucket-postgres-data:/var/lib/postgresql/data \
  docker.io/library/postgres:16

# Start MinIO
podman run -d --pod polybucket-dev \
  --name polybucket-minio \
  -e MINIO_ROOT_USER=minioadmin \
  -e MINIO_ROOT_PASSWORD=minioadmin \
  -v polybucket-minio-data:/data \
  docker.io/minio/minio server /data --console-address ":9001"

# Print status
echo "Development containers started:"
podman pod ps
echo ""
echo "PostgreSQL is available at localhost:5432"
echo "MinIO is available at:"
echo "  - API: http://localhost:9000"
echo "  - Console: http://localhost:9001"
echo "  - Access Key: minioadmin"
echo "  - Secret Key: minioadmin" 