#!/bin/bash

# Detect container runtime
if command -v podman &> /dev/null; then
    RUNTIME="podman"
    POD_CMD="podman pod"
    RUN_CMD="podman run"
elif command -v docker &> /dev/null; then
    RUNTIME="docker"
    # Docker doesn't have pods, so we'll use a network instead
    NETWORK_NAME="polybucket-dev"
    docker network create $NETWORK_NAME 2>/dev/null || true
else
    echo "Error: Neither Podman nor Docker is installed"
    exit 1
fi

echo "Using $RUNTIME as container runtime"

if [ "$RUNTIME" = "podman" ]; then
    # Create a pod for our services
    $POD_CMD create --name polybucket-dev -p 5432:5432 -p 9000:9000 -p 9001:9001
    POD_ARG="--pod polybucket-dev"
else
    POD_ARG="--network $NETWORK_NAME"
fi

# Start PostgreSQL
$RUN_CMD -d $POD_ARG \
  --name polybucket-postgres \
  -e POSTGRES_USER=polybucket \
  -e POSTGRES_PASSWORD=polybucket \
  -e POSTGRES_DB=polybucket \
  -v polybucket-postgres-data:/var/lib/postgresql/data \
  -p 5432:5432 \
  docker.io/library/postgres:16

# Start MinIO
$RUN_CMD -d $POD_ARG \
  --name polybucket-minio \
  -e MINIO_ROOT_USER=minioadmin \
  -e MINIO_ROOT_PASSWORD=minioadmin \
  -v polybucket-minio-data:/data \
  -p 9000:9000 \
  -p 9001:9001 \
  docker.io/minio/minio server /data --console-address ":9001"

# Print status
echo "Development containers started:"
if [ "$RUNTIME" = "podman" ]; then
    $POD_CMD ps
else
    docker ps
fi

echo ""
echo "PostgreSQL is available at localhost:5432"
echo "MinIO is available at:"
echo "  - API: http://localhost:9000"
echo "  - Console: http://localhost:9001"
echo "  - Access Key: minioadmin"
echo "  - Secret Key: minioadmin" 