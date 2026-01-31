/**
 * Visitor Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages visitor data and gallery reviews.
 * This module is namespaced as 'visitor'.
 */

import { visitorAPI } from '@/api/visitorAPI';

const STORAGE_KEY = 'artGallery_visitors';
const REVIEWS_KEY = 'artGallery_reviews';

export default {
  namespaced: true,

  state: () => ({
    visitors: [],
    reviews: [],
    selectedVisitor: null,
    isLoading: false,
    error: null,
    filter: {
      searchQuery: '',
      visitDateFrom: null,
      visitDateTo: null
    },
    sort: {
      field: 'visitDate',
      direction: 'desc'
    }
  }),

  mutations: {
    SET_VISITORS(state, visitors) {
      state.visitors = visitors;
    },

    ADD_VISITOR(state, visitor) {
      if (!visitor.id) {
        visitor.id = Date.now();
      }
      visitor.createdAt = new Date().toISOString();
      state.visitors.push(visitor);
    },

    UPDATE_VISITOR(state, updatedVisitor) {
      const index = state.visitors.findIndex(v => v.id === updatedVisitor.id);
      if (index !== -1) {
        updatedVisitor.updatedAt = new Date().toISOString();
        state.visitors.splice(index, 1, { ...state.visitors[index], ...updatedVisitor });
      }
    },

    DELETE_VISITOR(state, id) {
      state.visitors = state.visitors.filter(v => v.id !== id);
    },

    SET_REVIEWS(state, reviews) {
      state.reviews = reviews;
    },

    ADD_REVIEW(state, review) {
      if (!review.id) {
        review.id = Date.now();
      }
      review.createdAt = new Date().toISOString();
      state.reviews.push(review);
    },

    UPDATE_REVIEW(state, updatedReview) {
      const index = state.reviews.findIndex(r => r.id === updatedReview.id);
      if (index !== -1) {
        state.reviews.splice(index, 1, { ...state.reviews[index], ...updatedReview });
      }
    },

    DELETE_REVIEW(state, id) {
      state.reviews = state.reviews.filter(r => r.id !== id);
    },

    SET_SELECTED_VISITOR(state, visitor) {
      state.selectedVisitor = visitor;
    },

    SET_LOADING(state, isLoading) {
      state.isLoading = isLoading;
    },

    SET_ERROR(state, error) {
      state.error = error;
    },

    CLEAR_ERROR(state) {
      state.error = null;
    },

    SET_FILTER(state, filter) {
      state.filter = { ...state.filter, ...filter };
    },

    SET_SORT(state, { field, direction }) {
      state.sort.field = field;
      state.sort.direction = direction;
    }
  },

  actions: {
    async fetchVisitors({ commit, dispatch }) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const response = await visitorAPI.getAll();
        // API returns { success: true, data: { items: [...], totalCount: N } }
        let visitors = [];
        if (response.data?.success && response.data?.data) {
          const data = response.data.data;
          visitors = Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
        } else if (Array.isArray(response.data)) {
          visitors = response.data;
        }
        
        commit('SET_VISITORS', visitors);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(visitors));
        
        return visitors;
      } catch (error) {
        console.warn('API unavailable, loading from localStorage:', error.message);
        dispatch('loadFromLocalStorage');
        return [];
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createVisitor({ commit, dispatch }, visitor) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let newVisitor;
        try {
          const response = await visitorAPI.create(visitor);
          newVisitor = response.data;
        } catch (apiError) {
          console.warn('API unavailable, creating visitor locally');
          newVisitor = { ...visitor };
        }
        
        commit('ADD_VISITOR', newVisitor);
        dispatch('saveToLocalStorage');
        
        return newVisitor;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async updateVisitor({ commit, dispatch }, visitor) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        commit('UPDATE_VISITOR', visitor);
        dispatch('saveToLocalStorage');
        
        return visitor;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async deleteVisitor({ commit, dispatch }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        commit('DELETE_VISITOR', id);
        dispatch('saveToLocalStorage');
        
        return { success: true };
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createReview({ commit, dispatch }, review) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        let newReview;
        try {
          const response = await visitorAPI.createReview(review);
          newReview = response.data;
        } catch (apiError) {
          console.warn('API unavailable, creating review locally');
          newReview = { ...review };
        }
        
        commit('ADD_REVIEW', newReview);
        dispatch('saveReviewsToLocalStorage');
        
        return newReview;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    loadFromLocalStorage({ commit, dispatch }) {
      try {
        const storedVisitors = localStorage.getItem(STORAGE_KEY);
        if (storedVisitors) {
          commit('SET_VISITORS', JSON.parse(storedVisitors));
        } else {
          // Seed initial visitors data
          const initialVisitors = [
            { id: 1, name: 'John Smith', email: 'john@email.com', phone: '+1 555-0123', isMember: true, membershipType: 'Premium', visitCount: 45, lastVisit: '2024-01-20', createdAt: new Date().toISOString() },
            { id: 2, name: 'Sarah Johnson', email: 'sarah@email.com', phone: '+1 555-0456', isMember: true, membershipType: 'Standard', visitCount: 28, lastVisit: '2024-01-19', createdAt: new Date().toISOString() },
            { id: 3, name: 'Mike Brown', email: 'mike@email.com', phone: '+1 555-0789', isMember: false, membershipType: null, visitCount: 3, lastVisit: '2024-01-18', createdAt: new Date().toISOString() },
            { id: 4, name: 'Emily Davis', email: 'emily@email.com', phone: '+1 555-0321', isMember: true, membershipType: 'Premium', visitCount: 67, lastVisit: '2024-01-20', createdAt: new Date().toISOString() },
            { id: 5, name: 'Robert Wilson', email: 'robert@email.com', phone: '+1 555-0654', isMember: false, membershipType: null, visitCount: 1, lastVisit: '2024-01-15', createdAt: new Date().toISOString() }
          ];
          commit('SET_VISITORS', initialVisitors);
          dispatch('saveToLocalStorage');
          if (import.meta.env.DEV) {
            console.log(`📦 Seeded ${initialVisitors.length} initial visitors`);
          }
        }

        const storedReviews = localStorage.getItem(REVIEWS_KEY);
        if (storedReviews) {
          commit('SET_REVIEWS', JSON.parse(storedReviews));
        }
      } catch (error) {
        console.error('Load visitors from LocalStorage error:', error);
      }
    },

    saveToLocalStorage({ state }) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state.visitors));
    },

    saveReviewsToLocalStorage({ state }) {
      localStorage.setItem(REVIEWS_KEY, JSON.stringify(state.reviews));
    },

    clearVisitors({ commit }) {
      commit('SET_VISITORS', []);
      commit('SET_REVIEWS', []);
      localStorage.removeItem(STORAGE_KEY);
      localStorage.removeItem(REVIEWS_KEY);
    }
  },

  getters: {
    visitorById: (state) => (id) => {
      return state.visitors.find(v => v.id === parseInt(id));
    },

    filteredVisitors: (state) => {
      let result = [...state.visitors];
      const { filter } = state;

      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(v =>
          v.name?.toLowerCase().includes(query) ||
          v.email?.toLowerCase().includes(query)
        );
      }

      if (filter.visitDateFrom) {
        result = result.filter(v => new Date(v.visitDate) >= new Date(filter.visitDateFrom));
      }

      if (filter.visitDateTo) {
        result = result.filter(v => new Date(v.visitDate) <= new Date(filter.visitDateTo));
      }

      return result;
    },

    reviewsByVisitor: (state) => (visitorId) => {
      return state.reviews.filter(r => r.visitorId === visitorId);
    },

    reviewsByExhibition: (state) => (exhibitionId) => {
      return state.reviews.filter(r => r.exhibitionId === exhibitionId);
    },

    averageRating: (state) => (exhibitionId) => {
      const reviews = state.reviews.filter(r => r.exhibitionId === exhibitionId);
      if (reviews.length === 0) return 0;
      return reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length;
    },

    visitorCount: (state) => state.visitors.length,
    reviewCount: (state) => state.reviews.length,
    isLoading: (state) => state.isLoading,
    error: (state) => state.error
  }
};
