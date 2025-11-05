# Environment Variables in Marketplace Frontend

## How Next.js Loads Environment Variables

Next.js automatically loads environment variables from `.env` files in the following order (later files override earlier ones):

1. `.env` - Loaded in all environments
2. `.env.local` - Loaded in all environments (should be gitignored)
3. `.env.development` - Loaded in development (`npm run dev`)
4. `.env.production` - Loaded in production (`npm run build`)
5. `.env.development.local` - Development overrides
6. `.env.production.local` - Production overrides

## Public Variables (Browser Accessible)

Variables with the `NEXT_PUBLIC_` prefix are:
- Exposed to the browser (embedded in the JavaScript bundle)
- Accessible via `process.env.NEXT_PUBLIC_*` in client-side code
- Replaced at build time with their values
- **Require a server restart** after changing `.env` files

## Current Environment Variables

### `NEXT_PUBLIC_API_URL`
- **Purpose**: URL of the marketplace backend API
- **Default**: `http://localhost:10120`
- **Usage**: All API client requests use this base URL
- **Location**: `lib/api.ts`, `context/AuthContext.tsx`, `app/auth/callback/page.tsx`

### `NEXT_PUBLIC_POLYBUCKET_URL`
- **Purpose**: URL of the main PolyBucket application
- **Default**: `http://localhost:3000`
- **Usage**: Links and redirects to main app

### `NEXT_PUBLIC_GITHUB_CLIENT_ID`
- **Purpose**: GitHub OAuth Application Client ID
- **Default**: Empty (must be configured)
- **Usage**: Constructing GitHub OAuth authorization URL
- **Location**: `context/AuthContext.tsx` (login function)

### `NEXT_PUBLIC_GITHUB_REDIRECT_URI`
- **Purpose**: GitHub OAuth callback URL
- **Default**: `http://localhost:10110/auth/callback`
- **Usage**: Constructing GitHub OAuth authorization URL
- **Location**: `context/AuthContext.tsx` (login function)

## Setup Instructions

1. **Copy the example file:**
   ```bash
   cp .env.example .env.local
   ```

2. **Edit `.env.local` with your values:**
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:10120
   NEXT_PUBLIC_GITHUB_CLIENT_ID=your-actual-client-id
   NEXT_PUBLIC_GITHUB_REDIRECT_URI=http://localhost:10110/auth/callback
   ```

3. **Restart the dev server** after making changes:
   ```bash
   npm run dev
   ```

## Important Notes

- ✅ All API calls now use `NEXT_PUBLIC_API_URL` consistently
- ✅ Environment variables are accessed via `process.env.NEXT_PUBLIC_*`
- ✅ Fallback defaults are provided in `next.config.js`
- ✅ `.env.local` is gitignored (safe for secrets)
- ⚠️ Changing `.env` files requires restarting the dev server
- ⚠️ Public variables are exposed in the browser bundle

## Testing Environment Variables

To verify your environment variables are loaded:

1. Check the browser console - variables are embedded in the bundle
2. Add a temporary `console.log(process.env.NEXT_PUBLIC_API_URL)` in a component
3. Verify the API calls use the correct URL in Network tab
