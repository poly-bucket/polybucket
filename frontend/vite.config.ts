import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const apiUrl = env.VITE_API_URL || 'http://localhost:11666';
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
      outDir: 'build'
    }
  }
}) 