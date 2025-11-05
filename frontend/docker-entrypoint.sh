#!/bin/sh
set -e

# Set defaults if not provided
API_URL=${API_URL:-http://localhost:11666}
MINIO_URL=${MINIO_URL:-http://minio:9000}

# Substitute environment variables in nginx config template
envsubst '${API_URL} ${MINIO_URL}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

# Start nginx
exec nginx -g 'daemon off;'

