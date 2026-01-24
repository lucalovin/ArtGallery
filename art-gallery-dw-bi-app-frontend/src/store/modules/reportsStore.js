/**
 * Reports Vuex Store Module
 * Art Gallery Management System
 * 
 * Manages BI report data and chart configurations.
 * This module is namespaced as 'reports'.
 */

import { reportsAPI } from '@/api/reportsAPI';

const STORAGE_KEY = 'artGallery_reportData';

export default {
  namespaced: true,

  state: () => ({
    // Report data
    artistValueData: [],
    exhibitionTrendsData: [],
    insuranceCoverageData: {},
    collectionCompositionData: [],
    loanStatusData: {},
    
    // Report configurations
    selectedReport: 'artistValue',
    dateRange: {
      from: null,
      to: null
    },
    filters: {
      artistId: null,
      exhibitorId: null,
      collectionId: null
    },
    
    // Loading and error states
    isLoading: false,
    error: null,
    lastUpdated: null
  }),

  mutations: {
    SET_ARTIST_VALUE_DATA(state, data) {
      state.artistValueData = data;
    },

    SET_EXHIBITION_TRENDS_DATA(state, data) {
      state.exhibitionTrendsData = data;
    },

    SET_INSURANCE_COVERAGE_DATA(state, data) {
      state.insuranceCoverageData = data;
    },

    SET_COLLECTION_COMPOSITION_DATA(state, data) {
      state.collectionCompositionData = data;
    },

    SET_LOAN_STATUS_DATA(state, data) {
      state.loanStatusData = data;
    },

    SET_SELECTED_REPORT(state, reportType) {
      state.selectedReport = reportType;
    },

    SET_DATE_RANGE(state, { from, to }) {
      state.dateRange.from = from;
      state.dateRange.to = to;
    },

    SET_FILTERS(state, filters) {
      state.filters = { ...state.filters, ...filters };
    },

    RESET_FILTERS(state) {
      state.filters = {
        artistId: null,
        exhibitorId: null,
        collectionId: null
      };
      state.dateRange = {
        from: null,
        to: null
      };
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

    SET_LAST_UPDATED(state, timestamp) {
      state.lastUpdated = timestamp;
    }
  },

  actions: {
    /**
     * Fetch all report data
     */
    async fetchReportData({ dispatch }) {
      await Promise.all([
        dispatch('fetchArtistValueData'),
        dispatch('fetchExhibitionTrendsData'),
        dispatch('fetchInsuranceCoverageData'),
        dispatch('fetchCollectionCompositionData'),
        dispatch('fetchLoanStatusData')
      ]);
    },

    /**
     * Fetch artist value chart data
     */
    async fetchArtistValueData({ commit, rootState }) {
      try {
        commit('SET_LOADING', true);
        
        // Calculate from artwork data
        const artworks = rootState.artwork?.artworks || [];
        
        // Group by artist and sum values
        const artistValues = {};
        artworks.forEach(artwork => {
          const artistId = artwork.artistId;
          if (!artistValues[artistId]) {
            artistValues[artistId] = {
              artistId,
              artistName: artwork.artistName || `Artist ${artistId}`,
              totalValue: 0,
              artworkCount: 0
            };
          }
          artistValues[artistId].totalValue += artwork.estimatedValue || 0;
          artistValues[artistId].artworkCount += 1;
        });

        // Convert to array and sort by value
        const data = Object.values(artistValues)
          .sort((a, b) => b.totalValue - a.totalValue)
          .slice(0, 10); // Top 10 artists

        commit('SET_ARTIST_VALUE_DATA', data);
        commit('SET_LAST_UPDATED', new Date().toISOString());
        
        return data;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Fetch exhibition trends data
     */
    async fetchExhibitionTrendsData({ commit, rootState }) {
      try {
        commit('SET_LOADING', true);
        
        const exhibitions = rootState.exhibition?.exhibitions || [];
        
        // Group by month and exhibitor
        const trendsByMonth = {};
        exhibitions.forEach(exhibition => {
          const date = new Date(exhibition.startDate);
          const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
          
          if (!trendsByMonth[monthKey]) {
            trendsByMonth[monthKey] = {
              month: monthKey,
              exhibitions: [],
              totalVisitors: 0,
              averageRating: 0
            };
          }
          
          trendsByMonth[monthKey].exhibitions.push(exhibition);
          trendsByMonth[monthKey].totalVisitors += exhibition.visitorCount || 0;
        });

        const data = Object.values(trendsByMonth).sort((a, b) => a.month.localeCompare(b.month));
        
        commit('SET_EXHIBITION_TRENDS_DATA', data);
        
        return data;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Fetch insurance coverage data
     */
    async fetchInsuranceCoverageData({ commit, rootState }) {
      try {
        commit('SET_LOADING', true);
        
        const artworks = rootState.artwork?.artworks || [];
        const insurances = rootState.loans?.insurances || [];
        
        // Create a set of insured artwork IDs
        const insuredArtworkIds = new Set(insurances.map(i => i.artworkId));
        
        // Calculate coverage statistics
        const data = {
          covered: 0,
          partial: 0,
          uncovered: 0,
          totalValue: 0,
          coveredValue: 0
        };

        artworks.forEach(artwork => {
          const value = artwork.estimatedValue || 0;
          data.totalValue += value;
          
          if (insuredArtworkIds.has(artwork.id)) {
            data.covered += 1;
            data.coveredValue += value;
          } else {
            data.uncovered += 1;
          }
        });

        commit('SET_INSURANCE_COVERAGE_DATA', data);
        
        return data;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Fetch collection composition data
     */
    async fetchCollectionCompositionData({ commit, rootState }) {
      try {
        commit('SET_LOADING', true);
        
        const artworks = rootState.artwork?.artworks || [];
        
        // Group by collection and nationality
        const compositionByCollection = {};
        artworks.forEach(artwork => {
          const collectionId = artwork.collectionId || 'unassigned';
          const nationality = artwork.artistNationality || 'Unknown';
          
          if (!compositionByCollection[collectionId]) {
            compositionByCollection[collectionId] = {
              collectionId,
              collectionName: artwork.collectionName || `Collection ${collectionId}`,
              nationalities: {}
            };
          }
          
          if (!compositionByCollection[collectionId].nationalities[nationality]) {
            compositionByCollection[collectionId].nationalities[nationality] = 0;
          }
          
          compositionByCollection[collectionId].nationalities[nationality] += 1;
        });

        const data = Object.values(compositionByCollection);
        
        commit('SET_COLLECTION_COMPOSITION_DATA', data);
        
        return data;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Fetch loan status dashboard data
     */
    async fetchLoanStatusData({ commit, rootState, rootGetters }) {
      try {
        commit('SET_LOADING', true);
        
        const loans = rootState.loans?.loans || [];
        const restorations = rootState.loans?.restorations || [];
        const insurances = rootState.loans?.insurances || [];
        const exhibitions = rootState.exhibition?.exhibitions || [];
        
        // Calculate KPIs
        const now = new Date();
        
        const data = {
          activeLoans: loans.filter(l => !l.returnedDate && new Date(l.dueDate) > now).length,
          overdueLoans: loans.filter(l => !l.returnedDate && new Date(l.dueDate) <= now).length,
          totalLoans: loans.length,
          activeRestorations: restorations.filter(r => !r.completedDate).length,
          totalRestorations: restorations.length,
          totalInsuredAmount: insurances.reduce((sum, i) => sum + (i.insuredAmount || 0), 0),
          insuredArtworks: insurances.length,
          ongoingExhibitions: exhibitions.filter(e => {
            const start = new Date(e.startDate);
            const end = new Date(e.endDate);
            return now >= start && now <= end;
          }).length,
          upcomingExhibitions: exhibitions.filter(e => new Date(e.startDate) > now).length
        };

        commit('SET_LOAN_STATUS_DATA', data);
        
        return data;
      } catch (error) {
        commit('SET_ERROR', error.message);
        throw error;
      } finally {
        commit('SET_LOADING', false);
      }
    },

    /**
     * Load from LocalStorage
     */
    loadFromLocalStorage({ commit }) {
      try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          const data = JSON.parse(stored);
          if (data.artistValueData) commit('SET_ARTIST_VALUE_DATA', data.artistValueData);
          if (data.exhibitionTrendsData) commit('SET_EXHIBITION_TRENDS_DATA', data.exhibitionTrendsData);
          if (data.insuranceCoverageData) commit('SET_INSURANCE_COVERAGE_DATA', data.insuranceCoverageData);
          if (data.collectionCompositionData) commit('SET_COLLECTION_COMPOSITION_DATA', data.collectionCompositionData);
          if (data.loanStatusData) commit('SET_LOAN_STATUS_DATA', data.loanStatusData);
          if (data.lastUpdated) commit('SET_LAST_UPDATED', data.lastUpdated);
        }
      } catch (error) {
        console.error('Load reports from LocalStorage error:', error);
      }
    },

    /**
     * Save to LocalStorage
     */
    saveToLocalStorage({ state }) {
      try {
        const data = {
          artistValueData: state.artistValueData,
          exhibitionTrendsData: state.exhibitionTrendsData,
          insuranceCoverageData: state.insuranceCoverageData,
          collectionCompositionData: state.collectionCompositionData,
          loanStatusData: state.loanStatusData,
          lastUpdated: state.lastUpdated
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
      } catch (error) {
        console.error('Save reports to LocalStorage error:', error);
      }
    },

    /**
     * Clear all report data
     */
    clearReports({ commit }) {
      commit('SET_ARTIST_VALUE_DATA', []);
      commit('SET_EXHIBITION_TRENDS_DATA', []);
      commit('SET_INSURANCE_COVERAGE_DATA', {});
      commit('SET_COLLECTION_COMPOSITION_DATA', []);
      commit('SET_LOAN_STATUS_DATA', {});
      commit('SET_LAST_UPDATED', null);
      localStorage.removeItem(STORAGE_KEY);
    },

    /**
     * Select report type
     */
    selectReport({ commit }, reportType) {
      commit('SET_SELECTED_REPORT', reportType);
    },

    /**
     * Apply date range filter
     */
    applyDateRange({ commit }, dateRange) {
      commit('SET_DATE_RANGE', dateRange);
    },

    /**
     * Apply filters
     */
    applyFilters({ commit }, filters) {
      commit('SET_FILTERS', filters);
    },

    /**
     * Reset all filters
     */
    resetFilters({ commit }) {
      commit('RESET_FILTERS');
    }
  },

  getters: {
    /**
     * Get top artists by value (for bar chart)
     */
    topArtistsByValue: (state) => (limit = 10) => {
      return state.artistValueData.slice(0, limit);
    },

    /**
     * Get exhibition trends (for line chart)
     */
    exhibitionTrends: (state) => {
      return state.exhibitionTrendsData;
    },

    /**
     * Get insurance coverage percentages (for pie chart)
     */
    insuranceCoveragePercentages: (state) => {
      const { covered, uncovered, partial } = state.insuranceCoverageData;
      const total = (covered || 0) + (uncovered || 0) + (partial || 0);
      
      if (total === 0) {
        return { covered: 0, uncovered: 0, partial: 0 };
      }
      
      return {
        covered: Math.round(((covered || 0) / total) * 100),
        uncovered: Math.round(((uncovered || 0) / total) * 100),
        partial: Math.round(((partial || 0) / total) * 100)
      };
    },

    /**
     * Get collection composition (for stacked bar chart)
     */
    collectionComposition: (state) => {
      return state.collectionCompositionData;
    },

    /**
     * Get loan status KPIs
     */
    loanStatusKPIs: (state) => {
      return state.loanStatusData;
    },

    /**
     * Get currently selected report type
     */
    selectedReport: (state) => state.selectedReport,

    /**
     * Get date range filter
     */
    dateRange: (state) => state.dateRange,

    /**
     * Get active filters
     */
    activeFilters: (state) => state.filters,

    /**
     * Get last updated timestamp
     */
    lastUpdated: (state) => state.lastUpdated,

    /**
     * Get formatted last updated
     */
    formattedLastUpdated: (state) => {
      if (!state.lastUpdated) return 'Never';
      return new Date(state.lastUpdated).toLocaleString();
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
