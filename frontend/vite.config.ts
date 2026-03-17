import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  // VITE_PROXY_TARGET can be set to override the proxy target (useful in Docker)
  // Otherwise, use VITE_API_URL or default to localhost
  const apiUrl = env.VITE_PROXY_TARGET || env.VITE_API_URL || 'http://localhost:11666';
  return {
    plugins: [react()],
    server: {
      port: 12666,
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
          manualChunks(id) {
            if (id.includes('node_modules')) {
              if (id.includes('react') || id.includes('react-dom') || id.includes('react-router-dom')) {
                return 'react-vendor';
              }
              if (id.includes('@reduxjs/toolkit') || id.includes('react-redux') || id.includes('redux-persist')) {
                return 'redux-vendor';
              }
              if (id.includes('@mui/material') || id.includes('@mui/icons-material') || id.includes('@emotion/react') || id.includes('@emotion/styled')) {
                return 'mui-vendor';
              }
              if (id.includes('three') || id.includes('@react-three/fiber') || id.includes('@react-three/drei')) {
                return 'three-vendor';
              }
              return 'vendor';
            }
            if (id.includes('/src/api/client.ts')) {
              return 'api-client';
            }
            if (id.includes('/src/acp/')) {
              return 'admin';
            }
            if (id.includes('/src/mcp/')) {
              return 'moderation';
            }
            if (id.includes('/src/models/')) {
              return 'models';
            }
            if (id.includes('/src/collections/')) {
              return 'collections';
            }
            if (id.includes('/src/ucp/')) {
              return 'ucp';
            }
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
        './src/utils/use-sync-external-store-shim.ts',
      ],
      exclude: [
        '@react-three/fiber', 
        '@react-three/drei',
      ],
      esbuildOptions: {
        target: 'esnext',
      },
    },
    resolve: {
      dedupe: ['react', 'react-dom', 'react-redux'],
      alias: {
        'use-sync-external-store/with-selector': resolve(__dirname, 'src/utils/use-sync-external-store-shim.ts'),
        'use-sync-external-store/with-selector.js': resolve(__dirname, 'src/utils/use-sync-external-store-shim.ts'),
        'use-sync-external-store/shim/with-selector': resolve(__dirname, 'src/utils/use-sync-external-store-shim.ts'),
        'use-sync-external-store/shim/with-selector.js': resolve(__dirname, 'src/utils/use-sync-external-store-shim.ts'),
      },
    },
  } as any
}) 