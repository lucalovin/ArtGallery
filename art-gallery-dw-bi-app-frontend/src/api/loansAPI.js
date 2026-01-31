/**
 * Loans API Service
 * Art Gallery Management System
 * 
 * Provides API methods for managing loan operations.
 * 
 * Endpoints:
 * - GET    /api/loans          - Get all loans (paginated)
 * - GET    /api/loans/:id      - Get loan by ID
 * - POST   /api/loans          - Create new loan
 * - PUT    /api/loans/:id      - Update loan
 * - DELETE /api/loans/:id      - Delete loan
 * - GET    /api/loans/active   - Get active loans
 * - GET    /api/loans/overdue  - Get overdue loans
 * - GET    /api/loans/statistics - Get loan statistics
 */

import apiClient from './client';

/**
 * Loans API endpoints
 */
export const loansAPI = {
  /**
   * Get all loans with pagination
   * @param {Object} params - Pagination parameters
   * @returns {Promise} Array of loans
   */
  async getAll(params = {}) {
    const response = await apiClient.get('/loans', { params });
    const data = response.data;
    
    // Handle API response format
    if (data?.success && data?.data) {
      return data.data.items || data.data;
    }
    return data || [];
  },

  /**
   * Get loan by ID
   * @param {number} id - Loan ID
   * @returns {Promise} Loan object
   */
  async getById(id) {
    const response = await apiClient.get(`/loans/${id}`);
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data;
  },

  /**
   * Create a new loan
   * @param {Object} loan - Loan data
   * @returns {Promise} Created loan object
   */
  async create(loan) {
    const response = await apiClient.post('/loans', loan);
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data;
  },

  /**
   * Update an existing loan
   * @param {number} id - Loan ID
   * @param {Object} loan - Updated loan data
   * @returns {Promise} Updated loan object
   */
  async update(id, loan) {
    const response = await apiClient.put(`/loans/${id}`, loan);
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data;
  },

  /**
   * Delete a loan
   * @param {number} id - Loan ID
   * @returns {Promise} Void
   */
  async delete(id) {
    await apiClient.delete(`/loans/${id}`);
  },

  /**
   * Get active loans
   * @returns {Promise} Array of active loans
   */
  async getActive() {
    const response = await apiClient.get('/loans/active');
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data || [];
  },

  /**
   * Get overdue loans
   * @returns {Promise} Array of overdue loans
   */
  async getOverdue() {
    const response = await apiClient.get('/loans/overdue');
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data || [];
  },

  /**
   * Get loan statistics
   * @returns {Promise} Loan statistics object
   */
  async getStatistics() {
    const response = await apiClient.get('/loans/statistics');
    const data = response.data;
    
    if (data?.success && data?.data) {
      return data.data;
    }
    return data;
  }
};

export default loansAPI;
