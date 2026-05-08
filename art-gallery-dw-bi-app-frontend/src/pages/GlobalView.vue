<template>
  <!--
    GlobalView.vue - Cerinta 2 (1p)
    Read-only "global" UI: citeste DOAR din ARTGALLERY_GLOBAL prin view-uri.
    UI-ul este complet transparent fata de fragmentare - utilizatorul nu vede AM/EU.
  -->
  <div class="space-y-6">
    <header>
      <h1 class="text-3xl font-display font-bold text-gray-900">Global Unified View</h1>
      <p class="text-sm text-gray-500">
        Cerinta 2: vizualizare unificata, transparenta pe ARTGALLERY_GLOBAL.
      </p>
    </header>

    <div class="bg-white rounded-lg shadow p-4 grid grid-cols-1 md:grid-cols-3 gap-4">
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Entitate globala</span>
        <select v-model="entity" class="mt-1 w-full border-gray-300 rounded-md">
          <optgroup label="Reconstruite (transparenta)">
            <option value="ARTWORK">Artworks (GLOBAL_ARTWORK = CORE@EU + DETAILS@AM)</option>
            <option value="EXHIBITOR">Exhibitors (GLOBAL_EXHIBITOR = AM UNION EU)</option>
          </optgroup>
          <optgroup label="Fragmentate orizontal (UNION ALL)">
            <option value="EXHIBITION">Exhibitions</option>
            <option value="LOAN">Loans</option>
            <option value="GALLERY_REVIEW">Gallery Reviews</option>
            <option value="ARTWORK_EXHIBITION">Artwork_Exhibition</option>
          </optgroup>
          <optgroup label="Replicate (UNION ALL via @link_*)">
            <option value="ARTIST">Artists</option>
            <option value="COLLECTION">Collections</option>
          </optgroup>
          <optgroup label="Globale (doar in ARTGALLERY_GLOBAL)">
            <option value="LOCATION">Locations</option>
            <option value="VISITOR">Visitors</option>
            <option value="STAFF">Staff</option>
            <option value="INSURANCE_POLICY">Insurance Policies</option>
            <option value="INSURANCE">Insurance</option>
            <option value="RESTORATION">Restoration</option>
            <option value="ACQUISITION">Acquisition</option>
          </optgroup>
        </select>
      </label>
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Limit</span>
        <input v-model.number="limit" type="number" min="1" max="2000" class="mt-1 w-full border-gray-300 rounded-md" />
      </label>
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Filtru text (client-side)</span>
        <input v-model="filter" type="text" placeholder="cauta..." class="mt-1 w-full border-gray-300 rounded-md" />
      </label>
    </div>

    <div class="flex gap-2">
      <button @click="load" class="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700">
        Reincarca din GLOBAL
      </button>
      <span class="px-3 py-2 text-xs text-gray-500">
        {{ filtered.length }} / {{ rows.length }} randuri
      </span>
    </div>

    <div class="bg-white rounded-lg shadow overflow-x-auto">
      <div v-if="loading" class="p-6 text-center text-gray-500">Se incarca...</div>
      <div v-else-if="error" class="p-6 text-red-600">{{ error }}</div>
      <table v-else-if="filtered.length" class="min-w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th v-for="c in columns" :key="c" class="px-3 py-2 text-left font-semibold text-gray-700">{{ c }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, i) in filtered" :key="i" class="border-t hover:bg-gray-50">
            <td v-for="c in columns" :key="c" class="px-3 py-2 text-gray-800 align-top">{{ formatVal(r[c]) }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="p-6 text-gray-500">Niciun rand.</div>
    </div>
  </div>
</template>

<script>
import bddAPI from '@/api/bddAPI';

export default {
  name: 'GlobalView',
  data() {
    return {
      entity: 'ARTWORK',
      limit: 200,
      rows: [],
      filter: '',
      loading: false,
      error: null
    };
  },
  computed: {
    columns() {
      if (!this.rows.length) return [];
      return Object.keys(this.rows[0]);
    },
    filtered() {
      if (!this.filter) return this.rows;
      const f = this.filter.toLowerCase();
      return this.rows.filter(r => Object.values(r).some(v =>
        v !== null && v !== undefined && String(v).toLowerCase().includes(f)
      ));
    }
  },
  watch: {
    entity() { this.load(); }
  },
  mounted() { this.load(); },
  methods: {
    formatVal(v) {
      if (v === null || v === undefined) return '';
      if (typeof v === 'object') return JSON.stringify(v);
      return String(v);
    },
    async load() {
      this.loading = true; this.error = null;
      try {
        const res = await bddAPI.listGlobal(this.entity, this.limit);
        this.rows = res.data?.data || [];
      } catch (e) {
        this.error = e.response?.data?.error || e.message;
        this.rows = [];
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>
