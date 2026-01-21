/**
 * ETL API Service
 * Art Gallery Management System
 * 
 * Provides API methods for ETL and Data Warehouse operations.
 * Ready for .NET 10 backend integration.
 */

import apiClient from './client';

/**
 * ETL API endpoints
 */
export const etlAPI = {
  /**
   * Trigger ETL process to refresh Data Warehouse
   * @returns {Promise} Axios response promise with operation details
   */
  triggerRefresh: () => {
    return apiClient.post('/etl/refresh');
  },

  /**
   * Get current ETL status
   * @returns {Promise} Axios response promise with status details
   */
  getStatus: () => {
    return apiClient.get('/etl/status');
  },

  /**
   * Get ETL operation history
   * @param {Object} params - Query parameters (limit, offset)
   * @returns {Promise} Axios response promise with operation history
   */
  getHistory: (params = {}) => {
    return apiClient.get('/etl/history', { params });
  },

  /**
   * Get specific ETL operation by ID
   * @param {number|string} operationId - ETL operation ID
   * @returns {Promise} Axios response promise with operation details
   */
  getOperation: (operationId) => {
    return apiClient.get(`/etl/operations/${operationId}`);
  },

  /**
   * Get Data Warehouse validation report
   * @returns {Promise} Axios response promise with validation data
   */
  getValidationReport: () => {
    return apiClient.get('/etl/validation');
  },

  /**
   * Get before/after comparison for last ETL
   * @returns {Promise} Axios response promise with comparison data
   */
  getComparison: () => {
    return apiClient.get('/etl/comparison');
  },

  /**
   * Get DW dimension data
   * @param {string} dimension - Dimension name (artwork, artist, exhibition, location, date)
   * @returns {Promise} Axios response promise with dimension data
   */
  getDimension: (dimension) => {
    return apiClient.get(`/dw/dimensions/${dimension}`);
  },

  /**
   * Get DW fact table data
   * @param {Object} params - Query parameters for filtering
   * @returns {Promise} Axios response promise with fact data
   */
  getFactData: (params = {}) => {
    return apiClient.get('/dw/facts/exhibition-activity', { params });
  },

  /**
   * Get DW statistics
   * @returns {Promise} Axios response promise with DW statistics
   */
  getDWStatistics: () => {
    return apiClient.get('/dw/statistics');
  },

  /**
   * Cancel ongoing ETL operation
   * @param {number|string} operationId - ETL operation ID to cancel
   * @returns {Promise} Axios response promise
   */
  cancelOperation: (operationId) => {
    return apiClient.post(`/etl/operations/${operationId}/cancel`);
  }
};

export default etlAPI;
