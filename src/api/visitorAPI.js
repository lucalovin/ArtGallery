/**
 * Visitor API Service
 * Art Gallery Management System
 * 
 * Provides API methods for visitor and review operations.
 * Ready for .NET 10 backend integration.
 */

import apiClient from './client';

/**
 * Visitor API endpoints
 */
export const visitorAPI = {
  /**
   * Get all visitors
   * @param {Object} params - Query parameters
   * @returns {Promise} Axios response promise
   */
  getAll: (params = {}) => {
    return apiClient.get('/visitors', { params });
  },

  /**
   * Get a single visitor by ID
   * @param {number|string} id - Visitor ID
   * @returns {Promise} Axios response promise
   */
  getById: (id) => {
    return apiClient.get(`/visitors/${id}`);
  },

  /**
   * Create a new visitor
   * @param {Object} visitor - Visitor data
   * @returns {Promise} Axios response promise
   */
  create: (visitor) => {
    return apiClient.post('/visitors', visitor);
  },

  /**
   * Update visitor
   * @param {number|string} id - Visitor ID
   * @param {Object} visitor - Updated visitor data
   * @returns {Promise} Axios response promise
   */
  update: (id, visitor) => {
    return apiClient.put(`/visitors/${id}`, visitor);
  },

  /**
   * Delete visitor
   * @param {number|string} id - Visitor ID
   * @returns {Promise} Axios response promise
   */
  delete: (id) => {
    return apiClient.delete(`/visitors/${id}`);
  },

  // ========================================
  // REVIEW ENDPOINTS
  // ========================================

  /**
   * Get all reviews
   * @param {Object} params - Query parameters
   * @returns {Promise} Axios response promise
   */
  getAllReviews: (params = {}) => {
    return apiClient.get('/reviews', { params });
  },

  /**
   * Get reviews by exhibition
   * @param {number|string} exhibitionId - Exhibition ID
   * @returns {Promise} Axios response promise
   */
  getReviewsByExhibition: (exhibitionId) => {
    return apiClient.get('/reviews', { 
      params: { exhibitionId } 
    });
  },

  /**
   * Get reviews by visitor
   * @param {number|string} visitorId - Visitor ID
   * @returns {Promise} Axios response promise
   */
  getReviewsByVisitor: (visitorId) => {
    return apiClient.get('/reviews', { 
      params: { visitorId } 
    });
  },

  /**
   * Create a new review
   * @param {Object} review - Review data
   * @returns {Promise} Axios response promise
   */
  createReview: (review) => {
    return apiClient.post('/reviews', review);
  },

  /**
   * Update a review
   * @param {number|string} id - Review ID
   * @param {Object} review - Updated review data
   * @returns {Promise} Axios response promise
   */
  updateReview: (id, review) => {
    return apiClient.put(`/reviews/${id}`, review);
  },

  /**
   * Delete a review
   * @param {number|string} id - Review ID
   * @returns {Promise} Axios response promise
   */
  deleteReview: (id) => {
    return apiClient.delete(`/reviews/${id}`);
  },

  /**
   * Get visitor statistics
   * @returns {Promise} Axios response promise
   */
  getStatistics: () => {
    return apiClient.get('/visitors/statistics');
  }
};

export default visitorAPI;
