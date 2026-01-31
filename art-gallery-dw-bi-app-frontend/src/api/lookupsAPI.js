/**
 * Lookups API Service
 * Art Gallery Management System
 * 
 * Provides API methods for fetching lookup data (dropdowns)
 * to support FK relationships in forms.
 */

import apiClient from './client';

/**
 * Lookups API endpoints
 */
export const lookupsAPI = {
  /**
   * Get all artists for dropdown
   * @returns {Promise} Axios response promise
   */
  getArtists() {
    return apiClient.get('/lookups/artists');
  },

  /**
   * Get all collections for dropdown
   * @returns {Promise} Axios response promise
   */
  getCollections() {
    return apiClient.get('/lookups/collections');
  },

  /**
   * Get all locations for dropdown
   * @returns {Promise} Axios response promise
   */
  getLocations() {
    return apiClient.get('/lookups/locations');
  },

  /**
   * Get all exhibitors for dropdown
   * @returns {Promise} Axios response promise
   */
  getExhibitors() {
    return apiClient.get('/lookups/exhibitors');
  },

  /**
   * Get all staff members for dropdown
   * @returns {Promise} Axios response promise
   */
  getStaff() {
    return apiClient.get('/lookups/staff');
  },

  /**
   * Get all artworks for dropdown (loans, exhibitions)
   * @returns {Promise} Axios response promise
   */
  getArtworks() {
    return apiClient.get('/lookups/artworks');
  },

  /**
   * Get all insurance policies for dropdown
   * @returns {Promise} Axios response promise
   */
  getPolicies() {
    return apiClient.get('/lookups/policies');
  },

  /**
   * Get all lookup data in a single request
   * @returns {Promise} Axios response promise with all lookups
   */
  getAll() {
    return apiClient.get('/lookups/all');
  }
};

export default lookupsAPI;
