/**
 * Artwork API Service
 * Art Gallery Management System
 * 
 * Provides API methods for artwork CRUD operations.
 * Ready for .NET 10 backend integration.
 */

import apiClient from './client';

/**
 * Artwork API endpoints
 */
export const artworkAPI = {
  /**
   * Get all artworks
   * @param {Object} params - Query parameters for filtering/pagination
   * @returns {Promise} Axios response promise
   */
  getAll: (params = {}) => {
    return apiClient.get('/artworks', { params });
  },

  /**
   * Get a single artwork by ID
   * @param {number|string} id - Artwork ID
   * @returns {Promise} Axios response promise
   */
  getById: (id) => {
    return apiClient.get(`/artworks/${id}`);
  },

  /**
   * Create a new artwork
   * @param {Object} artwork - Artwork data
   * @returns {Promise} Axios response promise
   */
  create: (artwork) => {
    return apiClient.post('/artworks', artwork);
  },

  /**
   * Update an existing artwork
   * @param {number|string} id - Artwork ID
   * @param {Object} artwork - Updated artwork data
   * @returns {Promise} Axios response promise
   */
  update: (id, artwork) => {
    return apiClient.put(`/artworks/${id}`, artwork);
  },

  /**
   * Partially update an artwork
   * @param {number|string} id - Artwork ID
   * @param {Object} data - Partial update data
   * @returns {Promise} Axios response promise
   */
  patch: (id, data) => {
    return apiClient.patch(`/artworks/${id}`, data);
  },

  /**
   * Delete an artwork
   * @param {number|string} id - Artwork ID
   * @returns {Promise} Axios response promise
   */
  delete: (id) => {
    return apiClient.delete(`/artworks/${id}`);
  },

  /**
   * Search artworks by query
   * @param {string} query - Search query
   * @returns {Promise} Axios response promise
   */
  search: (query) => {
    return apiClient.get('/artworks', { 
      params: { q: query } 
    });
  },

  /**
   * Get artworks by artist
   * @param {number|string} artistId - Artist ID
   * @returns {Promise} Axios response promise
   */
  getByArtist: (artistId) => {
    return apiClient.get('/artworks', { 
      params: { artistId } 
    });
  },

  /**
   * Get artworks by collection
   * @param {number|string} collectionId - Collection ID
   * @returns {Promise} Axios response promise
   */
  getByCollection: (collectionId) => {
    return apiClient.get('/artworks', { 
      params: { collectionId } 
    });
  },

  /**
   * Get artwork statistics
   * @returns {Promise} Axios response promise
   */
  getStatistics: () => {
    return apiClient.get('/artworks/statistics');
  }
};

export default artworkAPI;
