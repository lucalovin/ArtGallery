/**
 * Vite Configuration for Art Gallery DW/BI Application
 * 
 * This configuration file sets up Vite as the build tool for the Vue.js 3 application.
 * Vite provides fast HMR (Hot Module Replacement) and optimized production builds.
 */

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'node:url';

// https://vitejs.dev/config/
export default defineConfig({
  // Vue plugin for Vite - enables .vue single file component support
  plugins: [
    vue()
  ],

  // Path resolution configuration
  resolve: {
    alias: {
      // '@' alias points to src folder for cleaner imports
      // Usage: import Component from '@/components/Component.vue'
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },

  // Development server configuration
  server: {
    port: 5173,
    // Enable CORS for development
    cors: true,
    // Proxy API requests to json-server during development
    proxy: {
      '/api': {
        target: 'http://localhost:3000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    }
  },

  // Build configuration for production
  build: {
    // Output directory
    outDir: 'dist',
    // Generate source maps for debugging
    sourcemap: true,
    // Rollup options for code splitting
    rollupOptions: {
      output: {
        // Manual chunk splitting for better caching
        manualChunks: {
          // Vue core libraries in separate chunk
          'vue-vendor': ['vue', 'vue-router', 'vuex'],
          // Chart libraries in separate chunk
          'chart-vendor': ['chart.js', 'vue-chartjs'],
          // Form validation in separate chunk
          'form-vendor': ['vee-validate', 'yup']
        }
      }
    }
  },

  // Environment variable prefix (accessed via import.meta.env.VITE_*)
  envPrefix: 'VITE_',

  // CSS configuration
  css: {
    // PostCSS configuration is loaded from postcss.config.js
    devSourcemap: true
  }
});
