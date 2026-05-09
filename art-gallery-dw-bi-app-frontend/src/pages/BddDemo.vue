<template>
  <!--
    BddDemo.vue
    Cerinta 3 (2p): LMD pe local -> vizibil global (BEFORE / OPERATIE / AFTER)
    Cerinta 4 (3p): LMD pe global -> propagat la fragment local (BEFORE / OPERATIE / AFTER)
    Toate cele 3 cazuri (orizontal, vertical, replicat) sunt acoperite.
  -->
  <div class="space-y-8">
    <header>
      <h1 class="text-3xl font-display font-bold text-gray-900">BDD Demo - Distributed Test Panel</h1>
      <p class="text-sm text-gray-500">
        Cerinta 3 (local -> global) si Cerinta 4 (global -> local) cu snapshot BEFORE/AFTER.
      </p>
    </header>

    <!-- Status statii -->
    <section class="bg-white rounded-lg shadow p-4">
      <div class="flex items-center justify-between mb-3">
        <h2 class="font-semibold text-gray-800">Status statii BDD</h2>
        <button @click="loadStatus" class="text-sm text-primary-600 hover:underline">Refresh</button>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-3 text-sm">
        <div v-for="s in ['AM','EU','GLOBAL']" :key="s"
             :class="['rounded p-3 border', statusOf(s).ok ? 'bg-emerald-50 border-emerald-200' : 'bg-red-50 border-red-200']">
          <div class="font-semibold">{{ s }}</div>
          <div v-if="statusOf(s).ok" class="text-xs text-gray-700">
            schema: {{ statusOf(s).schema }}<br>
            time: {{ statusOf(s).serverTime }}
          </div>
          <div v-else class="text-xs text-red-700">{{ statusOf(s).error || 'n/a' }}</div>
        </div>
      </div>
    </section>

    <!-- ============ CERINTA 3 ============ -->
    <section class="bg-white rounded-lg shadow p-4 space-y-4">
      <h2 class="text-xl font-bold text-gray-900">
        Cerinta 3 - LMD local -> vizibil in view-ul global
      </h2>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Scenariu</span>
          <select v-model="c3.scenario" class="mt-1 w-full border-gray-300 rounded-md">
            <optgroup label="Orizontal">
              <option value="exhibitor_am">EXHIBITOR_AM</option>
              <option value="exhibitor_eu">EXHIBITOR_EU</option>
              <option value="exhibition_am">EXHIBITION_AM</option>
              <option value="exhibition_eu">EXHIBITION_EU</option>
              <option value="loan_am">LOAN_AM</option>
              <option value="loan_eu">LOAN_EU</option>
              <option value="gallery_review_am">GALLERY_REVIEW_AM</option>
              <option value="gallery_review_eu">GALLERY_REVIEW_EU</option>
              <option value="artwork_exhibition_am">ARTWORK_EXHIBITION_AM</option>
              <option value="artwork_exhibition_eu">ARTWORK_EXHIBITION_EU</option>
            </optgroup>
            <optgroup label="Vertical (Artwork)">
              <option value="artwork_core">ARTWORK_CORE@EU</option>
              <option value="artwork_details">ARTWORK_DETAILS@AM</option>
            </optgroup>
            <optgroup label="Replicat">
              <option value="artist_am">ARTIST_AM</option>
              <option value="artist_eu">ARTIST_EU</option>
              <option value="collection_am">COLLECTION_AM</option>
              <option value="collection_eu">COLLECTION_EU</option>
            </optgroup>
          </select>
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Action</span>
          <select v-model="c3.action" class="mt-1 w-full border-gray-300 rounded-md">
            <option value="insert">INSERT</option>
            <option value="update">UPDATE</option>
            <option value="delete">DELETE</option>
          </select>
        </label>
        <div></div>
        <label class="block md:col-span-3">
          <span class="text-xs font-semibold text-gray-600 uppercase flex items-center justify-between">
            <span>Values (JSON)</span>
            <button type="button" @click="autoFillC3"
                    class="text-[10px] font-semibold px-2 py-1 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300 normal-case">
              Auto-fill din date reale
            </button>
          </span>
          <textarea v-model="c3.valuesJson" rows="4" class="mt-1 w-full font-mono text-xs border-gray-300 rounded-md"></textarea>
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Key column (UPDATE/DELETE)</span>
          <input v-model="c3.keyColumn" class="mt-1 w-full border-gray-300 rounded-md" />
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Key value</span>
          <input v-model="c3.keyValue" class="mt-1 w-full border-gray-300 rounded-md" />
        </label>
        <div class="flex items-end">
          <button @click="runC3" class="w-full px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700">
            Executa scenariul
          </button>
        </div>
      </div>

      <div v-if="c3.result" class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <SnapshotPanel title="Global BEFORE" :rows="c3.result.before" />
        <SnapshotPanel title="Global AFTER"  :rows="c3.result.after"  highlight />
      </div>
      <div v-if="c3.message" class="text-sm text-blue-700">{{ c3.message }}</div>
    </section>

    <!-- ============ CERINTA 4 ============ -->
    <section class="bg-white rounded-lg shadow p-4 space-y-4">
      <h2 class="text-xl font-bold text-gray-900">
        Cerinta 4 - LMD global -> propagat in fragmentul local
      </h2>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Scenariu</span>
          <select v-model="c4.scenario" class="mt-1 w-full border-gray-300 rounded-md">
            <optgroup label="Orizontal (necesita ?station= sau city pentru EXHIBITOR)">
              <option value="horizontal_exhibitor">EXHIBITOR (rutare automata dupa 'city')</option>
              <option value="horizontal_exhibition">EXHIBITION</option>
              <option value="horizontal_loan">LOAN</option>
              <option value="horizontal_review">GALLERY_REVIEW</option>
              <option value="horizontal_artwork_exh">ARTWORK_EXHIBITION</option>
            </optgroup>
            <optgroup label="Vertical">
              <option value="vertical_artwork">ARTWORK (CORE@EU + DETAILS@AM)</option>
            </optgroup>
            <optgroup label="Replicat (dual-write)">
              <option value="replicated_artist">ARTIST (-> AM + EU)</option>
              <option value="replicated_collection">COLLECTION (-> AM + EU)</option>
            </optgroup>
            <optgroup label="Global-only">
              <option value="global_location">LOCATION</option>
              <option value="global_visitor">VISITOR</option>
              <option value="global_staff">STAFF</option>
              <option value="global_insurance_policy">INSURANCE_POLICY</option>
              <option value="global_insurance">INSURANCE</option>
              <option value="global_restoration">RESTORATION</option>
              <option value="global_acquisition">ACQUISITION</option>
            </optgroup>
          </select>
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Action</span>
          <select v-model="c4.action" class="mt-1 w-full border-gray-300 rounded-md">
            <option value="insert">INSERT</option>
            <option value="update">UPDATE</option>
            <option value="delete">DELETE</option>
          </select>
        </label>
        <div></div>
        <label class="block md:col-span-3">
          <span class="text-xs font-semibold text-gray-600 uppercase flex items-center justify-between">
            <span>Values (JSON)</span>
            <button type="button" @click="autoFillC4"
                    class="text-[10px] font-semibold px-2 py-1 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300 normal-case">
              Auto-fill din date reale
            </button>
          </span>
          <textarea v-model="c4.valuesJson" rows="4" class="mt-1 w-full font-mono text-xs border-gray-300 rounded-md"></textarea>
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Key column (UPDATE/DELETE)</span>
          <input v-model="c4.keyColumn" class="mt-1 w-full border-gray-300 rounded-md" />
        </label>
        <label class="block">
          <span class="text-xs font-semibold text-gray-600 uppercase">Key value</span>
          <input v-model="c4.keyValue" class="mt-1 w-full border-gray-300 rounded-md" />
        </label>
        <div class="flex items-end">
          <button @click="runC4" class="w-full px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700">
            Executa scenariul
          </button>
        </div>
      </div>

      <div v-if="c4.result" class="space-y-4">
        <div v-for="(rows, key) in c4.result.before" :key="'b-'+key" class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <SnapshotPanel :title="key + ' BEFORE'" :rows="rows" />
          <SnapshotPanel :title="key + ' AFTER'"  :rows="c4.result.after[key]" highlight />
        </div>
      </div>
      <div v-if="c4.message" class="text-sm text-blue-700">{{ c4.message }}</div>
    </section>
  </div>
