/**
 * Reports API Service
 * Art Gallery Management System
 * 
 * Provides API methods for BI reports and analytics.
 * Ready for .NET 10 backend integration.
 */

import apiClient from './client';

/**
 * Reports API endpoints
 */
export const reportsAPI = {
  /**
   * Get top artists by insured value
   * @param {Object} params - Query parameters (limit, dateFrom, dateTo)
   * @returns {Promise} Axios response promise with artist value data
   */
  getTopArtistsByValue: (params = {}) => {
    return apiClient.get('/reports/artists/value', { params });
  },

  /**
   * Get exhibition performance trends
   * @param {Object} params - Query parameters (exhibitorId, dateFrom, dateTo)
   * @returns {Promise} Axios response promise with trend data
   */
  getExhibitionTrends: (params = {}) => {
    return apiClient.get('/reports/exhibitions/trends', { params });
  },

  /**
   * Get insurance coverage statistics
   * @returns {Promise} Axios response promise with coverage data
   */
  getInsuranceCoverage: () => {
    return apiClient.get('/reports/insurance/coverage');
  },

  /**
   * Get collection composition by nationality
   * @param {Object} params - Query parameters (collectionId)
   * @returns {Promise} Axios response promise with composition data
   */
  getCollectionComposition: (params = {}) => {
    return apiClient.get('/reports/collections/composition', { params });
  },

  /**
   * Get loan status dashboard data
   * @returns {Promise} Axios response promise with loan KPIs
   */
  getLoanStatus: () => {
    return apiClient.get('/reports/loans/status');
  },

  /**
   * Get all reports summary
   * @returns {Promise} Axios response promise with all report data
   */
  getAllReports: () => {
    return apiClient.get('/reports/summary');
  },

  /**
   * Get dashboard KPIs
   * @returns {Promise} Axios response promise with KPI data
   */
  getDashboardKPIs: () => {
    return apiClient.get('/reports/dashboard/kpis');
  },

  /**
   * Export report data
   * @param {string} reportType - Report type to export
   * @param {string} format - Export format (csv, excel, pdf)
   * @returns {Promise} Axios response promise with export data
   */
  exportReport: (reportType, format = 'csv') => {
    return apiClient.get(`/reports/export/${reportType}`, {
      params: { format },
      responseType: 'blob'
    });
  },

  /**
   * Get revenue by time period
   * @param {Object} params - Query parameters (period: daily, weekly, monthly, yearly)
   * @returns {Promise} Axios response promise with revenue data
   */
  getRevenueByPeriod: (params = {}) => {
    return apiClient.get('/reports/revenue', { params });
  },

  /**
   * Get visitor analytics
   * @param {Object} params - Query parameters
   * @returns {Promise} Axios response promise with visitor analytics
   */
  getVisitorAnalytics: (params = {}) => {
    return apiClient.get('/reports/visitors/analytics', { params });
  }
};

export default reportsAPI;
