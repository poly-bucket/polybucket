import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: {
      port: 3000,
      host: '0.0.0.0', // Listen on all interfaces
      proxy: {
        '/api': {
          target: 'http://api:11666',
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