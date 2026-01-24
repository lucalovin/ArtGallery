/**
 * Vuex Store Configuration
 * Art Gallery Management System
 * 
 * This file sets up the Vuex 4 store with multiple modules:
 * - artworkStore: Manages artwork data and operations
 * - exhibitionStore: Manages exhibition data and operations
 * - visitorStore: Manages visitor and review data
 * - staffStore: Manages staff activities
 * - loansStore: Manages loan and restoration data
 * - reportsStore: Manages BI report data
 * 
 * Each module is namespaced for clear separation of concerns.
 * Access patterns in components:
 *   - State: this.$store.state.moduleName.property
 *   - Getters: this.$store.getters['moduleName/getterName']
 *   - Mutations: this.$store.commit('moduleName/MUTATION_NAME', payload)
 *   - Actions: this.$store.dispatch('moduleName/actionName', payload)
 */

import { createStore } from 'vuex';

// Import store modules
import artworkStore from './modules/artworkStore';
import exhibitionStore from './modules/exhibitionStore';
import visitorStore from './modules/visitorStore';
import staffStore from './modules/staffStore';
import loansStore from './modules/loansStore';
import reportsStore from './modules/reportsStore';

/**
 * Create and export the Vuex store
 */
const store = createStore({
  /**
   * Strict mode in development
   * Throws error if state is mutated outside mutations
   */
  strict: import.meta.env.DEV,

  /**
   * Root state
   * Global state accessible to all modules
   */
  state: () => ({
    // Application-wide loading state
    isLoading: false,
    
    // Global error message
    globalError: null,
    
    // Last sync timestamp for DW operations
    lastDWSync: null,
    
    // Application initialization status
    isInitialized: false,

    // API connection status
    apiStatus: 'unknown' // 'connected', 'disconnected', 'error'
  }),

  /**
   * Root mutations
   * Synchronous state changes
   */
  mutations: {
    /**
     * Set global loading state
     */
    SET_LOADING(state, isLoading) {
      state.isLoading = isLoading;
    },

    /**
     * Set global error message
     */
    SET_GLOBAL_ERROR(state, error) {
      state.globalError = error;
    },

    /**
     * Clear global error
     */
    CLEAR_GLOBAL_ERROR(state) {
      state.globalError = null;
    },

    /**
     * Update last DW sync timestamp
     */
    SET_LAST_DW_SYNC(state, timestamp) {
      state.lastDWSync = timestamp;
    },

    /**
     * Set app initialization status
     */
    SET_INITIALIZED(state, status) {
      state.isInitialized = status;
    },

    /**
     * Update API connection status
     */
    SET_API_STATUS(state, status) {
      state.apiStatus = status;
    }
  },

  /**
   * Root actions
   * Async operations that commit mutations
   */
  actions: {
    /**
     * Initialize the application
     * Loads data from LocalStorage for all modules
     */
    async initializeApp({ commit, dispatch }) {
      try {
        commit('SET_LOADING', true);
        
        // Load data from LocalStorage for each module
        await Promise.all([
          dispatch('artwork/loadFromLocalStorage'),
          dispatch('exhibition/loadFromLocalStorage'),
          dispatch('visitor/loadFromLocalStorage'),
          dispatch('staff/loadFromLocalStorage'),
          dispatch('loans/loadFromLocalStorage'),
          dispatch('reports/loadFromLocalStorage')
        ]);

        commit('SET_INITIALIZED', true);
        
        if (import.meta.env.DEV) {
          console.log('✅ Store initialized successfully');
        }
      } catch (error) {
        commit('SET_GLOBAL_ERROR', 'Failed to initialize application');
        console.error('Store initialization error:', error);
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Check API connection status
     */
    async checkApiConnection({ commit }) {
      try {
        // Try to ping the API
        const response = await fetch(
          import.meta.env.VITE_API_BASE_URL || 'http://localhost:3000'
        );
        
        if (response.ok) {
          commit('SET_API_STATUS', 'connected');
        } else {
          commit('SET_API_STATUS', 'error');
        }
      } catch (error) {
        commit('SET_API_STATUS', 'disconnected');
      }
    },

    /**
     * Trigger DW refresh (ETL operation)
     * Coordinates data sync across modules
     */
    async refreshDataWarehouse({ commit, dispatch }) {
      try {
        commit('SET_LOADING', true);
        
        // Refresh all data from API
        await Promise.all([
          dispatch('artwork/fetchArtworks'),
          dispatch('exhibition/fetchExhibitions'),
          dispatch('visitor/fetchVisitors'),
          dispatch('loans/fetchLoans'),
          dispatch('reports/fetchReportData')
        ]);

        // Update sync timestamp
        const timestamp = new Date().toISOString();
        commit('SET_LAST_DW_SYNC', timestamp);
        
        // Persist timestamp to LocalStorage
        localStorage.setItem('lastDWSync', timestamp);

        return { success: true, timestamp };
      } catch (error) {
        commit('SET_GLOBAL_ERROR', 'Failed to refresh Data Warehouse');
        console.error('DW refresh error:', error);
        return { success: false, error: error.message };
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Clear all application data
     * Useful for logout or data reset
     */
    async clearAllData({ commit, dispatch }) {
      try {
        // Clear each module's data
        await Promise.all([
          dispatch('artwork/clearArtworks'),
          dispatch('exhibition/clearExhibitions'),
          dispatch('visitor/clearVisitors'),
          dispatch('staff/clearStaff'),
          dispatch('loans/clearLoans'),
          dispatch('reports/clearReports')
        ]);

        // Clear LocalStorage
        localStorage.clear();

        // Reset initialization status
        commit('SET_INITIALIZED', false);
        commit('SET_LAST_DW_SYNC', null);

        return { success: true };
      } catch (error) {
        commit('SET_GLOBAL_ERROR', 'Failed to clear data');
        return { success: false, error: error.message };
      }
    }
  },

  /**
   * Root getters
   * Computed properties for state
   */
  getters: {
    /**
     * Check if app is loading
     */
    isLoading: (state) => state.isLoading,

    /**
     * Get global error message
     */
    globalError: (state) => state.globalError,

    /**
     * Check if app has a global error
     */
    hasError: (state) => state.globalError !== null,

    /**
     * Get formatted last sync time
     */
    formattedLastSync: (state) => {
      if (!state.lastDWSync) return 'Never';
      
      const date = new Date(state.lastDWSync);
      return date.toLocaleString();
    },

    /**
     * Check if app is initialized
     */
    isInitialized: (state) => state.isInitialized,

    /**
     * Check if API is connected
     */
    isApiConnected: (state) => state.apiStatus === 'connected',

    /**
     * Get total record counts across modules
     * Useful for dashboard KPIs
     */
    totalRecordCounts: (state) => ({
      artworks: state.artwork?.artworks?.length || 0,
      exhibitions: state.exhibition?.exhibitions?.length || 0,
      visitors: state.visitor?.visitors?.length || 0,
      staff: state.staff?.staff?.length || 0,
      loans: state.loans?.loans?.length || 0
    })
  },

  /**
   * Register store modules
   * Each module is namespaced
   */
  modules: {
    artwork: artworkStore,
    exhibition: exhibitionStore,
    visitor: visitorStore,
    staff: staffStore,
    loans: loansStore,
    reports: reportsStore
  }
});

/**
 * Hot module replacement for Vuex in development
 */
if (import.meta.hot) {
  import.meta.hot.accept([
    './modules/artworkStore',
    './modules/exhibitionStore',
    './modules/visitorStore',
    './modules/staffStore',
    './modules/loansStore',
    './modules/reportsStore'
  ], () => {
    // Reload modules
    store.hotUpdate({
      modules: {
        artwork: artworkStore,
        exhibition: exhibitionStore,
        visitor: visitorStore,
        staff: staffStore,
        loans: loansStore,
        reports: reportsStore
      }
    });
  });
}

export default store;
