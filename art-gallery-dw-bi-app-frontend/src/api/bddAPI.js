/**
 * BDD (Distributed Database) API client
 * Module 3 / MODBD
 *
 * Endpoints (mounted under VITE_API_BASE_URL):
 *   GET    /api/bdd/status
 *   GET    /api/bdd/local/{station}/{entity}?limit=
 *   POST   /api/bdd/local/{station}/{entity}
 *   PUT    /api/bdd/local/{station}/{entity}?keyColumn=&keyValue=
 *   DELETE /api/bdd/local/{station}/{entity}?keyColumn=&keyValue=
 *   GET    /api/bdd/global/{entity}?limit=
 *   POST   /api/bdd/global/{entity}
 *   PUT    /api/bdd/global/{entity}?keyColumn=&keyValue=
 *   DELETE /api/bdd/global/{entity}?keyColumn=&keyValue=
 *   POST   /api/bdd/demo/local-to-global
 *   POST   /api/bdd/demo/global-to-local
 */

import apiClient from './client';

// Note: apiClient.baseURL already includes the `/api` prefix (VITE_API_BASE_URL),
// so this base must NOT repeat it - otherwise requests hit `/api/api/bdd/...` (404).
const base = '/bdd';

export const bddAPI = {
  // Diagnostics
  status: () => apiClient.get(`${base}/status`),

  // Cerinta 1 - local CRUD
  listLocal: (station, entity, limit = 200) =>
    apiClient.get(`${base}/local/${station}/${entity}`, { params: { limit } }),
  insertLocal: (station, entity, values) =>
    apiClient.post(`${base}/local/${station}/${entity}`, values),
  updateLocal: (station, entity, keyColumn, keyValue, values) =>
    apiClient.put(`${base}/local/${station}/${entity}`, values, { params: { keyColumn, keyValue } }),
  deleteLocal: (station, entity, keyColumn, keyValue) =>
    apiClient.delete(`${base}/local/${station}/${entity}`, { params: { keyColumn, keyValue } }),

  // Cerinta 2 - global read (and Cerinta 4 writes)
  listGlobal: (entity, limit = 200) =>
    apiClient.get(`${base}/global/${entity}`, { params: { limit } }),
  insertGlobal: (entity, values) =>
    apiClient.post(`${base}/global/${entity}`, values),
  updateGlobal: (entity, keyColumn, keyValue, values) =>
    apiClient.put(`${base}/global/${entity}`, values, { params: { keyColumn, keyValue } }),
  deleteGlobal: (entity, keyColumn, keyValue) =>
    apiClient.delete(`${base}/global/${entity}`, { params: { keyColumn, keyValue } }),

  // Cerinta 3 / Cerinta 4 - demo scenarios
  demoLocalToGlobal: (payload) =>
    apiClient.post(`${base}/demo/local-to-global`, payload),
  demoGlobalToLocal: (payload) =>
    apiClient.post(`${base}/demo/global-to-local`, payload),
};

export default bddAPI;
