/**
 * Loans Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages loan and restoration data.
 * This module is namespaced as 'loans'.
 */

const LOANS_KEY = 'artGallery_loans';
const RESTORATIONS_KEY = 'artGallery_restorations';
const INSURANCE_KEY = 'artGallery_insurances';

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
      status: null, // 'active', 'returned', 'overdue'
      loanDateFrom: null,
      loanDateTo: null
    }
  }),

  mutations: {
    SET_LOANS(state, loans) {
      state.loans = loans;
    },

    ADD_LOAN(state, loan) {
      if (!loan.id) {
        loan.id = Date.now();
      }
      loan.createdAt = new Date().toISOString();
      state.loans.push(loan);
    },

    UPDATE_LOAN(state, updatedLoan) {
      const index = state.loans.findIndex(l => l.id === updatedLoan.id);
      if (index !== -1) {
        updatedLoan.updatedAt = new Date().toISOString();
        state.loans.splice(index, 1, { ...state.loans[index], ...updatedLoan });
      }
    },

    DELETE_LOAN(state, id) {
      state.loans = state.loans.filter(l => l.id !== id);
    },

    SET_RESTORATIONS(state, restorations) {
      state.restorations = restorations;
    },

    ADD_RESTORATION(state, restoration) {
      if (!restoration.id) {
        restoration.id = Date.now();
      }
      restoration.createdAt = new Date().toISOString();
      state.restorations.push(restoration);
    },

    UPDATE_RESTORATION(state, updatedRestoration) {
      const index = state.restorations.findIndex(r => r.id === updatedRestoration.id);
      if (index !== -1) {
        state.restorations.splice(index, 1, { ...state.restorations[index], ...updatedRestoration });
      }
    },

    DELETE_RESTORATION(state, id) {
      state.restorations = state.restorations.filter(r => r.id !== id);
    },

    SET_INSURANCES(state, insurances) {
      state.insurances = insurances;
    },

    ADD_INSURANCE(state, insurance) {
      if (!insurance.id) {
        insurance.id = Date.now();
      }
      insurance.createdAt = new Date().toISOString();
      state.insurances.push(insurance);
    },

    UPDATE_INSURANCE(state, updatedInsurance) {
      const index = state.insurances.findIndex(i => i.id === updatedInsurance.id);
      if (index !== -1) {
        state.insurances.splice(index, 1, { ...state.insurances[index], ...updatedInsurance });
      }
    },

    DELETE_INSURANCE(state, id) {
      state.insurances = state.insurances.filter(i => i.id !== id);
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
        
        const stored = localStorage.getItem(LOANS_KEY);
        const loans = stored ? JSON.parse(stored) : [];
        commit('SET_LOANS', loans);
        
        return loans;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createLoan({ commit, dispatch }, loan) {
      try {
        commit('SET_LOADING', true);
        commit('ADD_LOAN', loan);
        dispatch('saveLoansToLocalStorage');
        
        return loan;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async updateLoan({ commit, dispatch }, loan) {
      try {
        commit('SET_LOADING', true);
        commit('UPDATE_LOAN', loan);
        dispatch('saveLoansToLocalStorage');
        
        return loan;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async deleteLoan({ commit, dispatch }, id) {
      try {
        commit('SET_LOADING', true);
        commit('DELETE_LOAN', id);
        dispatch('saveLoansToLocalStorage');
        
        return { success: true };
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createRestoration({ commit, dispatch }, restoration) {
      try {
        commit('SET_LOADING', true);
        commit('ADD_RESTORATION', restoration);
        dispatch('saveRestorationsToLocalStorage');
        
        return restoration;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    async createInsurance({ commit, dispatch }, insurance) {
      try {
        commit('SET_LOADING', true);
        commit('ADD_INSURANCE', insurance);
        dispatch('saveInsurancesToLocalStorage');
        
        return insurance;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    loadFromLocalStorage({ commit, dispatch }) {
      try {
        const storedLoans = localStorage.getItem(LOANS_KEY);
        if (storedLoans) {
          commit('SET_LOANS', JSON.parse(storedLoans));
        } else {
          // Seed initial loans data
          const initialLoans = [
            { id: 1, artworkId: 8, artworkTitle: 'Water Lilies', artist: 'Claude Monet', borrowerName: 'Metropolitan Museum', contactPerson: 'Dr. Jane Wilson', contactEmail: 'jane@metmuseum.org', startDate: '2024-01-15', endDate: '2024-06-15', status: 'Active', insuredValue: 2500000, insuranceProvider: 'ArtGuard', loanFee: 50000, daysRemaining: 145, createdAt: new Date().toISOString() },
            { id: 2, artworkId: 5, artworkTitle: 'The Thinker', artist: 'Auguste Rodin', borrowerName: 'Louvre Museum', contactPerson: 'Pierre Dubois', contactEmail: 'pierre@louvre.fr', startDate: '2024-02-01', endDate: '2024-08-01', status: 'Active', insuredValue: 3000000, insuranceProvider: 'MasterArt Insurance', loanFee: 75000, daysRemaining: 192, createdAt: new Date().toISOString() },
            { id: 3, artworkId: 3, artworkTitle: 'Girl with Pearl Earring', artist: 'Johannes Vermeer', borrowerName: 'National Gallery', contactPerson: 'Emma Thompson', contactEmail: 'emma@nationalgallery.uk', startDate: '2024-03-01', endDate: '2024-05-01', status: 'Pending', insuredValue: 1500000, insuranceProvider: 'Heritage Protect', loanFee: 35000, daysRemaining: 100, createdAt: new Date().toISOString() },
            { id: 4, artworkId: 1, artworkTitle: 'Starry Night', artist: 'Vincent van Gogh', borrowerName: 'MoMA', contactPerson: 'John Davis', contactEmail: 'john@moma.org', startDate: '2023-06-01', endDate: '2023-12-01', status: 'Returned', insuredValue: 2000000, insuranceProvider: 'ArtGuard', loanFee: 45000, daysRemaining: 0, createdAt: new Date().toISOString() }
          ];
          commit('SET_LOANS', initialLoans);
          dispatch('saveLoansToLocalStorage');
          if (import.meta.env.DEV) {
            console.log(`📦 Seeded ${initialLoans.length} initial loans`);
          }
        }

        const storedRestorations = localStorage.getItem(RESTORATIONS_KEY);
        if (storedRestorations) {
          commit('SET_RESTORATIONS', JSON.parse(storedRestorations));
        }

        const storedInsurances = localStorage.getItem(INSURANCE_KEY);
        if (storedInsurances) {
          commit('SET_INSURANCES', JSON.parse(storedInsurances));
        }
      } catch (error) {
        console.error('Load loans from LocalStorage error:', error);
      }
    },

    saveLoansToLocalStorage({ state }) {
      localStorage.setItem(LOANS_KEY, JSON.stringify(state.loans));
    },

    saveRestorationsToLocalStorage({ state }) {
      localStorage.setItem(RESTORATIONS_KEY, JSON.stringify(state.restorations));
    },

    saveInsurancesToLocalStorage({ state }) {
      localStorage.setItem(INSURANCE_KEY, JSON.stringify(state.insurances));
    },

    clearLoans({ commit }) {
      commit('SET_LOANS', []);
      commit('SET_RESTORATIONS', []);
      commit('SET_INSURANCES', []);
      localStorage.removeItem(LOANS_KEY);
      localStorage.removeItem(RESTORATIONS_KEY);
      localStorage.removeItem(INSURANCE_KEY);
    }
  },

  getters: {
    loanById: (state) => (id) => {
      return state.loans.find(l => l.id === parseInt(id));
    },

    getLoanStatus: () => (loan) => {
      const now = new Date();
      const dueDate = new Date(loan.dueDate);
      
      if (loan.returnedDate) return 'returned';
      if (now > dueDate) return 'overdue';
      return 'active';
    },

    filteredLoans: (state, getters) => {
      let result = [...state.loans];
      const { filter } = state;

      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        result = result.filter(l =>
          l.borrowerName?.toLowerCase().includes(query) ||
          l.artworkTitle?.toLowerCase().includes(query)
        );
      }

      if (filter.status) {
        result = result.filter(l => getters.getLoanStatus(l) === filter.status);
      }

      if (filter.loanDateFrom) {
        result = result.filter(l => new Date(l.loanDate) >= new Date(filter.loanDateFrom));
      }

      if (filter.loanDateTo) {
        result = result.filter(l => new Date(l.loanDate) <= new Date(filter.loanDateTo));
      }

      return result;
    },

    activeLoans: (state, getters) => {
      return state.loans.filter(l => getters.getLoanStatus(l) === 'active');
    },

    overdueLoans: (state, getters) => {
      return state.loans.filter(l => getters.getLoanStatus(l) === 'overdue');
    },

    returnedLoans: (state, getters) => {
      return state.loans.filter(l => getters.getLoanStatus(l) === 'returned');
    },

    loansByArtwork: (state) => (artworkId) => {
      return state.loans.filter(l => l.artworkId === artworkId);
    },

    restorationsByArtwork: (state) => (artworkId) => {
      return state.restorations.filter(r => r.artworkId === artworkId);
    },

    insuranceByArtwork: (state) => (artworkId) => {
      return state.insurances.find(i => i.artworkId === artworkId);
    },

    totalInsuredValue: (state) => {
      return state.insurances.reduce((sum, i) => sum + (i.insuredAmount || 0), 0);
    },

    activeRestorations: (state) => {
      return state.restorations.filter(r => !r.completedDate);
    },

    loanCount: (state) => state.loans.length,
    restorationCount: (state) => state.restorations.length,
    insuranceCount: (state) => state.insurances.length,
    isLoading: (state) => state.isLoading,
    error: (state) => state.error
  }
};
