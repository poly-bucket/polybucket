#!/bin/sh
set -e
cd /app
deps_stamp="node_modules/.deps-stamp"
deps_hash="$(cat package.json package-lock.json 2>/dev/null | cksum | cut -d' ' -f1)"
if [ ! -d "node_modules/next" ] || [ ! -f "$deps_stamp" ] || [ "$(cat "$deps_stamp" 2>/dev/null)" != "$deps_hash" ]; then
  echo "Installing dependencies..."
  npm install --legacy-peer-deps
  echo "$deps_hash" > "$deps_stamp"
fi
exec node node_modules/next/dist/bin/next dev -H 0.0.0.0
