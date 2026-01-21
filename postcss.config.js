/**
 * PostCSS Configuration for Art Gallery DW/BI Application
 * 
 * PostCSS is used to process CSS files with plugins like TailwindCSS and Autoprefixer.
 * This configuration is automatically picked up by Vite during build.
 */

export default {
  plugins: {
    // TailwindCSS plugin - processes @tailwind directives
    tailwindcss: {},
    
    // Autoprefixer - adds vendor prefixes for cross-browser compatibility
    autoprefixer: {},
  },
};
