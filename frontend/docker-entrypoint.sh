#!/bin/sh
set -e

# Set defaults if not provided
API_URL=${API_URL:-http://localhost:11666}
SEAWEEDFS_S3_URL=${SEAWEEDFS_S3_URL:-http://seaweedfs:8333}

# Substitute environment variables in nginx config template
envsubst '${API_URL} ${SEAWEEDFS_S3_URL}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

# Start nginx
exec nginx -g 'daemon off;'
