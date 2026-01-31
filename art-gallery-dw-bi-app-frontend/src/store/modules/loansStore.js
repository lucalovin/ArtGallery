/**
 * Loans Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages loan and restoration data using real API.
 * This module is namespaced as 'loans'.
 */

import { loansAPI } from '@/api/loansAPI';

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
        commit('SET_LOANS', loans);
        
        return loans;
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
        commit('SET_SELECTED_LOAN', loan);
        
        return loan;
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
        commit('ADD_LOAN', createdLoan);
        
        return createdLoan;
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
        commit('UPDATE_LOAN', updatedLoan);
        
        return updatedLoan;
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
