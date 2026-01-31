/**
 * ETL API Service
 * Art Gallery Management System
 * 
 * Provides API methods for ETL and Data Warehouse operations.
 * Matches backend EtlController endpoints.
 */

import apiClient from './client';

/**
 * ETL API endpoints
 */
export const etlAPI = {
  /**
   * Trigger ETL sync operation (maps to POST /api/etl/sync)
   * @param {Object} dto - Sync configuration { syncType: 'full'|'incremental', sourceTables: [] }
   * @returns {Promise} Axios response promise with operation details
   */
  triggerRefresh: (dto = { syncType: 'full', sourceTables: [] }) => {
    return apiClient.post('/etl/sync', dto);
  },

  /**
   * Run OLTP to DW propagation using Oracle PL/SQL procedures
   * @param {Object} request - { source: 'OLTP', target: 'DW', operation: 'full_load'|'incremental' }
   * @returns {Promise} Axios response promise with propagation result
   */
  runPropagation: (request = { source: 'OLTP', target: 'DW', operation: 'full_load' }) => {
    return apiClient.post('/etl/run-propagation', request);
  },

  /**
   * Get current ETL status (maps to GET /api/etl/status)
   * @returns {Promise} Axios response promise with status details
   */
  getStatus: () => {
    return apiClient.get('/etl/status');
  },

  /**
   * Get ETL sync history (maps to GET /api/etl/syncs)
   * @param {Object} params - Query parameters { pageNumber, pageSize }
   * @returns {Promise} Axios response promise with sync history
   */
  getHistory: (params = {}) => {
    return apiClient.get('/etl/syncs', { params });
  },

  /**
   * Get specific ETL sync record by ID (maps to GET /api/etl/syncs/{id})
   * @param {number|string} syncId - ETL sync record ID
   * @returns {Promise} Axios response promise with sync details
   */
  getOperation: (syncId) => {
    return apiClient.get(`/etl/syncs/${syncId}`);
  },

  /**
   * Get ETL field mappings configuration (maps to GET /api/etl/mappings)
   * @returns {Promise} Axios response promise with mapping data
   */
  getMappings: () => {
    return apiClient.get('/etl/mappings');
  },

  /**
   * Update ETL field mappings (maps to PUT /api/etl/mappings)
   * @param {Array} mappings - Array of mapping configurations
   * @returns {Promise} Axios response promise
   */
  updateMappings: (mappings) => {
    return apiClient.put('/etl/mappings', mappings);
  },

  /**
   * Get ETL statistics (maps to GET /api/etl/statistics)
   * @returns {Promise} Axios response promise with statistics data
   */
  getStatistics: () => {
    return apiClient.get('/etl/statistics');
  },

  /**
   * Validate data consistency between OLTP and DW (maps to POST /api/etl/validate)
   * @returns {Promise} Axios response promise with validation result
   */
  validateConsistency: () => {
    return apiClient.post('/etl/validate');
  },

  /**
   * Validate referential integrity in DW (maps to POST /api/etl/validate-integrity)
   * @returns {Promise} Axios response promise with integrity result
   */
  validateIntegrity: () => {
    return apiClient.post('/etl/validate-integrity');
  },

  /**
   * Get Data Warehouse validation report (combines validate endpoints)
   * @returns {Promise} Axios response promise with validation data
   */
  getValidationReport: async () => {
    const [consistency, integrity] = await Promise.all([
      apiClient.post('/etl/validate'),
      apiClient.post('/etl/validate-integrity')
    ]);
    return {
      data: {
        success: true,
        data: {
          consistencyValid: consistency.data?.data,
          integrityResult: integrity.data?.data
        }
      }
    };
  },

  /**
   * Get before/after comparison for last ETL
   * @returns {Promise} Axios response promise with comparison data
   */
  getComparison: () => {
    return apiClient.get('/etl/statistics');
  }
};

export default etlAPI;
