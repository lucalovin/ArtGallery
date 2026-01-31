/**
 * API Services Index
 * Art Gallery Management System
 * 
 * Exports all API services for use throughout the application.
 * Provides a unified interface for backend communication.
 */

import apiClient from './client';
import { etlAPI } from './etlAPI';
import { reportsAPI } from './reportsAPI';
import { analyticsAPI } from './analyticsAPI';
import { lookupsAPI } from './lookupsAPI';
import { insuranceAPI } from './insuranceAPI';

// Import other APIs if they export named exports, or import defaults
import * as artworkAPIModule from './artworkAPI';
import * as exhibitionAPIModule from './exhibitionAPI';
import * as visitorAPIModule from './visitorAPI';

// Extract the API objects from modules
const artworkAPI = artworkAPIModule.artworkAPI || artworkAPIModule.default || {};
const exhibitionAPI = exhibitionAPIModule.exhibitionAPI || exhibitionAPIModule.default || {};
const visitorAPI = visitorAPIModule.visitorAPI || visitorAPIModule.default || {};

/**
 * Staff API Service
 * Provides API methods for staff management.
 */
const staffAPI = {
  getStaff(params = {}) {
    return apiClient.get('/staff', { params });
  },
  getStaffById(id) {
    return apiClient.get(`/staff/${id}`);
  },
  createStaff(data) {
    return apiClient.post('/staff', data);
  },
  updateStaff(id, data) {
    return apiClient.put(`/staff/${id}`, data);
  },
  deleteStaff(id) {
    return apiClient.delete(`/staff/${id}`);
  }
};

/**
 * Artworks API Service (use artworkAPI directly with correct method names)
 */
const artworksAPI = {
  getAll(params = {}) {
    return apiClient.get('/artworks', { params });
  },
  getById(id) {
    return apiClient.get(`/artworks/${id}`);
  },
  create(data) {
    return apiClient.post('/artworks', data);
  },
  update(id, data) {
    return apiClient.put(`/artworks/${id}`, data);
  },
  delete(id) {
    return apiClient.delete(`/artworks/${id}`);
  },
  // Aliases for backward compatibility
  getArtworks(params = {}) {
    return apiClient.get('/artworks', { params });
  },
  getArtworkById(id) {
    return apiClient.get(`/artworks/${id}`);
  },
  createArtwork(data) {
    return apiClient.post('/artworks', data);
  },
  updateArtwork(id, data) {
    return apiClient.put(`/artworks/${id}`, data);
  },
  deleteArtwork(id) {
    return apiClient.delete(`/artworks/${id}`);
  }
};

/**
 * Exhibitions API Service (use consistent method names)
 */
const exhibitionsAPI = {
  getAll(params = {}) {
    return apiClient.get('/exhibitions', { params });
  },
  getById(id) {
    return apiClient.get(`/exhibitions/${id}`);
  },
  create(data) {
    return apiClient.post('/exhibitions', data);
  },
  update(id, data) {
    return apiClient.put(`/exhibitions/${id}`, data);
  },
  delete(id) {
    return apiClient.delete(`/exhibitions/${id}`);
  },
  // Aliases for backward compatibility
  getExhibitions(params = {}) {
    return apiClient.get('/exhibitions', { params });
  },
  getExhibitionById(id) {
    return apiClient.get(`/exhibitions/${id}`);
  },
  createExhibition(data) {
    return apiClient.post('/exhibitions', data);
  },
  updateExhibition(id, data) {
    return apiClient.put(`/exhibitions/${id}`, data);
  },
  deleteExhibition(id) {
    return apiClient.delete(`/exhibitions/${id}`);
  }
};

/**
 * Loans API Service (use consistent method names)
 */
const loansAPI = {
  getAll(params = {}) {
    return apiClient.get('/loans', { params });
  },
  getById(id) {
    return apiClient.get(`/loans/${id}`);
  },
  create(data) {
    return apiClient.post('/loans', data);
  },
  update(id, data) {
    return apiClient.put(`/loans/${id}`, data);
  },
  delete(id) {
    return apiClient.delete(`/loans/${id}`);
  },
  // Aliases for backward compatibility
  getLoans(params = {}) {
    return apiClient.get('/loans', { params });
  },
  getLoanById(id) {
    return apiClient.get(`/loans/${id}`);
  },
  createLoan(data) {
    return apiClient.post('/loans', data);
  },
  updateLoan(id, data) {
    return apiClient.put(`/loans/${id}`, data);
  },
  deleteLoan(id) {
    return apiClient.delete(`/loans/${id}`);
  }
};

/**
 * Combined API object for global access
 */
const api = {
  client: apiClient,
  etl: etlAPI,
  reports: reportsAPI,
  analytics: analyticsAPI,
  lookups: lookupsAPI,
  insurance: insuranceAPI,
  staff: staffAPI,
  artworks: artworksAPI,
  exhibitions: exhibitionsAPI,
  visitors: visitorAPI,
  loans: loansAPI
};

/**
 * Vue plugin to install API as global property
 * Usage: app.use(apiPlugin)
 * Access: this.$api.etl.getStatus(), this.$api.reports.getDashboardKPIs(), etc.
 */
export const apiPlugin = {
  install(app) {
    app.config.globalProperties.$api = api;
    
    // Also provide for Composition API
    app.provide('api', api);
  }
};

export { api, etlAPI, reportsAPI, analyticsAPI, lookupsAPI, insuranceAPI, staffAPI, artworksAPI, exhibitionsAPI, loansAPI };
export default api;
