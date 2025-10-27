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
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001',
    NEXT_PUBLIC_POLYBUCKET_URL: process.env.NEXT_PUBLIC_POLYBUCKET_URL || 'http://localhost:3000',
  },
}

module.exports = nextConfig
