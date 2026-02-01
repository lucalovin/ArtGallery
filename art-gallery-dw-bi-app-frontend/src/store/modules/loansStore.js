/**
 * Loans Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages loan and restoration data using real API.
 * This module is namespaced as 'loans'.
 */

import { loansAPI } from '@/api/loansAPI';

/**
 * Computes loan status based on dates
 * @param {Date} startDate - Loan start date
 * @param {Date|null} endDate - Loan end date (optional)
 * @returns {string} Status: 'Pending', 'Active', or 'Returned'
 */
function computeLoanStatus(startDate, endDate) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  const start = new Date(startDate);
  start.setHours(0, 0, 0, 0);
  
  if (today < start) {
    return 'Pending';
  }
  
  if (endDate) {
    const end = new Date(endDate);
    end.setHours(0, 0, 0, 0);
    if (today > end) {
      return 'Returned';
    }
  }
  
  return 'Active';
}

/**
 * Computes days remaining until loan ends
 * @param {Date|null} endDate - Loan end date
 * @returns {number|null} Days remaining or null if no end date
 */
function computeDaysRemaining(endDate) {
  if (!endDate) return null;
  
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  const end = new Date(endDate);
  end.setHours(0, 0, 0, 0);
  
  const diffMs = end - today;
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
  
  return diffDays >= 0 ? diffDays : 0;
}

/**
 * Enriches a loan object with computed status and daysRemaining
 * @param {Object} loan - Loan object from API
 * @returns {Object} Enriched loan object
 */
function enrichLoan(loan) {
  return {
    ...loan,
    status: loan.status || computeLoanStatus(loan.startDate, loan.endDate),
    daysRemaining: loan.daysRemaining !== undefined ? loan.daysRemaining : computeDaysRemaining(loan.endDate)
  };
}

export default {
  namespaced: true,

  state: () => ({
    loans: [],
    restorations: [],
    insurances: [],
    selectedLoan: null,
    isLoading: false,
    error: null,
    filter: {
      searchQuery: '',
      status: null,
      loanDateFrom: null,
      loanDateTo: null
    }
  }),

  mutations: {
    SET_LOANS(state, loans) {
      state.loans = loans;
    },

    ADD_LOAN(state, loan) {
      state.loans.push(loan);
    },

    UPDATE_LOAN(state, updatedLoan) {
      const index = state.loans.findIndex(l => l.id === updatedLoan.id);
      if (index !== -1) {
        state.loans.splice(index, 1, updatedLoan);
      }
    },

    DELETE_LOAN(state, id) {
      state.loans = state.loans.filter(l => l.id !== id);
    },

    SET_RESTORATIONS(state, restorations) {
      state.restorations = restorations;
    },

    SET_INSURANCES(state, insurances) {
      state.insurances = insurances;
    },

    SET_SELECTED_LOAN(state, loan) {
      state.selectedLoan = loan;
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
    async fetchLoans({ commit }) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const loans = await loansAPI.getAll();
        // Enrich loans with computed status and daysRemaining if not provided by API
        const enrichedLoans = loans.map(loan => enrichLoan(loan));
        commit('SET_LOANS', enrichedLoans);
        
        return enrichedLoans;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async fetchLoanById({ commit }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const loan = await loansAPI.getById(id);
        const enrichedLoan = enrichLoan(loan);
        commit('SET_SELECTED_LOAN', enrichedLoan);
        
        return enrichedLoan;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createLoan({ commit }, loan) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const createdLoan = await loansAPI.create(loan);
        const enrichedLoan = enrichLoan(createdLoan);
        commit('ADD_LOAN', enrichedLoan);
        
        return enrichedLoan;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async updateLoan({ commit }, loan) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        const updatedLoan = await loansAPI.update(loan.id, loan);
        const enrichedLoan = enrichLoan(updatedLoan);
        commit('UPDATE_LOAN', enrichedLoan);
        
        return enrichedLoan;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async deleteLoan({ commit }, id) {
      try {
        commit('SET_LOADING', true);
        commit('CLEAR_ERROR');
        
        await loansAPI.delete(id);
        commit('DELETE_LOAN', id);
        
        return { success: true };
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    clearLoans({ commit }) {
      commit('SET_LOANS', []);
      commit('SET_RESTORATIONS', []);
      commit('SET_INSURANCES', []);
    },

    /**
     * Load loans from LocalStorage (placeholder for compatibility)
     * Since loans are always fetched from API, this is a no-op
     */
    loadFromLocalStorage() {
      // No-op: loans are always fetched from API
      // This action exists for consistency with other stores
    }
  },

  getters: {
    loanById: (state) => (id) => {
      return state.loans.find(l => l.id === parseInt(id));
    },

    filteredLoans: (state) => {
      let result = [...state.loans];
      const { filter } = state;

      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(l =>
          l.exhibitorName?.toLowerCase().includes(query) ||
          l.artworkTitle?.toLowerCase().includes(query)
        );
      }

      if (filter.loanDateFrom) {
        result = result.filter(l => new Date(l.startDate) >= new Date(filter.loanDateFrom));
      }

      if (filter.loanDateTo) {
        result = result.filter(l => new Date(l.startDate) <= new Date(filter.loanDateTo));
      }

      return result;
    },

    loansByArtwork: (state) => (artworkId) => {
      return state.loans.filter(l => l.artworkId === artworkId);
    },

    loanCount: (state) => state.loans.length,
    isLoading: (state) => state.isLoading,
    error: (state) => state.error
  }
};
