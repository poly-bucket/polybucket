#!/bin/sh
set -e
cd /app
if [ ! -d "node_modules/next" ]; then
  echo "Installing dependencies..."
  npm install --legacy-peer-deps
fi
exec node node_modules/next/dist/bin/next dev -H 0.0.0.0
