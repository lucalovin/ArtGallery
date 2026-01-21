/**
 * TailwindCSS Configuration for Art Gallery DW/BI Application
 * 
 * This configuration extends TailwindCSS with custom colors, fonts, and utilities
 * specifically designed for the Art Gallery Management System UI.
 * 
 * Tailwind provides utility-first CSS classes for responsive design.
 */

/** @type {import('tailwindcss').Config} */
export default {
  // Content paths - Tailwind will scan these files for class usage
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],

  // Theme customization
  theme: {
    extend: {
      // Custom color palette for Art Gallery branding
      colors: {
        // Primary colors - Deep gallery blue
        primary: {
          50: '#f0f5ff',
          100: '#e0ebff',
          200: '#c7d9ff',
          300: '#a3bfff',
          400: '#7a9dff',
          500: '#5474f7',
          600: '#3d4eed',
          700: '#2f3bd4',
          800: '#2a34ab',
          900: '#283286',
          950: '#1a1f52',
        },
        // Secondary colors - Gallery gold/amber for accents
        secondary: {
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24',
          500: '#f59e0b',
          600: '#d97706',
          700: '#b45309',
          800: '#92400e',
          900: '#78350f',
          950: '#451a03',
        },
        // Gallery-specific colors
        gallery: {
          cream: '#faf7f2',
          charcoal: '#2d2d2d',
          burgundy: '#722f37',
          forest: '#2d5a27',
          slate: '#4a5568',
        },
        // Status colors for ETL and validation
        status: {
          success: '#10b981',
          warning: '#f59e0b',
          error: '#ef4444',
          info: '#3b82f6',
        }
      },

      // Custom font families
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        serif: ['Playfair Display', 'Georgia', 'serif'],
        display: ['Montserrat', 'sans-serif'],
      },

      // Custom spacing for gallery layouts
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
      },

      // Animation for loading states and transitions
      animation: {
        'fade-in': 'fadeIn 0.3s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'spin-slow': 'spin 2s linear infinite',
      },

      // Keyframes for custom animations
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
      },

      // Box shadow for cards and elevated elements
      boxShadow: {
        'gallery': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'gallery-lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
        'gallery-xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
      },

      // Border radius for card-style components
      borderRadius: {
        'gallery': '0.75rem',
      },

      // Grid template for dashboard layouts
      gridTemplateColumns: {
        'dashboard': 'repeat(auto-fit, minmax(280px, 1fr))',
        'reports': 'repeat(auto-fit, minmax(350px, 1fr))',
      },

      // Z-index for modals and overlays
      zIndex: {
        '60': '60',
        '70': '70',
        '80': '80',
        '90': '90',
        '100': '100',
      },
    },
  },

  // Tailwind plugins
  plugins: [
    // Add custom utilities if needed
  ],

  // Safelist classes that might be dynamically generated
  safelist: [
    // Status colors
    'bg-status-success',
    'bg-status-warning',
    'bg-status-error',
    'bg-status-info',
    'text-status-success',
    'text-status-warning',
    'text-status-error',
    'text-status-info',
    // Chart colors
    'bg-primary-500',
    'bg-secondary-500',
    'bg-gallery-burgundy',
    'bg-gallery-forest',
  ],
};
