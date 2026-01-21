<template>
  <!--
    App.vue - Root Component for Art Gallery Management System
    
    This component serves as the main layout wrapper containing:
    - Navigation menu (top)
    - Main content area (router-view)
    - Footer (bottom)
  -->
  <div id="app" class="min-h-screen bg-gray-50 flex flex-col">
    <!-- Global Loading Overlay -->
    <div 
      v-if="isGlobalLoading" 
      class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center"
    >
      <div class="bg-white rounded-lg p-6 shadow-xl flex flex-col items-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="mt-4 text-gray-600">{{ loadingMessage }}</p>
      </div>
    </div>

    <!-- Toast Notification Container -->
    <div class="fixed top-4 right-4 z-50 space-y-2">
      <transition-group name="toast">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          :class="[
            'px-4 py-3 rounded-lg shadow-lg flex items-center space-x-3 min-w-[300px]',
            toast.type === 'success' ? 'bg-green-500 text-white' : '',
            toast.type === 'error' ? 'bg-red-500 text-white' : '',
            toast.type === 'warning' ? 'bg-yellow-500 text-white' : '',
            toast.type === 'info' ? 'bg-blue-500 text-white' : ''
          ]"
        >
          <!-- Toast Icon -->
          <span class="flex-shrink-0">
            <svg v-if="toast.type === 'success'" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
            </svg>
            <svg v-else-if="toast.type === 'error'" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
            </svg>
            <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/>
            </svg>
          </span>
          <span class="flex-1">{{ toast.message }}</span>
          <button 
            @click="removeToast(toast.id)" 
            class="flex-shrink-0 hover:opacity-75"
          >
            <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"/>
            </svg>
          </button>
        </div>
      </transition-group>
    </div>

    <!-- Navigation Menu Component -->
    <navigation-menu 
      :is-mobile-menu-open="isMobileMenuOpen"
      @toggle-mobile-menu="toggleMobileMenu"
    />

    <!-- Main Content Area -->
    <main class="flex-1 container mx-auto px-4 py-8 md:px-6 lg:px-8">
      <!-- Router View -->
      <router-view
        @show-toast="showToast"
        @show-loading="showLoading"
        @hide-loading="hideLoading"
      />
    </main>

    <!-- Footer Component -->
    <footer-component />
  </div>
</template>

<script>
/**
 * App.vue - Root Application Component
 */

// Import child components
import NavigationMenu from './components/common/NavigationMenu.vue';
import FooterComponent from './components/common/Footer.vue';

