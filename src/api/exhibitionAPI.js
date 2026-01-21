/**
 * Exhibition API Service
 * Art Gallery Management System
 * 
 * Provides API methods for exhibition CRUD operations.
 * Ready for .NET 10 backend integration.
 */

import apiClient from './client';

/**
 * Exhibition API endpoints
 */
export const exhibitionAPI = {
  /**
   * Get all exhibitions
   * @param {Object} params - Query parameters for filtering/pagination
   * @returns {Promise} Axios response promise
   */
  getAll: (params = {}) => {
    return apiClient.get('/exhibitions', { params });
  },

  /**
   * Get a single exhibition by ID
   * @param {number|string} id - Exhibition ID
   * @returns {Promise} Axios response promise
   */
  getById: (id) => {
    return apiClient.get(`/exhibitions/${id}`);
  },

  /**
   * Create a new exhibition
   * @param {Object} exhibition - Exhibition data
   * @returns {Promise} Axios response promise
   */
  create: (exhibition) => {
    return apiClient.post('/exhibitions', exhibition);
  },

  /**
   * Update an existing exhibition
   * @param {number|string} id - Exhibition ID
   * @param {Object} exhibition - Updated exhibition data
   * @returns {Promise} Axios response promise
   */
  update: (id, exhibition) => {
    return apiClient.put(`/exhibitions/${id}`, exhibition);
  },

  /**
   * Delete an exhibition
   * @param {number|string} id - Exhibition ID
   * @returns {Promise} Axios response promise
   */
  delete: (id) => {
    return apiClient.delete(`/exhibitions/${id}`);
  },

  /**
   * Get exhibitions by status
   * @param {string} status - 'upcoming', 'ongoing', or 'past'
   * @returns {Promise} Axios response promise
   */
  getByStatus: (status) => {
    return apiClient.get('/exhibitions', { 
      params: { status } 
    });
  },

  /**
   * Get exhibitions by exhibitor
   * @param {number|string} exhibitorId - Exhibitor ID
   * @returns {Promise} Axios response promise
   */
  getByExhibitor: (exhibitorId) => {
    return apiClient.get('/exhibitions', { 
      params: { exhibitorId } 
    });
  },

  /**
   * Add artwork to exhibition
   * @param {number|string} exhibitionId - Exhibition ID
   * @param {number|string} artworkId - Artwork ID to add
   * @returns {Promise} Axios response promise
   */
  addArtwork: (exhibitionId, artworkId) => {
    return apiClient.post(`/exhibitions/${exhibitionId}/artworks`, { 
      artworkId 
    });
  },

  /**
   * Remove artwork from exhibition
   * @param {number|string} exhibitionId - Exhibition ID
   * @param {number|string} artworkId - Artwork ID to remove
   * @returns {Promise} Axios response promise
   */
  removeArtwork: (exhibitionId, artworkId) => {
    return apiClient.delete(`/exhibitions/${exhibitionId}/artworks/${artworkId}`);
  },

  /**
   * Get exhibition statistics
   * @returns {Promise} Axios response promise
   */
  getStatistics: () => {
    return apiClient.get('/exhibitions/statistics');
  }
};

export default exhibitionAPI;
