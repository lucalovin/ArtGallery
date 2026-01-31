/**
 * Insurance API Service
 * Art Gallery Management System
 * 
 * Provides API methods for managing insurance policies.
 * 
 * Endpoints:
 * - GET    /api/insurance          - Get all policies (paginated)
 * - GET    /api/insurance/:id      - Get policy by ID
 * - POST   /api/insurance          - Create new policy
 * - PUT    /api/insurance/:id      - Update policy
 * - DELETE /api/insurance/:id      - Delete policy
 * - GET    /api/insurance/active   - Get active policies
 * - GET    /api/insurance/expiring - Get expiring policies
 * - GET    /api/insurance/statistics - Get statistics
 */

import apiClient from './client';

/**
 * Insurance API endpoints
 */
export const insuranceAPI = {
  /**
   * Get all insurance policies with pagination
   * @param {Object} params - Pagination parameters
   * @returns {Promise} Axios response promise
   */
  getAll(params = {}) {
    return apiClient.get('/insurance', { params });
  },

  /**
   * Get insurance policy by ID
   * @param {number} id - Policy ID
   * @returns {Promise} Axios response promise
   */
  getById(id) {
    return apiClient.get(`/insurance/${id}`);
  },

  /**
   * Create a new insurance policy
   * @param {Object} policy - Policy data
   * @returns {Promise} Axios response promise
   */
  create(policy) {
    return apiClient.post('/insurance', policy);
  },

  /**
   * Update an existing insurance policy
   * @param {number} id - Policy ID
   * @param {Object} policy - Updated policy data
   * @returns {Promise} Axios response promise
   */
  update(id, policy) {
    return apiClient.put(`/insurance/${id}`, policy);
  },

  /**
   * Delete an insurance policy
   * @param {number} id - Policy ID
   * @returns {Promise} Axios response promise
   */
  delete(id) {
    return apiClient.delete(`/insurance/${id}`);
  },

  /**
   * Get active insurance policies
   * @returns {Promise} Axios response promise
   */
  getActive() {
    return apiClient.get('/insurance/active');
  },

  /**
   * Get expiring insurance policies
   * @param {number} days - Days until expiration (default: 30)
   * @returns {Promise} Axios response promise
   */
  getExpiring(days = 30) {
    return apiClient.get('/insurance/expiring', { params: { days } });
  },

  /**
   * Get insurance statistics
   * @returns {Promise} Axios response promise
   */
  getStatistics() {
    return apiClient.get('/insurance/statistics');
  }
};

export default insuranceAPI;
