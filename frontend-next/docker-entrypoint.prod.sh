#!/bin/sh
set -e

API_URL="${NEXT_PUBLIC_API_URL:-http://localhost:11666}"
ESCAPED_API_URL=$(printf '%s\n' "$API_URL" | sed 's/[\/&]/\\&/g')

if [ -d "/app/.next" ]; then
  find /app/.next -type f \( -name "*.js" -o -name "*.json" -o -name "*.map" \) -exec sed -i "s|__NEXT_PUBLIC_API_URL__|$ESCAPED_API_URL|g" {} +
fi

exec "$@"
