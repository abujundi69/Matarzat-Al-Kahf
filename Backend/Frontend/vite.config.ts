import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    host: '127.0.0.1',
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5212',
        changeOrigin: true,
        secure: false,
      },
      '/uploads': {
        target: 'http://127.0.0.1:5212',
        changeOrigin: true,
        secure: false,
      },
      '/sitemap.xml': {
        target: 'http://127.0.0.1:5212',
        changeOrigin: true,
      },
      '/robots.txt': {
        target: 'http://127.0.0.1:5212',
        changeOrigin: true,
      },
    },
  },
})
