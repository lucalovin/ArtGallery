/**
 * Vuex module: dataSourceStore
 *
 * Holds which Oracle schema the OLTP-style endpoints should target:
 *   - 'OLTP'   = ART_GALLERY_OLTP (default, legacy behaviour)
 *   - 'AM'     = ARTGALLERY_AM   (Americas local fragment)
 *   - 'EU'     = ARTGALLERY_EU   (Europe local fragment)
 *   - 'GLOBAL' = ARTGALLERY_GLOBAL (unified views via DB links)
 *
 * The selection is persisted in localStorage and is sent on every API call as
 * the `X-Data-Source` request header (see `src/api/client.js`). The backend
 * `DataSourceMiddleware` reads that header and configures `AppDbContext`
 * accordingly per request.
 */

const STORAGE_KEY = 'artgallery.dataSource';
const VALID = ['OLTP', 'AM', 'EU', 'GLOBAL'];

function loadInitial() {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && VALID.includes(stored)) return stored;
  } catch (_) { /* ignore */ }
  return 'OLTP';
}

const SOURCE_LABELS = {
  OLTP:   { label: 'OLTP (legacy)',  badge: 'bg-gray-200 text-gray-800',    schema: 'ART_GALLERY_OLTP' },
  AM:     { label: 'BDD · Americas', badge: 'bg-amber-100 text-amber-800',  schema: 'ARTGALLERY_AM' },
  EU:     { label: 'BDD · Europe',   badge: 'bg-blue-100 text-blue-800',    schema: 'ARTGALLERY_EU' },
  GLOBAL: { label: 'BDD · Global',   badge: 'bg-emerald-100 text-emerald-800', schema: 'ARTGALLERY_GLOBAL' }
};

// Default capability matrix; refined when /api/datasource is fetched.
const DEFAULT_SUPPORTS = {
  OLTP: { artworks: true, exhibitions: true, exhibitors: true, artists: true, collections: true, locations: true, visitors: true, staff: true, loans: true, insurance: true, restorations: true, reviews: true, etl: true, analytics: true },
  AM:   { artworks: true, exhibitions: true, exhibitors: true, artists: true, collections: true, locations: false, visitors: false, staff: false, loans: true, insurance: false, restorations: false, reviews: true, etl: false, analytics: false },
  EU:   { artworks: true, exhibitions: true, exhibitors: true, artists: true, collections: true, locations: false, visitors: false, staff: false, loans: true, insurance: false, restorations: false, reviews: true, etl: false, analytics: false },
  GLOBAL:{ artworks: true, exhibitions: false, exhibitors: true, artists: false, collections: false, locations: true, visitors: true, staff: true, loans: false, insurance: true, restorations: true, reviews: false, etl: false, analytics: false }
};

export default {
  namespaced: true,

  state: () => ({
    source: loadInitial(),
    supports: { ...DEFAULT_SUPPORTS }
  }),

  getters: {
    current: (state) => state.source,
    isOltp: (state) => state.source === 'OLTP',
    isBdd: (state) => state.source !== 'OLTP',
    label: (state) => SOURCE_LABELS[state.source]?.label ?? state.source,
    badgeClass: (state) => SOURCE_LABELS[state.source]?.badge ?? 'bg-gray-200 text-gray-800',
    schema: (state) => SOURCE_LABELS[state.source]?.schema ?? state.source,
    options: () => VALID.map(code => ({ code, ...SOURCE_LABELS[code] })),
    /** Whether the given logical module is supported on the current source. */
    supports: (state) => (module) => {
      const map = state.supports[state.source] ?? {};
      return map[module] !== false;
    }
  },

  mutations: {
    SET_SOURCE(state, value) {
      if (!VALID.includes(value)) return;
      state.source = value;
      try { localStorage.setItem(STORAGE_KEY, value); } catch (_) { /* ignore */ }
    },
    SET_SUPPORTS(state, payload) {
      if (payload && typeof payload === 'object') {
        state.supports = { ...state.supports, ...payload };
      }
    }
  },

  actions: {
    setSource({ commit }, value) {
      commit('SET_SOURCE', value);
    }
  }
};

/**
 * Read the currently selected source synchronously (used by the axios
 * interceptor so it doesn't have to depend on a Vuex store instance).
 */
export function getCurrentDataSource() {
  return loadInitial();
}
