#!/bin/bash

# Stop and remove the pod (this will stop all containers in the pod)
podman pod stop polybucket-dev
podman pod rm polybucket-dev

# Remove the volumes (optional, comment out if you want to keep the data)
podman volume rm polybucket-postgres-data polybucket-minio-data

echo "Development containers stopped and cleaned up" 