/**
 * Analytics API Service
 * Art Gallery Management System - DW/BI Module
 * 
 * Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
 * Provides API methods for Data Warehouse analytics endpoints.
 * 
 * Endpoints:
 * 1. GET /api/reports/analytics/top-artists - Top artists by artwork count
 * 2. GET /api/reports/analytics/value-by-category - Value by medium and collection
 * 3. GET /api/reports/analytics/visitor-trends - Monthly exhibition activity
 * 4. GET /api/reports/analytics/membership-distribution - Location distribution
 * 5. GET /api/reports/analytics/acquisition-trends - Annual exhibition trends
 */

import apiClient from './client';

/**
 * Analytics API endpoints for DW queries
 */
export const analyticsAPI = {
  /**
   * Query 1: Get top N artists by artwork count with total estimated value.
   * Natural Language: "Show me the top 10 artists with the most artworks in the collection"
   * 
   * @param {number} topN - Number of top artists to retrieve (default: 10)
   * @returns {Promise} Axios response promise with artist statistics
   * 
   * Response format:
   * {
   *   success: true,
   *   data: [
   *     { artistName: string, artworkCount: number, totalValue: number, averageValue: number }
   *   ]
   * }
   */
  getTopArtists(topN = 10) {
    return apiClient.get(`/reports/analytics/top-artists?topN=${topN}`);
  },

  /**
   * Query 2: Get collection value breakdown by art medium and collection type.
   * Natural Language: "What is the total estimated value broken down by art medium and collection type?"
   * 
   * @returns {Promise} Axios response promise with category value data
   * 
   * Response format:
   * {
   *   success: true,
   *   data: [
   *     { mediumType: string, collectionName: string, artworkCount: number, totalValue: number, averageValue: number }
   *   ]
   * }
   */
  getValueByCategory() {
    return apiClient.get('/reports/analytics/value-by-category');
  },

  /**
   * Query 3: Get monthly exhibition activity metrics for the last N months.
   * Natural Language: "Analyze exhibition performance: show monthly activity metrics for the past year"
   * 
   * @param {number} months - Number of months to analyze (default: 12)
   * @returns {Promise} Axios response promise with monthly activity data
   * 
   * Response format:
   * {
   *   success: true,
   *   data: [
   *     { monthName: string, year: number, exhibitionCount: number, artworksExhibited: number, 
   *       totalArtworkValue: number, averageRating: number }
   *   ]
   * }
   */
  getVisitorTrends(months = 12) {
    return apiClient.get(`/reports/analytics/visitor-trends?months=${months}`);
  },

  /**
   * Query 4: Get location/gallery distribution of artworks.
   * Natural Language: "What is the gallery occupancy rate and distribution of artworks across locations?"
   * 
   * @returns {Promise} Axios response promise with location distribution data
   * 
   * Response format:
   * {
   *   success: true,
   *   data: [
   *     { locationName: string, galleryRoom: string, locationType: string, 
   *       artworksCount: number, totalValue: number, percentage: number }
   *   ]
   * }
   */
  getMembershipDistribution() {
    return apiClient.get('/reports/analytics/membership-distribution');
  },

  /**
   * Query 5: Get annual exhibition value trends with year-over-year growth.
   * Natural Language: "Show the trend of exhibition activity: how has the annual total artwork value evolved?"
   * 
   * @param {number} years - Number of years to analyze (default: 5)
   * @returns {Promise} Axios response promise with annual trend data
   * 
   * Response format:
   * {
   *   success: true,
   *   data: [
   *     { year: number, exhibitionsCount: number, artworksCount: number,
   *       totalArtworkValue: number, averageArtworkValue: number,
   *       previousYearValue: number, yoyGrowthRate: number }
   *   ]
   * }
   */
  getAcquisitionTrends(years = 5) {
    return apiClient.get(`/reports/analytics/acquisition-trends?years=${years}`);
  }
};

export default analyticsAPI;
