/**
 * Artwork Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages all artwork-related state, mutations, actions, and getters.
 * This module is namespaced as 'artwork'.
 * 
 * Access in components:
 *   - State: this.$store.state.artwork.artworks
 *   - Getters: this.$store.getters['artwork/artworkById'](id)
 *   - Mutations: this.$store.commit('artwork/SET_ARTWORKS', artworks)
 *   - Actions: this.$store.dispatch('artwork/fetchArtworks')
 */

import { artworkAPI } from '@/api/artworkAPI';

// LocalStorage key for artwork data
const STORAGE_KEY = 'artGallery_artworks';

export default {
  // Enable namespacing for this module
  namespaced: true,

  /**
   * State - Reactive data for artworks
   * Returns a function to ensure fresh state for each store instance
   */
  state: () => ({
    // Array of all artworks
    artworks: [],
    
    // Currently selected artwork (for detail view)
    selectedArtwork: null,
    
    // Loading state for async operations
    isLoading: false,
    
    // Error message from last operation
    error: null,
    
    // Filter and sort options
    filter: {
      searchQuery: '',
      artistId: null,
      collectionId: null,
      medium: null,
      yearFrom: null,
      yearTo: null,
      valueMin: null,
      valueMax: null
    },
    
    // Sort configuration
    sort: {
      field: 'title',
      direction: 'asc' // 'asc' or 'desc'
    },
    
    // Pagination
    pagination: {
      currentPage: 1,
      itemsPerPage: 12,
      totalItems: 0
    }
  }),

  /**
   * Mutations - Synchronous state changes
   * All mutations should be named in SCREAMING_SNAKE_CASE
   */
  mutations: {
    /**
     * Set all artworks
     */
    SET_ARTWORKS(state, artworks) {
      state.artworks = artworks;
      state.pagination.totalItems = artworks.length;
    },

    /**
     * Add a new artwork
     */
    ADD_ARTWORK(state, artwork) {
      // Generate ID if not present
      if (!artwork.id) {
        artwork.id = Date.now();
      }
      // Add timestamps
      artwork.createdAt = new Date().toISOString();
      artwork.updatedAt = new Date().toISOString();
      
      state.artworks.push(artwork);
      state.pagination.totalItems = state.artworks.length;
    },

    /**
     * Update an existing artwork
     */
    UPDATE_ARTWORK(state, updatedArtwork) {
      const index = state.artworks.findIndex(a => a.id === updatedArtwork.id);
      if (index !== -1) {
        updatedArtwork.updatedAt = new Date().toISOString();
        // Use Vue's reactivity - replace the object
        state.artworks.splice(index, 1, { 
          ...state.artworks[index], 
          ...updatedArtwork 
        });
      }
    },

    /**
     * Delete an artwork by ID
     */
    DELETE_ARTWORK(state, id) {
      state.artworks = state.artworks.filter(a => a.id !== id);
      state.pagination.totalItems = state.artworks.length;
      
      // Clear selection if deleted artwork was selected
      if (state.selectedArtwork?.id === id) {
        state.selectedArtwork = null;
      }
    },

    /**
     * Set selected artwork
     */
    SET_SELECTED_ARTWORK(state, artwork) {
      state.selectedArtwork = artwork;
    },

    /**
     * Clear selected artwork
     */
    CLEAR_SELECTED_ARTWORK(state) {
      state.selectedArtwork = null;
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
     * Clear error message
     */
    CLEAR_ERROR(state) {
      state.error = null;
    },

    /**
     * Update filter options
     */
    SET_FILTER(state, filter) {
      state.filter = { ...state.filter, ...filter };
      // Reset to first page when filter changes
      state.pagination.currentPage = 1;
    },

    /**
     * Reset all filters
     */
    RESET_FILTER(state) {
      state.filter = {
        searchQuery: '',
        artistId: null,
        collectionId: null,
        medium: null,
        yearFrom: null,
        yearTo: null,
        valueMin: null,
        valueMax: null
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
    }
  },

  /**
   * Actions - Async operations that commit mutations
   * Actions can be async and should handle API calls
   */
  actions: {
    /**
     * Fetch all artworks from API
     */
    async fetchArtworks({ commit, dispatch }) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const response = await artworkAPI.getAll();
        commit('SET_ARTWORKS', response.data);
        
        // Also save to LocalStorage for persistence
        localStorage.setItem(STORAGE_KEY, JSON.stringify(response.data));
        
        return response.data;
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
     * Fetch a single artwork by ID
     */
    async fetchArtworkById({ commit, state }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        // First check if already in state
        const existing = state.artworks.find(a => a.id === parseInt(id));
        if (existing) {
          commit('SET_SELECTED_ARTWORK', existing);
          return existing;
        }
        
        // Fetch from API
        const response = await artworkAPI.getById(id);
        commit('SET_SELECTED_ARTWORK', response.data);
        
        return response.data;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to fetch artwork';
        commit('SET_ERROR', message);
        console.error('Fetch artwork by ID error:', error);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Create a new artwork
     */
    async createArtwork({ commit, dispatch }, artwork) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let newArtwork;
        try {
          const response = await artworkAPI.create(artwork);
          newArtwork = response.data;
        } catch (apiError) {
          // API unavailable, create locally
          console.warn('API unavailable, creating artwork locally');
          newArtwork = { ...artwork };
        }
        
        commit('ADD_ARTWORK', newArtwork);
        dispatch('saveToLocalStorage');
        
        return newArtwork;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to create artwork';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Update an existing artwork
     */
    async updateArtwork({ commit, dispatch }, artwork) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let updatedArtwork;
        try {
          const response = await artworkAPI.update(artwork.id, artwork);
          updatedArtwork = response.data;
        } catch (apiError) {
          // API unavailable, update locally
          console.warn('API unavailable, updating artwork locally');
          updatedArtwork = { ...artwork };
        }
        
        commit('UPDATE_ARTWORK', updatedArtwork);
        dispatch('saveToLocalStorage');
        
        return updatedArtwork;
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to update artwork';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Delete an artwork
     */
    async deleteArtwork({ commit, dispatch }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        try {
          await artworkAPI.delete(id);
        } catch (apiError) {
          // API unavailable, delete locally
          console.warn('API unavailable, deleting artwork locally');
        }
        
        commit('DELETE_ARTWORK', id);
        dispatch('saveToLocalStorage');
        
        return { success: true };
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to delete artwork';
        commit('SET_ERROR', message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Load artworks from LocalStorage
     * Called during app initialization
     */
    loadFromLocalStorage({ commit, dispatch }) {
      try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          const artworks = JSON.parse(stored);
          commit('SET_ARTWORKS', artworks);
          
          if (import.meta.env.DEV) {
            console.log(`📦 Loaded ${artworks.length} artworks from LocalStorage`);
          }
        } else {
          // Seed with initial data if localStorage is empty
          const initialArtworks = [
            { id: 1, title: 'Starry Night', artist: 'Vincent van Gogh', year: 1889, category: 'Painting', status: 'On Display', medium: 'Oil on canvas', dimensions: '73.7 cm × 92.1 cm', location: 'Gallery A, Room 3', imageUrl: '', tags: ['Post-Impressionism', 'Night Scene'], createdAt: new Date().toISOString() },
            { id: 2, title: 'The Persistence of Memory', artist: 'Salvador Dalí', year: 1931, category: 'Painting', status: 'In Storage', medium: 'Oil on canvas', dimensions: '24 cm × 33 cm', location: 'Storage Unit B-12', imageUrl: '', tags: ['Surrealism'], createdAt: new Date().toISOString() },
            { id: 3, title: 'Girl with a Pearl Earring', artist: 'Johannes Vermeer', year: 1665, category: 'Painting', status: 'On Loan', medium: 'Oil on canvas', dimensions: '44.5 cm × 39 cm', location: 'Metropolitan Museum, New York', imageUrl: '', tags: ['Dutch Golden Age', 'Portrait'], createdAt: new Date().toISOString() },
            { id: 4, title: 'The Birth of Venus', artist: 'Sandro Botticelli', year: 1485, category: 'Painting', status: 'On Display', medium: 'Tempera on canvas', dimensions: '172.5 cm × 278.9 cm', location: 'Gallery B, Room 1', imageUrl: '', tags: ['Renaissance', 'Mythology'], createdAt: new Date().toISOString() },
            { id: 5, title: 'The Thinker', artist: 'Auguste Rodin', year: 1904, category: 'Sculpture', status: 'On Display', medium: 'Bronze', dimensions: '189 cm × 98 cm × 140 cm', location: 'Sculpture Garden', imageUrl: '', tags: ['Sculpture', 'Bronze'], createdAt: new Date().toISOString() },
            { id: 6, title: 'David', artist: 'Michelangelo', year: 1504, category: 'Sculpture', status: 'On Loan', medium: 'Marble', dimensions: '517 cm × 199 cm', location: 'Galleria dell\'Accademia, Florence', imageUrl: '', tags: ['Renaissance', 'Marble'], createdAt: new Date().toISOString() },
            { id: 7, title: 'Migrant Mother', artist: 'Dorothea Lange', year: 1936, category: 'Photography', status: 'In Storage', medium: 'Gelatin silver print', dimensions: '28.3 cm × 21.8 cm', location: 'Archive Room 2', imageUrl: '', tags: ['Documentary', 'Great Depression'], createdAt: new Date().toISOString() },
            { id: 8, title: 'Water Lilies', artist: 'Claude Monet', year: 1906, category: 'Painting', status: 'Under Restoration', medium: 'Oil on canvas', dimensions: '89.9 cm × 94.1 cm', location: 'Restoration Lab', imageUrl: '', tags: ['Impressionism', 'Nature'], createdAt: new Date().toISOString() }
          ];
          commit('SET_ARTWORKS', initialArtworks);
          dispatch('saveToLocalStorage');
          
          if (import.meta.env.DEV) {
            console.log(`📦 Seeded ${initialArtworks.length} initial artworks`);
          }
        }
      } catch (error) {
        console.error('Load from LocalStorage error:', error);
        // Clear corrupted data
        localStorage.removeItem(STORAGE_KEY);
      }
    },

    /**
     * Save current artworks to LocalStorage
     */
    saveToLocalStorage({ state }) {
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state.artworks));
      } catch (error) {
        console.error('Save to LocalStorage error:', error);
      }
    },

    /**
     * Clear all artworks
     */
    clearArtworks({ commit }) {
      commit('SET_ARTWORKS', []);
      commit('CLEAR_SELECTED_ARTWORK');
      localStorage.removeItem(STORAGE_KEY);
    },

    /**
     * Apply filter to artworks
     */
    applyFilter({ commit }, filter) {
      commit('SET_FILTER', filter);
    },

    /**
     * Apply sort to artworks
     */
    applySort({ commit }, sortConfig) {
      commit('SET_SORT', sortConfig);
    },

    /**
     * Change page
     */
    changePage({ commit }, page) {
      commit('SET_CURRENT_PAGE', page);
    }
  },

  /**
   * Getters - Computed properties for state
   * Return functions for parameterized getters
   */
  getters: {
    /**
     * Get artwork by ID
     * Usage: this.$store.getters['artwork/artworkById'](id)
     */
    artworkById: (state) => (id) => {
      return state.artworks.find(a => a.id === parseInt(id));
    },

    /**
     * Get filtered artworks based on filter options
     */
    filteredArtworks: (state) => {
      let result = [...state.artworks];
      const { filter } = state;

      // Search query filter (title, description)
      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(a => 
          a.title?.toLowerCase().includes(query) ||
          a.description?.toLowerCase().includes(query)
        );
      }

      // Artist filter
      if (filter.artistId) {
        result = result.filter(a => a.artistId === filter.artistId);
      }

      // Collection filter
      if (filter.collectionId) {
        result = result.filter(a => a.collectionId === filter.collectionId);
      }

      // Medium filter
      if (filter.medium) {
        result = result.filter(a => a.medium === filter.medium);
      }

      // Year range filter
      if (filter.yearFrom) {
        result = result.filter(a => a.yearCreated >= filter.yearFrom);
      }
      if (filter.yearTo) {
        result = result.filter(a => a.yearCreated <= filter.yearTo);
      }

      // Value range filter
      if (filter.valueMin) {
        result = result.filter(a => a.estimatedValue >= filter.valueMin);
      }
      if (filter.valueMax) {
        result = result.filter(a => a.estimatedValue <= filter.valueMax);
      }

      return result;
    },

    /**
     * Get sorted artworks
     */
    sortedArtworks: (state, getters) => {
      const artworks = [...getters.filteredArtworks];
      const { field, direction } = state.sort;

      artworks.sort((a, b) => {
        let valueA = a[field];
        let valueB = b[field];

        // Handle string comparison
        if (typeof valueA === 'string') {
          valueA = valueA.toLowerCase();
          valueB = valueB?.toLowerCase() || '';
        }

        // Handle null/undefined
        if (valueA == null) return direction === 'asc' ? 1 : -1;
        if (valueB == null) return direction === 'asc' ? -1 : 1;

        // Compare
        if (valueA < valueB) return direction === 'asc' ? -1 : 1;
        if (valueA > valueB) return direction === 'asc' ? 1 : -1;
        return 0;
      });

      return artworks;
    },

    /**
     * Get paginated artworks
     */
    paginatedArtworks: (state, getters) => {
      const { currentPage, itemsPerPage } = state.pagination;
      const start = (currentPage - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      
      return getters.sortedArtworks.slice(start, end);
    },

    /**
     * Get total pages for pagination
     */
    totalPages: (state, getters) => {
      return Math.ceil(getters.filteredArtworks.length / state.pagination.itemsPerPage);
    },

    /**
     * Get total estimated value of all artworks
     */
    totalArtworkValue: (state) => {
      return state.artworks.reduce((sum, a) => sum + (a.estimatedValue || 0), 0);
    },

    /**
     * Get artworks by artist ID
     * Usage: this.$store.getters['artwork/artworksByArtist'](artistId)
     */
    artworksByArtist: (state) => (artistId) => {
      return state.artworks.filter(a => a.artistId === artistId);
    },

    /**
     * Get artworks by collection ID
     */
    artworksByCollection: (state) => (collectionId) => {
      return state.artworks.filter(a => a.collectionId === collectionId);
    },

    /**
     * Get unique mediums for filter dropdown
     */
    uniqueMediums: (state) => {
      const mediums = state.artworks.map(a => a.medium).filter(Boolean);
      return [...new Set(mediums)].sort();
    },

    /**
     * Get artwork count
     */
    artworkCount: (state) => state.artworks.length,

    /**
     * Check if loading
     */
    isLoading: (state) => state.isLoading,

    /**
     * Get error message
     */
    error: (state) => state.error,

    /**
     * Check if has error
     */
    hasError: (state) => state.error !== null
  }
};