</template>

<script>
import bddAPI from '@/api/bddAPI';

const SnapshotPanel = {
  props: { title: String, rows: Array, highlight: Boolean },
  template: `
    <div :class="['rounded border p-2 overflow-x-auto', highlight ? 'border-emerald-300 bg-emerald-50' : 'border-gray-200 bg-gray-50']">
      <div class="text-xs font-semibold text-gray-700 mb-1">{{ title }} ({{ rows ? rows.length : 0 }} randuri)</div>
      <table v-if="rows && rows.length" class="min-w-full text-xs">
        <thead><tr>
          <th v-for="c in Object.keys(rows[0])" :key="c" class="px-2 py-1 text-left font-semibold text-gray-700">{{ c }}</th>
        </tr></thead>
        <tbody>
          <tr v-for="(r, i) in rows" :key="i" class="border-t">
            <td v-for="c in Object.keys(rows[0])" :key="c" class="px-2 py-1 text-gray-800">{{ r[c] }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="text-xs text-gray-500">(gol)</div>
    </div>
  `
};

export default {
  name: 'BddDemo',
  components: { SnapshotPanel },
  data() {
    return {
      status: {},
      c3: {
        scenario: 'exhibition_am',
        action: 'insert',
        valuesJson: '{\n  "exhibition_id": 9001,\n  "title": "Demo Local AM",\n  "start_date": "2026-06-01",\n  "end_date": "2026-06-30",\n  "exhibitor_id": 1,\n  "description": "demo c3"\n}',
        keyColumn: '',
        keyValue: '',
        result: null,
        message: null
      },
      c4: {
        scenario: 'horizontal_exhibitor',
        action: 'insert',
        valuesJson: '{\n  "exhibitor_id": 9101,\n  "name": "Demo Global Routed",\n  "address": "5th Ave",\n  "city": "New York",\n  "contact_info": "demo@x"\n}',
        keyColumn: '',
        keyValue: '',
        result: null,
        message: null
      }
    };
  },
  mounted() {
    this.loadStatus();
    this.autoFillC3();
    this.autoFillC4();
  },
  watch: {
    'c3.scenario'() { this.autoFillC3(); },
    'c3.action'(val) {
      if (val === 'insert') this.autoFillC3();
    },
    'c4.scenario'() { this.autoFillC4(); },
    'c4.action'(val) {
      if (val === 'insert') this.autoFillC4();
    }
  },
  methods: {
    statusOf(s) { return this.status[s] || { ok: false }; },
    async loadStatus() {
      try {
        const res = await bddAPI.status();
        this.status = res.data?.data || {};
      } catch (e) {
        this.status = {};
      }
    },
    parseJsonOrNull(s) {
      try { return s ? JSON.parse(s) : null; } catch { return null; }
    },
    async autoFillC3() {
      if (this.c3.action !== 'insert') return;
      try {
        const res = await bddAPI.demoSampleValues(this.c3.scenario);
        const values = res.data?.data;
        if (values && typeof values === 'object') {
          this.c3.valuesJson = JSON.stringify(values, null, 2);
        }
      } catch (e) {
        // keep existing template; surface a hint
        this.c3.message = 'Nu am putut auto-completa valorile: ' +
          (e.response?.data?.error || e.message);
      }
    },
    async autoFillC4() {
      if (this.c4.action !== 'insert') return;
      try {
        const res = await bddAPI.demoSampleValues(this.c4.scenario);
        const values = res.data?.data;
        if (values && typeof values === 'object') {
          this.c4.valuesJson = JSON.stringify(values, null, 2);
        }
      } catch (e) {
        this.c4.message = 'Nu am putut auto-completa valorile: ' +
          (e.response?.data?.error || e.message);
      }
    },
    async runC3() {
      this.c3.message = null; this.c3.result = null;
      try {
        const payload = {
          scenario: this.c3.scenario,
          action: this.c3.action,
          values: this.parseJsonOrNull(this.c3.valuesJson),
          keyColumn: this.c3.keyColumn || null,
          keyValue: this.c3.keyValue || null
        };
        const res = await bddAPI.demoLocalToGlobal(payload);
        this.c3.result = res.data?.data;
        this.c3.message = res.data?.message;
      } catch (e) {
        this.c3.message = 'Eroare: ' + (e.response?.data?.error || e.message);
      }
    },
    async runC4() {
      this.c4.message = null; this.c4.result = null;
      try {
        const payload = {
          scenario: this.c4.scenario,
          action: this.c4.action,
          values: this.parseJsonOrNull(this.c4.valuesJson),
          keyColumn: this.c4.keyColumn || null,
          keyValue: this.c4.keyValue || null
        };
        const res = await bddAPI.demoGlobalToLocal(payload);
        this.c4.result = res.data?.data;
        this.c4.message = res.data?.message;
      } catch (e) {
        this.c4.message = 'Eroare: ' + (e.response?.data?.error || e.message);
      }
    }
  }
};
</script>
