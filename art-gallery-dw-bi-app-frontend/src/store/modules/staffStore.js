/**
 * Staff Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages staff data and activities.
 * This module is namespaced as 'staff'.
 */

const STORAGE_KEY = 'artGallery_staff';

export default {
  namespaced: true,

  state: () => ({
    staff: [],
    activities: [],
    selectedStaff: null,
    isLoading: false,
    error: null,
    filter: {
      searchQuery: '',
      role: null,
      department: null
    }
  }),

  mutations: {
    SET_STAFF(state, staff) {
      state.staff = staff;
    },

    ADD_STAFF(state, staffMember) {
      if (!staffMember.id) {
        staffMember.id = Date.now();
      }
      staffMember.createdAt = new Date().toISOString();
      state.staff.push(staffMember);
    },

    UPDATE_STAFF(state, updatedStaff) {
      const index = state.staff.findIndex(s => s.id === updatedStaff.id);
      if (index !== -1) {
        updatedStaff.updatedAt = new Date().toISOString();
        state.staff.splice(index, 1, { ...state.staff[index], ...updatedStaff });
      }
    },

    DELETE_STAFF(state, id) {
      state.staff = state.staff.filter(s => s.id !== id);
    },

    SET_ACTIVITIES(state, activities) {
      state.activities = activities;
    },

    ADD_ACTIVITY(state, activity) {
      if (!activity.id) {
        activity.id = Date.now();
      }
      activity.createdAt = new Date().toISOString();
      state.activities.push(activity);
    },

    SET_SELECTED_STAFF(state, staff) {
      state.selectedStaff = staff;
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
    }
  },

  actions: {
    async fetchStaff({ commit }) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        // Mock API call - replace with actual API
        const stored = localStorage.getItem(STORAGE_KEY);
        const staff = stored ? JSON.parse(stored) : [];
        commit('SET_STAFF', staff);
        
        return staff;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createStaff({ commit, dispatch }, staffMember) {
      try {
        commit('SET_LOADING', true);
        commit('ADD_STAFF', staffMember);
        dispatch('saveToLocalStorage');
        
        return staffMember;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async updateStaff({ commit, dispatch }, staffMember) {
      try {
        commit('SET_LOADING', true);
        commit('UPDATE_STAFF', staffMember);
        dispatch('saveToLocalStorage');
        
        return staffMember;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async deleteStaff({ commit, dispatch }, id) {
      try {
        commit('SET_LOADING', true);
        commit('DELETE_STAFF', id);
        dispatch('saveToLocalStorage');
        
        return { success: true };
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    loadFromLocalStorage({ commit }) {
      try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          commit('SET_STAFF', JSON.parse(stored));
        }
      } catch (error) {
        console.error('Load staff from LocalStorage error:', error);
      }
    },

    saveToLocalStorage({ state }) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state.staff));
    },

    clearStaff({ commit }) {
      commit('SET_STAFF', []);
      commit('SET_ACTIVITIES', []);
      localStorage.removeItem(STORAGE_KEY);
    }
  },

  getters: {
    staffById: (state) => (id) => {
      return state.staff.find(s => s.id === parseInt(id));
    },

    filteredStaff: (state) => {
      let result = [...state.staff];
      const { filter } = state;

      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(s =>
          s.name?.toLowerCase().includes(query) ||
          s.email?.toLowerCase().includes(query)
        );
      }

      if (filter.role) {
        result = result.filter(s => s.role === filter.role);
      }

      if (filter.department) {
        result = result.filter(s => s.department === filter.department);
      }

      return result;
    },

    staffByRole: (state) => (role) => {
      return state.staff.filter(s => s.role === role);
    },

    staffByDepartment: (state) => (department) => {
      return state.staff.filter(s => s.department === department);
    },

    uniqueRoles: (state) => {
      const roles = state.staff.map(s => s.role).filter(Boolean);
      return [...new Set(roles)].sort();
    },

    uniqueDepartments: (state) => {
      const departments = state.staff.map(s => s.department).filter(Boolean);
      return [...new Set(departments)].sort();
    },

    staffCount: (state) => state.staff.length,
    isLoading: (state) => state.isLoading,
    error: (state) => state.error
  }
};
