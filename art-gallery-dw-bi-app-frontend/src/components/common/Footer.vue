<template>
  <!--
    Footer.vue - Application Footer Component
    Art Gallery Management System
  -->
  <footer class="bg-gray-900 text-white mt-auto">
    <!-- Main Footer Content -->
    <div class="container mx-auto px-4 lg:px-8 py-12">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        
        <!-- Brand Column -->
        <div class="lg:col-span-1">
          <div class="flex items-center space-x-3 mb-4">
            <!-- Gallery Icon -->
            <div class="w-10 h-10 bg-primary-500 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <span class="text-xl font-display font-bold">Art Gallery</span>
          </div>
          <p class="text-gray-400 text-sm leading-relaxed mb-4">
            {{ description }}
          </p>
          
          <!-- Social Links -->
          <div class="flex space-x-4">
            <a 
              v-for="social in socialLinks" 
              :key="social.name"
              :href="social.url"
              :title="social.name"
              class="w-8 h-8 bg-gray-800 rounded-full flex items-center justify-center hover:bg-primary-600 transition-colors"
              target="_blank"
              rel="noopener noreferrer"
            >
              <span v-html="social.icon"></span>
            </a>
          </div>
        </div>

        <!-- Quick Links -->
        <div>
          <h3 class="text-lg font-semibold mb-4">Quick Links</h3>
          <ul class="space-y-2">
            <li v-for="link in quickLinks" :key="link.name">
              <router-link 
                :to="link.path"
                class="text-gray-400 hover:text-white transition-colors text-sm"
              >
                {{ link.name }}
              </router-link>
            </li>
          </ul>
        </div>

        <!-- Resources -->
        <div>
          <h3 class="text-lg font-semibold mb-4">Resources</h3>
          <ul class="space-y-2">
            <li v-for="link in resourceLinks" :key="link.name">
              <router-link 
                :to="link.path"
                class="text-gray-400 hover:text-white transition-colors text-sm"
              >
                {{ link.name }}
              </router-link>
            </li>
          </ul>
        </div>

        <!-- Contact / DW-BI Info -->
        <div>
          <h3 class="text-lg font-semibold mb-4">Project Info</h3>
          <ul class="space-y-3 text-sm text-gray-400">
            <li class="flex items-start space-x-2">
              <svg class="w-5 h-5 text-primary-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Vue.js 3 Master's Project</span>
            </li>
            <li class="flex items-start space-x-2">
              <svg class="w-5 h-5 text-primary-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>DW/BI Master's Project</span>
            </li>
            <li class="flex items-start space-x-2">
              <svg class="w-5 h-5 text-secondary-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
              </svg>
              <span>Options API Pattern</span>
            </li>
          </ul>
          
          <!-- Slot for custom content -->
          <slot name="extra-info"></slot>
        </div>
      </div>
    </div>

    <!-- Bottom Bar -->
    <div class="border-t border-gray-800">
      <div class="container mx-auto px-4 lg:px-8 py-4">
        <div class="flex flex-col md:flex-row items-center justify-between text-sm text-gray-400">
          <p>
            &copy; {{ currentYear }} Art Gallery Management System. All rights reserved.
          </p>
          <div class="flex space-x-6 mt-2 md:mt-0">
            <a href="#" class="hover:text-white transition-colors">Privacy Policy</a>
            <a href="#" class="hover:text-white transition-colors">Terms of Service</a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<script>
/**
 * Footer Component
 */
export default {
  // Component name
  name: 'FooterComponent',

  /**
   * data() - Reactive state
   * Contains navigation link arrays
   */
  data() {
    return {
      // Application description
      description: 'A comprehensive management system for art galleries, featuring artwork inventory, exhibition management, and business intelligence reporting.',

      // Quick links for navigation - demonstrates v-for iteration
      quickLinks: [
        { name: 'Dashboard', path: '/' },
        { name: 'Artworks', path: '/artworks' },
        { name: 'Exhibitions', path: '/exhibitions' },
        { name: 'Add Artwork', path: '/artworks/new' },
        { name: 'Add Exhibition', path: '/exhibitions/new' }
      ],

      // Resource links
      resourceLinks: [
        { name: 'ETL Management', path: '/etl' },
        { name: 'BI Reports', path: '/reports' },
        { name: 'Documentation', path: '#' },
        { name: 'API Reference', path: '#' }
      ],

      // Social media links with SVG icons
      socialLinks: [
        {
          name: 'GitHub',
          url: 'https://github.com',
          icon: '<svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>'
        },
        {
          name: 'Twitter',
          url: 'https://twitter.com',
          icon: '<svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M23.953 4.57a10 10 0 01-2.825.775 4.958 4.958 0 002.163-2.723c-.951.555-2.005.959-3.127 1.184a4.92 4.92 0 00-8.384 4.482C7.69 8.095 4.067 6.13 1.64 3.162a4.822 4.822 0 00-.666 2.475c0 1.71.87 3.213 2.188 4.096a4.904 4.904 0 01-2.228-.616v.06a4.923 4.923 0 003.946 4.827 4.996 4.996 0 01-2.212.085 4.936 4.936 0 004.604 3.417 9.867 9.867 0 01-6.102 2.105c-.39 0-.779-.023-1.17-.067a13.995 13.995 0 007.557 2.209c9.053 0 13.998-7.496 13.998-13.985 0-.21 0-.42-.015-.63A9.935 9.935 0 0024 4.59z"/></svg>'
        },
        {
          name: 'LinkedIn',
          url: 'https://linkedin.com',
          icon: '<svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></svg>'
        }
      ]
    };
  },

  /**
   * computed - Derived properties
   * Recalculate automatically when dependencies change
   */
  computed: {
    /**
     * Get current year dynamically
     * @returns {number} Current year
     */
    currentYear() {
      return new Date().getFullYear();
    }
  }
};
</script>

<style scoped>
/* Footer-specific styles */
footer {
  /* Ensure footer sticks to bottom */
  flex-shrink: 0;
}
</style>
