import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  // VITE_PROXY_TARGET can be set to override the proxy target (useful in Docker)
  // Otherwise, use VITE_API_URL or default to localhost
  const apiUrl = env.VITE_PROXY_TARGET || env.VITE_API_URL || 'http://localhost:11666';
  return {
    plugins: [react()],
    server: {
      port: 3000,
      host: '0.0.0.0', // Listen on all interfaces
      proxy: {
        '/api': {
          target: apiUrl,
          changeOrigin: true,
          secure: false,
        }
      }
    },
    build: {
      outDir: 'build',
      rollupOptions: {
        output: {
          manualChunks: {
            // Vendor chunks
            'react-vendor': ['react', 'react-dom', 'react-router-dom'],
            'redux-vendor': ['@reduxjs/toolkit', 'react-redux', 'redux-persist'],
            'mui-vendor': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
            'three-vendor': ['three', '@react-three/fiber', '@react-three/drei'],
            // Large API client gets its own chunk
            'api-client': ['./src/api/client.ts'],
            // Feature chunks
            'admin': ['./src/acp'],
            'moderation': ['./src/mcp'],
            'models': ['./src/models'],
            'collections': ['./src/collections'],
            'ucp': ['./src/ucp'],
          },
          chunkFileNames: 'assets/js/[name]-[hash].js',
          entryFileNames: 'assets/js/[name]-[hash].js',
          assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
        },
      },
      chunkSizeWarningLimit: 1000,
      minify: 'esbuild',
      sourcemap: false,
      target: 'esnext',
    },
    optimizeDeps: {
      include: [
        'react',
        'react-dom',
        'react-router-dom',
        '@reduxjs/toolkit',
        'react-redux',
      ],
      exclude: ['@react-three/fiber', '@react-three/drei'],
    },
  }
}) 