export default {
  // Component name for debugging
  name: 'App',

  // Register child components
  components: {
    NavigationMenu,
    FooterComponent
  },

  /**
   * data() - OPTIONS API reactive state
   * Returns an object containing all reactive properties
   */
  data() {
    return {
      // Global loading state
      isGlobalLoading: false,
      loadingMessage: 'Loading...',

      // Toast notifications array
      toasts: [],
      toastIdCounter: 0,

      // Mobile menu state
      isMobileMenuOpen: false,

      // App initialization state
      isInitialized: false,

      // Last route for analytics/debugging
      lastRoute: null
    };
  },

  /**
   * computed - OPTIONS API derived properties
   * Calculated values that update when dependencies change
   */
  computed: {
    /**
     * Check if user is on a form page (for unsaved changes warning)
     */
    isOnFormPage() {
      const formRoutes = ['/artworks/new', '/exhibitions/new'];
      const currentPath = this.$route?.path || '';
      return formRoutes.some(route => currentPath.includes(route)) || 
             currentPath.includes('/edit');
    },

    /**
     * Get current route name for analytics
     */
    currentRouteName() {
      return this.$route?.name || 'Unknown';
    },

    /**
     * Count of active toasts
     */
    activeToastCount() {
      return this.toasts.length;
    }
  },

  /**
   * watch - OPTIONS API reactive watchers
   * React to data changes
   */
  watch: {
    /**
     * Watch route changes for analytics and mobile menu
     */
    '$route'(to, from) {
      // Close mobile menu on navigation
      this.isMobileMenuOpen = false;
      
      // Store last route for back navigation
      this.lastRoute = from;

      // Log route change in development
      if (import.meta.env.DEV) {
        console.log(`Route: ${from?.path || 'initial'} → ${to.path}`);
      }
    },

    /**
     * Watch mobile menu state for body scroll lock
     */
    isMobileMenuOpen(isOpen) {
      if (isOpen) {
        document.body.classList.add('overflow-hidden');
      } else {
        document.body.classList.remove('overflow-hidden');
      }
    }
  },

  /**
   * methods - OPTIONS API component methods
   * All functions used in the component
   */
  methods: {
    /**
     * Toggle mobile menu visibility
     */
    toggleMobileMenu() {
      this.isMobileMenuOpen = !this.isMobileMenuOpen;
    },

    /**
     * Show global loading overlay
     * @param {string} message - Loading message to display
     */
    showLoading(message = 'Loading...') {
      this.loadingMessage = message;
      this.isGlobalLoading = true;
    },

    /**
     * Hide global loading overlay
     */
    hideLoading() {
      this.isGlobalLoading = false;
      this.loadingMessage = 'Loading...';
    },

    /**
     * Show toast notification
     * @param {Object} options - Toast options
     * @param {string} options.message - Toast message
     * @param {string} options.type - Toast type (success, error, warning, info)
     * @param {number} options.duration - Duration in ms (default: 5000)
     */
    showToast({ message, type = 'info', duration = 5000 }) {
      const id = ++this.toastIdCounter;
      
      this.toasts.push({
        id,
        message,
        type
      });

      // Auto-remove toast after duration
      if (duration > 0) {
        setTimeout(() => {
          this.removeToast(id);
        }, duration);
      }
    },

    /**
     * Remove a toast notification by ID
     * @param {number} id - Toast ID to remove
     */
    removeToast(id) {
      const index = this.toasts.findIndex(t => t.id === id);
      if (index !== -1) {
        this.toasts.splice(index, 1);
      }
    },

    /**
     * Clear all toast notifications
     */
    clearAllToasts() {
      this.toasts = [];
    },

    /**
     * Initialize application data from LocalStorage
     * Called in created() lifecycle hook
     */
    initializeApp() {
      // Load persisted data via Vuex actions
      this.$store.dispatch('artwork/loadFromLocalStorage');
      this.$store.dispatch('exhibition/loadFromLocalStorage');
      
      // Mark app as initialized
      this.isInitialized = true;
    },

    /**
     * Handle window resize for responsive adjustments
     */
    handleResize() {
      // Close mobile menu on larger screens
      if (window.innerWidth >= 768 && this.isMobileMenuOpen) {
        this.isMobileMenuOpen = false;
      }
    },

    /**
     * Handle before unload event (unsaved changes warning)
     */
    handleBeforeUnload(event) {
      if (this.isOnFormPage) {
        event.preventDefault();
        event.returnValue = '';
        return '';
      }
    }
  },

  /**
   * created() - OPTIONS API lifecycle hook
   * Called after instance is created, before mounting
   * Use for: data initialization, API calls, store dispatch
   */
  created() {
    // Initialize app data
    this.initializeApp();

    // Log app creation in development
    if (import.meta.env.DEV) {
      console.log('🎨 App component created');
    }
  },

  /**
   * mounted() - OPTIONS API lifecycle hook
   * Called after component is mounted to DOM
   * Use for: DOM manipulation, event listeners, third-party integrations
   */
  mounted() {
    // Add window resize listener
    window.addEventListener('resize', this.handleResize);

    // Add beforeunload listener for unsaved changes
    window.addEventListener('beforeunload', this.handleBeforeUnload);

    // Show welcome toast in development
    if (import.meta.env.DEV) {
      this.showToast({
        message: 'Welcome to Art Gallery Management System!',
        type: 'info',
        duration: 3000
      });
    }

    console.log('🎨 Art Gallery Management System mounted');
  },

  /**
   * beforeUnmount() - OPTIONS API lifecycle hook (Vue 3)
   * Called before component is unmounted
   * Use for: cleanup, remove event listeners
   * Note: In Vue 3, 'destroyed' is renamed to 'unmounted',
   *       and 'beforeDestroy' is renamed to 'beforeUnmount'
   */
  beforeUnmount() {
    // Remove event listeners
    window.removeEventListener('resize', this.handleResize);
    window.removeEventListener('beforeunload', this.handleBeforeUnload);

    // Clear any pending toasts
    this.clearAllToasts();

    console.log('🎨 App component cleanup complete');
  }
};
</script>

<style>
/**
 * Global styles for the application
 * Note: Most styling uses TailwindCSS utility classes
 * These are for transitions and global elements
 */

/* Page transition: fade */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Toast notification transition */
.toast-enter-active {
  transition: all 0.3s ease-out;
}

.toast-leave-active {
  transition: all 0.2s ease-in;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

/* Slide up animation for modals and dropdowns */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

/* Custom scrollbar styling */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: #f3f4f6;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: #9ca3af;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #6b7280;
}

/* Focus visible for accessibility */
*:focus-visible {
  outline: 2px solid #5474f7;
  outline-offset: 2px;
}

/* Print styles */
@media print {
  .no-print {
    display: none !important;
  }
}
</style>
