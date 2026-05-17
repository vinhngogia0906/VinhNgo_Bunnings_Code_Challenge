import { defineConfig } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] })
  ],
  server: {
    port: 5173,
    proxy: {
      // forward /api to the .NET API during dev so CORS is never an issue.
      '/api': {
        target: 'http://localhost:5182',
        changeOrigin: true,
      },
    },
  },
})
