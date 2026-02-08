/**
 * Axios API Client Configuration
 * Art Gallery Management System
 * 
 * This file configures the Axios HTTP client for API calls.
 * Prepared for integration with .NET 10 REST API.
 * Currently uses json-server for development.
 */

import axios from 'axios';

/**
 * Create Axios instance with default configuration
 */
const apiClient = axios.create({
  // Base URL from environment variable or default to json-server
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:3000',
  
  // Request timeout (10 seconds)
  timeout: 10000,
  
  // Default headers
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    // Prevent browser caching to ensure fresh data on refresh
    'Cache-Control': 'no-cache, no-store, must-revalidate',
    'Pragma': 'no-cache',
    'Expires': '0'
  }
});

/**
 * Request interceptor
 * Runs before every request is sent
 */
apiClient.interceptors.request.use(
  (config) => {
    // Log request in development
    if (import.meta.env.DEV) {
      console.log(`📡 API Request: ${config.method?.toUpperCase()} ${config.url}`);
    }

    // Add authorization token if available
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add request timestamp for performance tracking
    config.metadata = { startTime: new Date() };

    return config;
  },
  (error) => {
    console.error('Request interceptor error:', error);
    return Promise.reject(error);
  }
);

/**
 * Response interceptor
 * Runs after every response is received
 */
apiClient.interceptors.response.use(
  (response) => {
    // Calculate request duration
    const duration = new Date() - response.config.metadata?.startTime;
    
    // Log response in development
    if (import.meta.env.DEV) {
      console.log(`✅ API Response: ${response.status} (${duration}ms)`);
    }

    return response;
  },
  (error) => {
    // Calculate request duration even for errors
    const duration = error.config?.metadata 
      ? new Date() - error.config.metadata.startTime 
      : 0;

    // Log error details
    console.error('API Error:', {
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status,
      message: error.message,
      duration: `${duration}ms`
    });

    // Handle specific error status codes
    if (error.response) {
      switch (error.response.status) {
        case 401:
          // Unauthorized - clear auth and redirect to login
          localStorage.removeItem('authToken');
          // In a real app, redirect to login page
          console.warn('Unauthorized access - please login');
          break;
        
        case 403:
          // Forbidden
          console.warn('Access forbidden');
          break;
        
        case 404:
          // Not found
          console.warn('Resource not found');
          break;
        
        case 422:
          // Validation error
          console.warn('Validation error:', error.response.data);
          break;
        
        case 500:
          // Server error
          console.error('Server error - please try again later');
          break;
        
        default:
          console.error(`API Error: ${error.response.status}`);
      }
    } else if (error.request) {
      // Network error - no response received
      console.error('Network error - please check your connection');
    }

    return Promise.reject(error);
  }
);

/**
 * Helper function to handle API errors consistently
 * @param {Error} error - The error object from Axios
 * @returns {Object} Normalized error object
 */
export const handleApiError = (error) => {
  return {
    message: error.response?.data?.message || error.message || 'An error occurred',
    status: error.response?.status || 500,
    data: error.response?.data || null
  };
};

/**
 * Helper function to create query string from object
 * @param {Object} params - Query parameters object
 * @returns {string} Query string
 */
export const createQueryString = (params) => {
  const query = new URLSearchParams();
  
  Object.entries(params).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== '') {
      query.append(key, value);
    }
  });
  
  return query.toString();
};

export default apiClient;
