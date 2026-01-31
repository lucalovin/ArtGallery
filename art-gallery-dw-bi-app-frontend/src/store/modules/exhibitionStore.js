/**
 * Exhibition Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages all exhibition-related state, mutations, actions, and getters.
 * This module is namespaced as 'exhibition'.
 * 
 * Access in components:
 *   - State: this.$store.state.exhibition.exhibitions
 *   - Getters: this.$store.getters['exhibition/exhibitionById'](id)
 *   - Mutations: this.$store.commit('exhibition/SET_EXHIBITIONS', exhibitions)
 *   - Actions: this.$store.dispatch('exhibition/fetchExhibitions')
 */

import { exhibitionAPI } from '@/api/exhibitionAPI';

// LocalStorage key for exhibition data
const STORAGE_KEY = 'artGallery_exhibitions';

export default {
  // Enable namespacing for this module
  namespaced: true,

  /**
   * State - Reactive data for exhibitions
   */
  state: () => ({
    // Array of all exhibitions
    exhibitions: [],
    
    // Currently selected exhibition
    selectedExhibition: null,
    
    // Loading state
    isLoading: false,
    
    // Error message
    error: null,
    
    // Filter options
    filter: {
      searchQuery: '',
      exhibitorId: null,
      status: null, // 'upcoming', 'ongoing', 'past'
      dateFrom: null,
      dateTo: null
    },
    
    // Sort configuration
    sort: {
      field: 'startDate',
      direction: 'desc'
    },
    
    // Pagination
    pagination: {
      currentPage: 1,
      itemsPerPage: 10,
      totalItems: 0
    }
  }),

  /**
   * Mutations - Synchronous state changes
   */
  mutations: {
    /**
     * Set all exhibitions
     */
    SET_EXHIBITIONS(state, exhibitions) {
      // Ensure exhibitions is always an array
      state.exhibitions = Array.isArray(exhibitions) ? exhibitions : [];
      state.pagination.totalItems = state.exhibitions.length;
    },

    /**
     * Add a new exhibition
     */
    ADD_EXHIBITION(state, exhibition) {
      if (!exhibition.id) {
        exhibition.id = Date.now();
      }
      exhibition.createdAt = new Date().toISOString();
      exhibition.updatedAt = new Date().toISOString();
      
      // Ensure exhibitions is an array before pushing
      if (!Array.isArray(state.exhibitions)) {
        state.exhibitions = [];
      }
      state.exhibitions.push(exhibition);
      state.pagination.totalItems = state.exhibitions.length;
    },

    /**
     * Update an existing exhibition
     */
    UPDATE_EXHIBITION(state, updatedExhibition) {
      const index = state.exhibitions.findIndex(e => e.id === updatedExhibition.id);
      if (index !== -1) {
        updatedExhibition.updatedAt = new Date().toISOString();
        state.exhibitions.splice(index, 1, {
          ...state.exhibitions[index],
          ...updatedExhibition
        });
      }
    },

    /**
     * Delete an exhibition by ID
     */
    DELETE_EXHIBITION(state, id) {
      state.exhibitions = state.exhibitions.filter(e => e.id !== id);
      state.pagination.totalItems = state.exhibitions.length;
      
      if (state.selectedExhibition?.id === id) {
        state.selectedExhibition = null;
      }
    },

    /**
     * Set selected exhibition
     */
    SET_SELECTED_EXHIBITION(state, exhibition) {
      state.selectedExhibition = exhibition;
    },

    /**
     * Clear selected exhibition
     */
    CLEAR_SELECTED_EXHIBITION(state) {
      state.selectedExhibition = null;
    },

    /**
     * Set loading state
     */
    SET_LOADING(state, isLoading) {
      state.isLoading = isLoading;
    },

    /**
     * Set error message
     */
    SET_ERROR(state, error) {
      state.error = error;
    },

    /**
     * Clear error
     */
    CLEAR_ERROR(state) {
      state.error = null;
    },

    /**
     * Update filter options
     */
    SET_FILTER(state, filter) {
      state.filter = { ...state.filter, ...filter };
      state.pagination.currentPage = 1;
    },

    /**
     * Reset filters
     */
    RESET_FILTER(state) {
      state.filter = {
        searchQuery: '',
        exhibitorId: null,
        status: null,
        dateFrom: null,
        dateTo: null
      };
      state.pagination.currentPage = 1;
    },

    /**
     * Set sort configuration
     */
    SET_SORT(state, { field, direction }) {
      state.sort.field = field;
      state.sort.direction = direction;
    },

    /**
     * Set current page
     */
    SET_CURRENT_PAGE(state, page) {
      state.pagination.currentPage = page;
    },

    /**
     * Set items per page
     */
    SET_ITEMS_PER_PAGE(state, count) {
      state.pagination.itemsPerPage = count;
      state.pagination.currentPage = 1;
    },

    /**
     * Add artwork to exhibition
     */
    ADD_ARTWORK_TO_EXHIBITION(state, { exhibitionId, artworkId }) {
      const exhibition = state.exhibitions.find(e => e.id === exhibitionId);
      if (exhibition) {
        if (!exhibition.artworkIds) {
          exhibition.artworkIds = [];
        }
        if (!exhibition.artworkIds.includes(artworkId)) {
          exhibition.artworkIds.push(artworkId);
          exhibition.updatedAt = new Date().toISOString();
        }
      }
    },

    /**
     * Remove artwork from exhibition
     */
    REMOVE_ARTWORK_FROM_EXHIBITION(state, { exhibitionId, artworkId }) {
      const exhibition = state.exhibitions.find(e => e.id === exhibitionId);
      if (exhibition && exhibition.artworkIds) {
        exhibition.artworkIds = exhibition.artworkIds.filter(id => id !== artworkId);
        exhibition.updatedAt = new Date().toISOString();
      }
    }
  },

  /**
   * Actions - Async operations
   */
  actions: {
    /**
     * Fetch all exhibitions from API
     */
    async fetchExhibitions({ commit, dispatch }) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const response = await exhibitionAPI.getAll();
        // API returns { success: true, data: { items: [...], totalCount: N } }
        let exhibitions = [];
        if (response.data?.success && response.data?.data) {
          const data = response.data.data;
          exhibitions = Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
        } else if (Array.isArray(response.data)) {
          exhibitions = response.data;
        }
        
        commit('SET_EXHIBITIONS', exhibitions);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(exhibitions));
        
        return exhibitions;
      } catch (error) {
        console.warn('API unavailable, loading from localStorage:', error.message);
        // Fallback to localStorage when API fails
        dispatch('loadFromLocalStorage');
        return [];
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Fetch a single exhibition by ID
     */
    async fetchExhibitionById({ commit, state }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const existing = state.exhibitions.find(e => e.id === parseInt(id));
        if (existing) {
          commit('SET_SELECTED_EXHIBITION', existing);
          return existing;
        }
        
        const response = await exhibitionAPI.getById(id);
        commit('SET_SELECTED_EXHIBITION', response.data);
        
        return response.data;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to fetch exhibition';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Create a new exhibition
     */
    async createExhibition({ commit, dispatch }, exhibition) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let newExhibition;
        try {
          const response = await exhibitionAPI.create(exhibition);
          newExhibition = response.data;
        } catch (apiError) {
          // API unavailable, create locally
          console.warn('API unavailable, creating exhibition locally');
          newExhibition = { ...exhibition };
        }
        
        commit('ADD_EXHIBITION', newExhibition);
        dispatch('saveToLocalStorage');
        
        return newExhibition;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to create exhibition';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Update an existing exhibition
     */
    async updateExhibition({ commit, dispatch }, exhibition) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let updatedExhibition;
        try {
          const response = await exhibitionAPI.update(exhibition.id, exhibition);
          updatedExhibition = response.data;
        } catch (apiError) {
          // API unavailable, update locally
          console.warn('API unavailable, updating exhibition locally');
          updatedExhibition = { ...exhibition };
        }
        
        commit('UPDATE_EXHIBITION', updatedExhibition);
        dispatch('saveToLocalStorage');
        
        return updatedExhibition;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to update exhibition';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Delete an exhibition
     */
    async deleteExhibition({ commit, dispatch }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        try {
          await exhibitionAPI.delete(id);
        } catch (apiError) {
          // API unavailable, delete locally
          console.warn('API unavailable, deleting exhibition locally');
        }
        
        commit('DELETE_EXHIBITION', id);
        dispatch('saveToLocalStorage');
        
        return { success: true };
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to delete exhibition';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Load exhibitions from LocalStorage
     */
    loadFromLocalStorage({ commit, dispatch }) {
      try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          const parsed = JSON.parse(stored);
          // Ensure parsed data is an array
          const exhibitions = Array.isArray(parsed) ? parsed : [];
          commit('SET_EXHIBITIONS', exhibitions);
          
          if (import.meta.env.DEV) {
            console.log(`📦 Loaded ${exhibitions.length} exhibitions from LocalStorage`);
          }
        } else {
          // Seed with initial data if localStorage is empty
          const initialExhibitions = [
            { id: 1, title: 'Modern Masters', description: 'Exploring the works of 20th century masters', startDate: '2024-01-15', endDate: '2024-04-30', status: 'current', curator: 'Dr. Sarah Mitchell', location: 'Gallery Wing A', ticketPrice: 25, artworkCount: 45, createdAt: new Date().toISOString() },
            { id: 2, title: 'Renaissance Revival', description: 'Classical art from the Renaissance period', startDate: '2024-05-01', endDate: '2024-08-15', status: 'upcoming', curator: 'Prof. Marco Rossi', location: 'Gallery Wing B', ticketPrice: 30, artworkCount: 32, createdAt: new Date().toISOString() },
            { id: 3, title: 'Contemporary Visions', description: 'Modern artists and their perspectives', startDate: '2024-06-01', endDate: '2024-09-30', status: 'upcoming', curator: 'Emily Chen', location: 'Contemporary Wing', ticketPrice: 20, artworkCount: 28, createdAt: new Date().toISOString() },
            { id: 4, title: 'Impressionism Era', description: 'Light and color in motion', startDate: '2023-09-01', endDate: '2023-12-31', status: 'past', curator: 'Dr. Sarah Mitchell', location: 'Gallery Wing A', ticketPrice: 25, artworkCount: 50, createdAt: new Date().toISOString() }
          ];
          commit('SET_EXHIBITIONS', initialExhibitions);
          dispatch('saveToLocalStorage');
          
          if (import.meta.env.DEV) {
            console.log(`📦 Seeded ${initialExhibitions.length} initial exhibitions`);
          }
        }
      } catch (error) {
        console.error('Load exhibitions from LocalStorage error:', error);
        localStorage.removeItem(STORAGE_KEY);
      }
    },

    /**
     * Save exhibitions to LocalStorage
     */
    saveToLocalStorage({ state }) {
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state.exhibitions));
      } catch (error) {
        console.error('Save exhibitions to LocalStorage error:', error);
      }
    },

    /**
     * Clear all exhibitions
     */
    clearExhibitions({ commit }) {
      commit('SET_EXHIBITIONS', []);
      commit('CLEAR_SELECTED_EXHIBITION');
      localStorage.removeItem(STORAGE_KEY);
    },

    /**
     * Apply filter
     */
    applyFilter({ commit }, filter) {
      commit('SET_FILTER', filter);
    },

    /**
     * Apply sort
     */
    applySort({ commit }, sortConfig) {
      commit('SET_SORT', sortConfig);
    },

    /**
     * Add artwork to exhibition
     */
    addArtworkToExhibition({ commit, dispatch }, payload) {
      commit('ADD_ARTWORK_TO_EXHIBITION', payload);
      dispatch('saveToLocalStorage');
    },

    /**
     * Remove artwork from exhibition
     */
    removeArtworkFromExhibition({ commit, dispatch }, payload) {
      commit('REMOVE_ARTWORK_FROM_EXHIBITION', payload);
      dispatch('saveToLocalStorage');
    }
  },

  /**
   * Getters - Computed properties
   */
  getters: {
    /**
     * Get exhibition by ID
     */
    exhibitionById: (state) => (id) => {
      return state.exhibitions.find(e => e.id === parseInt(id));
    },

    /**
     * Get exhibition status based on dates
     */
    getExhibitionStatus: () => (exhibition) => {
      const now = new Date();
      const startDate = new Date(exhibition.startDate);
      const endDate = new Date(exhibition.endDate);
      
      if (now < startDate) return 'upcoming';
      if (now > endDate) return 'past';
      return 'ongoing';
    },

    /**
     * Get filtered exhibitions
     */
    filteredExhibitions: (state, getters) => {
      let result = [...state.exhibitions];
      const { filter } = state;

      // Search query
      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(e =>
          e.title?.toLowerCase().includes(query) ||
          e.description?.toLowerCase().includes(query)
        );
      }

      // Exhibitor filter
      if (filter.exhibitorId) {
        result = result.filter(e => e.exhibitorId === filter.exhibitorId);
      }

      // Status filter
      if (filter.status) {
        result = result.filter(e => getters.getExhibitionStatus(e) === filter.status);
      }

      // Date range filter
      if (filter.dateFrom) {
        result = result.filter(e => new Date(e.startDate) >= new Date(filter.dateFrom));
      }
      if (filter.dateTo) {
        result = result.filter(e => new Date(e.endDate) <= new Date(filter.dateTo));
      }

      return result;
    },

    /**
     * Get sorted exhibitions
     */
    sortedExhibitions: (state, getters) => {
      const exhibitions = [...getters.filteredExhibitions];
      const { field, direction } = state.sort;

      exhibitions.sort((a, b) => {
        let valueA = a[field];
        let valueB = b[field];

        // Handle date comparison
        if (field.includes('Date')) {
          valueA = new Date(valueA).getTime();
          valueB = new Date(valueB).getTime();
        }

        if (typeof valueA === 'string') {
          valueA = valueA.toLowerCase();
          valueB = valueB?.toLowerCase() || '';
        }

        if (valueA == null) return direction === 'asc' ? 1 : -1;
        if (valueB == null) return direction === 'asc' ? -1 : 1;

        if (valueA < valueB) return direction === 'asc' ? -1 : 1;
        if (valueA > valueB) return direction === 'asc' ? 1 : -1;
        return 0;
      });

      return exhibitions;
    },

    /**
     * Get paginated exhibitions
     */
    paginatedExhibitions: (state, getters) => {
      const { currentPage, itemsPerPage } = state.pagination;
      const start = (currentPage - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      
      return getters.sortedExhibitions.slice(start, end);
    },

    /**
     * Get total pages
     */
    totalPages: (state, getters) => {
      return Math.ceil(getters.filteredExhibitions.length / state.pagination.itemsPerPage);
    },

    /**
     * Get upcoming exhibitions
     */
    upcomingExhibitions: (state, getters) => {
      return state.exhibitions.filter(e => getters.getExhibitionStatus(e) === 'upcoming');
    },

    /**
     * Get ongoing exhibitions
     */
    ongoingExhibitions: (state, getters) => {
      return state.exhibitions.filter(e => getters.getExhibitionStatus(e) === 'ongoing');
    },

    /**
     * Get past exhibitions
     */
    pastExhibitions: (state, getters) => {
      return state.exhibitions.filter(e => getters.getExhibitionStatus(e) === 'past');
    },

    /**
     * Get exhibition count
     */
    exhibitionCount: (state) => state.exhibitions.length,

    /**
     * Get exhibitions by exhibitor
     */
    exhibitionsByExhibitor: (state) => (exhibitorId) => {
      return state.exhibitions.filter(e => e.exhibitorId === exhibitorId);
    },

    /**
     * Check if loading
     */
    isLoading: (state) => state.isLoading,

    /**
     * Get error
     */
    error: (state) => state.error
  }
};
