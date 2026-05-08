<template>
  <!--
    LocalManagement.vue - Cerinta 1 (3p)
    CRUD on LOCAL distributed databases (ARTGALLERY_AM / ARTGALLERY_EU)
    The user picks a station (Americas / Europe) and an entity, and operates
    directly on the local fragment - bypassing the global views.
  -->
  <div class="space-y-6">
    <header class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-display font-bold text-gray-900">BDD - Local Management</h1>
        <p class="text-sm text-gray-500">
          Cerinta 1: CRUD direct pe baze locale (ARTGALLERY_AM / ARTGALLERY_EU).
        </p>
      </div>
      <span
        :class="[
          'px-4 py-2 rounded-full text-sm font-semibold tracking-wide',
          station === 'AM' ? 'bg-blue-100 text-blue-800' : 'bg-emerald-100 text-emerald-800'
        ]"
      >
        Statie: {{ station === 'AM' ? 'Americas (New York)' : 'Europe (Paris/London/Madrid)' }}
      </span>
    </header>

    <!-- Selectoare -->
    <div class="bg-white rounded-lg shadow p-4 grid grid-cols-1 md:grid-cols-3 gap-4">
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Statie locala</span>
        <select v-model="station" class="mt-1 w-full border-gray-300 rounded-md">
          <option value="AM">Americas (ARTGALLERY_AM)</option>
          <option value="EU">Europe (ARTGALLERY_EU)</option>
        </select>
      </label>
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Entitate</span>
        <select v-model="entity" class="mt-1 w-full border-gray-300 rounded-md">
          <optgroup label="Fragmentate orizontal (sufix _AM/_EU)">
            <option value="EXHIBITOR">Exhibitor</option>
            <option value="EXHIBITION">Exhibition</option>
            <option value="LOAN">Loan</option>
            <option value="GALLERY_REVIEW">Gallery_Review</option>
            <option value="ARTWORK_EXHIBITION">Artwork_Exhibition</option>
          </optgroup>
          <optgroup label="Fragmentate vertical (Artwork)">
            <option value="ARTWORK_CORE" :disabled="station !== 'EU'">ARTWORK_CORE (doar EU)</option>
            <option value="ARTWORK_DETAILS" :disabled="station !== 'AM'">ARTWORK_DETAILS (doar AM)</option>
          </optgroup>
          <optgroup label="Replicate (tabele _AM si _EU)">
            <option value="ARTIST">Artist</option>
            <option value="COLLECTION">Collection</option>
          </optgroup>
        </select>
      </label>
      <label class="block">
        <span class="text-xs font-semibold text-gray-600 uppercase">Limit</span>
        <input v-model.number="limit" type="number" min="1" max="2000" class="mt-1 w-full border-gray-300 rounded-md" />
      </label>
    </div>

    <div class="flex flex-wrap gap-2">
      <button @click="load" class="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700">
        Reincarca
      </button>
      <button @click="showInsertForm = !showInsertForm" class="px-4 py-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700">
        {{ showInsertForm ? 'Anuleaza INSERT' : 'INSERT nou' }}
      </button>
    </div>

    <!-- INSERT form (free JSON for column flexibility across all entities) -->
    <div v-if="showInsertForm" class="bg-white rounded-lg shadow p-4 space-y-3">
      <h3 class="font-semibold text-gray-800">INSERT pe {{ entity }}@{{ station }}</h3>
      <p class="text-xs text-gray-500">
        Introdu un obiect JSON cu coloanele si valorile. Exemplu:
        <code class="bg-gray-100 px-1">{ "EXHIBITORID": 999, "NAME": "Test", "CITY": "New York" }</code>
      </p>
      <textarea v-model="insertJson" rows="6" class="w-full font-mono text-sm border-gray-300 rounded-md"></textarea>
      <button @click="doInsert" class="px-4 py-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700">
        Executa INSERT
      </button>
    </div>

    <!-- Tabel rezultate cu UPDATE/DELETE inline pe rand -->
    <div class="bg-white rounded-lg shadow overflow-x-auto">
      <div v-if="loading" class="p-6 text-center text-gray-500">Se incarca...</div>
      <div v-else-if="error" class="p-6 text-red-600">{{ error }}</div>
      <table v-else-if="rows.length" class="min-w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th v-for="c in columns" :key="c" class="px-3 py-2 text-left font-semibold text-gray-700">{{ c }}</th>
            <th class="px-3 py-2 text-right">Actiuni</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, i) in rows" :key="i" class="border-t">
            <td v-for="c in columns" :key="c" class="px-3 py-2 text-gray-800 align-top">{{ formatVal(r[c]) }}</td>
            <td class="px-3 py-2 text-right whitespace-nowrap">
              <button @click="startEdit(r)" class="text-blue-600 hover:underline mr-3">Edit</button>
              <button @click="doDelete(r)" class="text-red-600 hover:underline">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="p-6 text-gray-500">Niciun rand.</div>
    </div>

    <!-- UPDATE inline modal -->
    <div v-if="editRow" class="bg-white rounded-lg shadow p-4 space-y-3">
      <h3 class="font-semibold text-gray-800">UPDATE pe {{ entity }}@{{ station }}</h3>
      <label class="block text-xs font-semibold text-gray-600">
        Coloana cheie
        <input v-model="editKeyColumn" class="mt-1 w-full border-gray-300 rounded-md" />
      </label>
      <label class="block text-xs font-semibold text-gray-600">
        Valoare cheie
        <input v-model="editKeyValue" class="mt-1 w-full border-gray-300 rounded-md" />
      </label>
      <label class="block text-xs font-semibold text-gray-600">
        Set values (JSON)
        <textarea v-model="editJson" rows="4" class="mt-1 w-full font-mono text-sm border-gray-300 rounded-md"></textarea>
      </label>
      <div class="flex gap-2">
        <button @click="doUpdate" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">Salveaza UPDATE</button>
        <button @click="editRow = null" class="px-4 py-2 bg-gray-200 rounded-md">Renunta</button>
      </div>
    </div>

    <!-- Mesaj rezultat operatie -->
    <div v-if="opMessage" class="bg-blue-50 border border-blue-200 text-blue-800 px-4 py-3 rounded">
      {{ opMessage }}
    </div>
  </div>
