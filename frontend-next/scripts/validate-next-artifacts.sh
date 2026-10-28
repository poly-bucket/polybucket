#!/bin/sh
set -e

ROOT="${1:-.}"

if grep -r --include='*.js' --include='*.json' --include='*.map' -l '__NEXT_PUBLIC_API_URL__' "$ROOT" 2>/dev/null | grep -q .; then
  echo "Placeholder __NEXT_PUBLIC_API_URL__ still present under $ROOT" >&2
  exit 1
fi

echo "No API URL placeholders found."
