/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: 'http',
        hostname: 'localhost',
        port: '',
        pathname: '/**',
      },
      {
        protocol: 'https',
        hostname: 'marketplace.polybucket.com',
        port: '',
        pathname: '/**',
      },
    ],
    unoptimized: true,
  },
  // Environment variables with NEXT_PUBLIC_ prefix are automatically loaded from:
  // - .env.local (loaded in all environments, should be gitignored)
  // - .env.development (loaded in development)
  // - .env.production (loaded in production)
  // - .env (loaded in all environments)
  // The env section below provides fallback defaults if variables aren't set
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:10120',
    NEXT_PUBLIC_POLYBUCKET_URL: process.env.NEXT_PUBLIC_POLYBUCKET_URL || 'http://localhost:3000',
    NEXT_PUBLIC_GITHUB_CLIENT_ID: process.env.NEXT_PUBLIC_GITHUB_CLIENT_ID || '',
    NEXT_PUBLIC_GITHUB_REDIRECT_URI: process.env.NEXT_PUBLIC_GITHUB_REDIRECT_URI || 'http://localhost:10110/auth/callback',
  },
}

module.exports = nextConfig
