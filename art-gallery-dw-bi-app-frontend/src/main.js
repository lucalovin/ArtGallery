/**
 * Main Entry Point for Art Gallery DW/BI Application
 * 
 * This file initializes the Vue.js 3 application with:
 * - Vue Router 4 for navigation
 * - Vuex 4 for state management
 * - API Services for backend communication
 * - Global styles (TailwindCSS)
 * 
 */

// Vue core imports
import { createApp } from 'vue';

// Root component
import App from './App.vue';

// Router configuration (Vue Router 4)
import router from './router';

// Vuex store configuration (Vuex 4)
import store from './store';

// API Services plugin
import { apiPlugin } from './api';

// Global styles with TailwindCSS
import './assets/styles/tailwind.css';

/**
 * Create Vue application instance
 * Vue 3 uses createApp() instead of new Vue() from Vue 2
 */
const app = createApp(App);

/**
 * Register Vue Router
 * Provides navigation capabilities throughout the application
 */
app.use(router);

/**
 * Register Vuex Store
 * Provides centralized state management with modules:
 * - artworkStore: Manages artwork data
 * - exhibitionStore: Manages exhibition data
 * - visitorStore: Manages visitor and review data
 * - staffStore: Manages staff activities
 * - loansStore: Manages loan and restoration data
 * - reportsStore: Manages BI report data
 */
app.use(store);

/**
 * Register API Services Plugin
 * Provides access to backend APIs via this.$api
 * - this.$api.etl - ETL operations
 * - this.$api.reports - Reports and KPIs
 * - this.$api.analytics - DW Analytics queries
 * - this.$api.artworks - Artwork CRUD
 * - this.$api.exhibitions - Exhibition CRUD
 * - this.$api.staff - Staff CRUD
 */
app.use(apiPlugin);

/**
 * Global Error Handler
 * Catches unhandled errors in components
 */
app.config.errorHandler = (err, instance, info) => {
  console.error('Global Error:', err);
  console.error('Component:', instance);
  console.error('Info:', info);
  
  // In production, you might want to send errors to a logging service
  if (import.meta.env.PROD) {
    // TODO: Send to error logging service (e.g., Sentry)
  }
};

/**
 * Global Warning Handler (development only)
 * Helps catch potential issues during development
 */
app.config.warnHandler = (msg, instance, trace) => {
  console.warn('Vue Warning:', msg);
  if (import.meta.env.DEV) {
    console.warn('Trace:', trace);
  }
};

/**
 * Global Properties
 * Make commonly used values available to all components
 * Access via this.$appName, this.$version in OPTIONS API components
 */
app.config.globalProperties.$appName = 'Art Gallery Management System';
app.config.globalProperties.$version = '1.0.0';

/**
 * Custom directive: v-focus
 * Automatically focuses an input element when mounted
 * Usage: <input v-focus />
 */
app.directive('focus', {
  mounted(el) {
    el.focus();
  }
});

/**
 * Custom directive: v-currency
 * Formats a number as currency display
 * Usage: <span v-currency="150000"></span>
 */
app.directive('currency', {
  mounted(el, binding) {
    const value = binding.value || 0;
    el.textContent = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  },
  updated(el, binding) {
    const value = binding.value || 0;
    el.textContent = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }
});

/**
 * Mount the application to the DOM
 * The #app element is defined in index.html
 */
app.mount('#app');

/**
 * Initialize the application store
 * Load data from localStorage for all modules
 */
store.dispatch('initializeApp').then(() => {
  if (import.meta.env.DEV) {
    console.log('📦 Store initialized from localStorage');
  }
});

// Log application start in development mode
if (import.meta.env.DEV) {
  console.log('🎨 Art Gallery Management System started');
  console.log('📊 DW/BI & Vue.js Project');
  console.log('🔧 Running in development mode');
}