</template>

<script>
import bddAPI from '@/api/bddAPI';

export default {
  name: 'LocalManagement',
  data() {
    return {
      station: 'AM',
      entity: 'EXHIBITOR',
      limit: 100,
      rows: [],
      loading: false,
      error: null,
      opMessage: null,
      showInsertForm: false,
      insertJson: '{\n  \n}',
      editRow: null,
      editKeyColumn: '',
      editKeyValue: '',
      editJson: '{\n  \n}'
    };
  },
  computed: {
    columns() {
      if (!this.rows.length) return [];
      return Object.keys(this.rows[0]);
    }
  },
  watch: {
    station() { this.load(); },
    entity()  { this.load(); }
  },
  mounted() { this.load(); },
  methods: {
    formatVal(v) {
      if (v === null || v === undefined) return '';
      if (typeof v === 'object') return JSON.stringify(v);
      return String(v);
    },
    async load() {
      this.loading = true; this.error = null; this.opMessage = null;
      try {
        const res = await bddAPI.listLocal(this.station, this.entity, this.limit);
        this.rows = res.data?.data || [];
      } catch (e) {
        this.error = e.response?.data?.error || e.message;
        this.rows = [];
      } finally {
        this.loading = false;
      }
    },
    async doInsert() {
      try {
        const values = JSON.parse(this.insertJson);
        const res = await bddAPI.insertLocal(this.station, this.entity, values);
        this.opMessage = res.data?.message || 'INSERT executat.';
        this.showInsertForm = false;
        await this.load();
      } catch (e) {
        this.opMessage = 'Eroare INSERT: ' + (e.response?.data?.error || e.message);
      }
    },
    startEdit(row) {
      this.editRow = row;
      const keys = Object.keys(row);
      this.editKeyColumn = keys[0] || '';
      this.editKeyValue = String(row[this.editKeyColumn] ?? '');
      const editable = { ...row };
      delete editable[this.editKeyColumn];
      this.editJson = JSON.stringify(editable, null, 2);
    },
    async doUpdate() {
      try {
        const values = JSON.parse(this.editJson);
        const res = await bddAPI.updateLocal(this.station, this.entity, this.editKeyColumn, this.editKeyValue, values);
        this.opMessage = res.data?.message || 'UPDATE executat.';
        this.editRow = null;
        await this.load();
      } catch (e) {
        this.opMessage = 'Eroare UPDATE: ' + (e.response?.data?.error || e.message);
      }
    },
    async doDelete(row) {
      const keys = Object.keys(row);
      const keyColumn = keys[0];
      const keyValue = String(row[keyColumn] ?? '');
      if (!confirm(`Sterg ${this.entity}@${this.station} unde ${keyColumn} = ${keyValue}?`)) return;
      try {
        const res = await bddAPI.deleteLocal(this.station, this.entity, keyColumn, keyValue);
        this.opMessage = res.data?.message || 'DELETE executat.';
        await this.load();
      } catch (e) {
        this.opMessage = 'Eroare DELETE: ' + (e.response?.data?.error || e.message);
      }
    }
  }
};
</script>
