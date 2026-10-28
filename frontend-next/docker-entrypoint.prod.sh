#!/bin/sh
set -e

API_URL="${POLYBUCKET_API_URL:-${NEXT_PUBLIC_API_URL:-http://localhost:11666}}"

case "$API_URL" in
  http://*|https://*) ;;
  *)
    echo "POLYBUCKET_API_URL / NEXT_PUBLIC_API_URL must be http(s) URL" >&2
    exit 1
    ;;
esac

ESCAPED_API_URL=$(printf '%s\n' "$API_URL" | sed 's/[\/&]/\\&/g')

if [ -z "$SESSION_SECRET" ] || [ "${#SESSION_SECRET}" -lt 32 ]; then
  echo "SESSION_SECRET must be at least 32 characters in production" >&2
  exit 1
fi

if [ -d "/app/.next" ]; then
  find /app/.next -type f \( -name "*.js" -o -name "*.json" -o -name "*.map" \) -exec sed -i "s|__NEXT_PUBLIC_API_URL__|$ESCAPED_API_URL|g" {} +
  sh /app/scripts/validate-next-artifacts.sh /app/.next
fi

exec "$@"
