import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Bind to all interfaces so the Vite dev server is reachable from
    // outside the Docker container (see docker-compose.yml).
    host: '0.0.0.0',
    port: 5173,
    strictPort: true,
  },
})
