# Marketplace Environment Variables

## Single .env File Setup

The marketplace uses a **single `.env` file** at the marketplace root (`marketplace/.env`) that contains all environment variables for both frontend and backend.

## Quick Start

1. **Copy the example file:**
   ```bash
   cd marketplace
   cp .env.example .env
   ```

2. **Edit `.env` with your actual values:**
   ```bash
   # Required: GitHub OAuth credentials
   NEXT_PUBLIC_GITHUB_CLIENT_ID=your-actual-client-id
   GitHub__ClientSecret=your-actual-client-secret
   
   # Recommended: Strong JWT secret
   Jwt__SecretKey=$(openssl rand -base64 32)
   ```

3. **Use with Docker Compose:**
   ```bash
   # The .env file is automatically loaded by docker-compose
   docker-compose -f docker-compose.marketplace.yml up
   ```

## File Structure

```
marketplace/
├── .env              # Single source of truth (gitignored)
├── .env.example      # Template with all variables documented
├── frontend/
│   └── .env.example  # Reference to root .env file
└── backend/
    └── .env.example  # Reference to root .env file
```

## Local Development

### For Frontend (Next.js)

Next.js automatically loads from `.env.local` in the frontend directory. You can either:

**Option 1:** Copy from root (recommended)
```bash
cd marketplace/frontend
cp ../.env .env.local
# Next.js will only read NEXT_PUBLIC_* variables
```

**Option 2:** Use a symlink
```bash
cd marketplace/frontend
ln -s ../.env .env.local
```

### For Backend (.NET)

The backend reads from:
1. `appsettings.json`
2. `appsettings.Development.json`
3. Environment variables (from system or .env file)

For local development, you can:
- Set environment variables manually
- Use the root `.env` file (via system environment or tooling)
- Use `appsettings.Development.json` for local overrides

## Docker Compose

Both services automatically load from `marketplace/.env` via the `env_file` directive:

```yaml
services:
  marketplace-api:
    env_file:
      - .env
    environment:
      - # Variables can reference .env values
```

## Variables Reference

See `marketplace/.env.example` for a complete list with descriptions.

### Key Variables

- **Frontend:** `NEXT_PUBLIC_*` prefix (exposed to browser)
- **Backend:** No prefix (server-side only)
- **Shared:** GitHub OAuth uses same Client ID for both

## Production

For production deployments:
1. Create `.env.production` with production values
2. Ensure `.env` is in `.gitignore`
3. Use secrets management (Docker secrets, Kubernetes secrets, etc.)
4. Never commit `.env` files with real credentials
