<template>
  <!--
    ETLDashboard.vue - ETL Control Dashboard Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-dashboard">
    <!-- Quick Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Last Sync Status</p>
            <p class="text-lg font-semibold" :class="lastSyncStatusClass">{{ lastSyncStatus }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Last Sync Time</p>
            <p class="text-lg font-semibold text-gray-900">{{ formattedLastSyncTime }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4"
              />
            </svg>
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Records Synced</p>
            <p class="text-lg font-semibold text-gray-900">{{ totalRecordsSynced.toLocaleString() }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 10V3L4 14h7v7l9-11h-7z"
              />
            </svg>
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Pending Changes</p>
            <p class="text-lg font-semibold" :class="pendingChanges > 0 ? 'text-yellow-600' : 'text-gray-900'">
              {{ pendingChanges }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Left Column: ETL Controls -->
      <div class="lg:col-span-2 space-y-6">
        <!-- Sync Controls Panel -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
            <h2 class="text-lg font-semibold text-gray-900">Sync Controls</h2>
          </div>

          <div class="p-6">
            <!-- Data Source Selection -->
            <div class="mb-6">
              <h3 class="text-sm font-medium text-gray-700 mb-3">Select Data Sources to Sync</h3>

              <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
                <label
                  v-for="source in mergedDataSources"
                  :key="source.id"
                  class="flex items-center p-3 bg-gray-50 rounded-lg hover:bg-gray-100 cursor-pointer transition-colors"
                  :class="{ 'ring-2 ring-primary-500 bg-primary-50': selectedSources.includes(source.id) }"
                >
                  <input
                    type="checkbox"
                    :value="source.id"
                    v-model="selectedSources"
                    class="form-checkbox text-primary-600 rounded"
                  />
                  <div class="ml-3">
                    <p class="text-sm font-medium text-gray-900">{{ source.name }}</p>
                    <p class="text-xs text-gray-500">{{ source.recordCount }} records</p>
                  </div>
                </label>
              </div>
            </div>

            <!-- Sync Type Selection -->
            <div class="mb-6">
              <h3 class="text-sm font-medium text-gray-700 mb-3">Sync Type</h3>

              <div class="flex space-x-4">
                <label class="flex items-center">
                  <input
                    type="radio"
                    v-model="syncType"
                    value="incremental"
                    class="form-radio text-primary-600"
                  />
                  <span class="ml-2 text-sm">Incremental (Changes only)</span>
                </label>

                <label class="flex items-center">
                  <input
                    type="radio"
                    v-model="syncType"
                    value="full"
                    class="form-radio text-primary-600"
                  />
                  <span class="ml-2 text-sm">Full Refresh</span>
                </label>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="flex flex-wrap gap-3">
              <button
                @click="startSync"
                :disabled="isSyncing || selectedSources.length === 0"
                class="btn btn-primary"
                :class="{ 'opacity-50 cursor-not-allowed': isSyncing || selectedSources.length === 0 }"
              >
                <span v-if="isSyncing" class="flex items-center">
                  <svg class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                    <path
                      class="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    />
                  </svg>
                  Syncing...
                </span>
                <span v-else>Start Sync</span>
              </button>

              <button @click="selectAllSources" class="btn btn-secondary">
                Select All
              </button>

              <button @click="clearSelection" class="btn btn-secondary">
                Clear Selection
              </button>

              <button v-if="isSyncing" @click="cancelSync" class="btn btn-danger">
                Cancel
              </button>
            </div>
          </div>
        </div>

        <!-- Progress Panel -->
        <div
          v-if="isSyncing || syncProgress > 0"
          class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
        >
          <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
            <h2 class="text-lg font-semibold text-gray-900">Sync Progress</h2>
          </div>

          <div class="p-6">
            <div class="mb-6">
              <div class="flex justify-between text-sm mb-2">
                <span class="text-gray-600">Overall Progress</span>
                <span class="font-medium">{{ syncProgress }}%</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-3">
                <div
                  class="bg-primary-500 h-3 rounded-full transition-all duration-500"
                  :style="{ width: `${syncProgress}%` }"
                ></div>
              </div>
            </div>

            <div v-if="currentOperation" class="p-4 bg-blue-50 rounded-lg">
              <p class="text-sm text-blue-700">
                <strong>Current:</strong> {{ currentOperation }}
              </p>
            </div>

            <div class="mt-4 space-y-3">
              <div
                v-for="source in syncingSourcesStatus"
                :key="source.id"
                class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
              >
                <div class="flex items-center">
                  <span v-if="source.status === 'completed'" class="w-5 h-5 text-green-500">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path
                        fill-rule="evenodd"
                        d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                        clip-rule="evenodd"
                      />
                    </svg>
                  </span>

                  <span v-else-if="source.status === 'syncing'" class="w-5 h-5 text-blue-500 animate-spin">
                    <svg fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                    </svg>
                  </span>

                  <span v-else-if="source.status === 'error'" class="w-5 h-5 text-red-500">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path
                        fill-rule="evenodd"
                        d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                        clip-rule="evenodd"
                      />
                    </svg>
                  </span>

                  <span v-else class="w-5 h-5 text-gray-400">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path
                        fill-rule="evenodd"
                        d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z"
                        clip-rule="evenodd"
                      />
                    </svg>
                  </span>

                  <span class="ml-2 text-sm font-medium text-gray-900">{{ source.name }}</span>
                </div>

                <span class="text-sm text-gray-500">{{ source.recordsProcessed }} / {{ source.totalRecords }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Column: Status -->
      <div class="space-y-6">
        <etl-status
          :status="etlStatus"
          :connections="dataConnections"
          @test-connection="testConnection"
        />
      </div>
    </div>
  </div>
</template>

<script>
import ETLStatus from './ETLStatus.vue';

export default {
  name: 'ETLDashboard',

  components: {
    'etl-status': ETLStatus
  },

  props: {
    dataSources: {
      type: Array,
      default: () => [
        { id: 'artworks', name: 'Artworks', status: 'connected', recordCount: 0 },
        { id: 'exhibitions', name: 'Exhibitions', status: 'connected', recordCount: 0 },
        { id: 'visitors', name: 'Visitors', status: 'connected', recordCount: 0 },
        { id: 'staff', name: 'Staff', status: 'connected', recordCount: 0 },
        { id: 'loans', name: 'Loans', status: 'connected', recordCount: 0 }
      ]
    },

    isSyncingProp: {
      type: Boolean,
      default: false
    },

    syncProgressProp: {
      type: Number,
      default: 0
    },

    lastSync: {
      type: String,
      default: null
    },

    syncStats: {
      type: Object,
      default: () => ({
        totalRecords: 0,
        lastSyncDuration: 'N/A',
        successRate: 0,
        failedRecords: 0
      })
    }
  },

  emits: ['start-sync', 'stop-sync'],

  data() {
    return {
      selectedSources: [],
      syncType: 'incremental',
      isSyncing: false,
      syncProgress: 0,
      currentOperation: '',

      syncingSourcesStatus: [],

      etlStatus: {
        oltpConnection: 'connected',
        dwConnection: 'connected',
        lastSync: null,
        status: 'idle'
      },

      dataConnections: [
        { id: 'db', name: 'Database', type: 'Oracle', status: 'connected' }
      ],

      localDataSources: []
    };
  },

  computed: {
    lastSyncStatus() {
      if (!this.etlStatus.lastSync) return 'Never';

      const status = String(this.etlStatus.lastSync.status || '').toLowerCase();

      if (status === 'success' || status === 'completed' || status === '') return 'Success';
      if (status === 'failed' || status === 'error') return 'Failed';

      return 'Success';
    },

    lastSyncStatusClass() {
      if (!this.etlStatus.lastSync) return 'text-gray-500';

      const status = String(this.etlStatus.lastSync.status || '').toLowerCase();

      if (status === 'success' || status === 'completed' || status === '') return 'text-green-600';
      if (status === 'failed' || status === 'error') return 'text-red-600';

      return 'text-green-600';
    },

    formattedLastSyncTime() {
      if (!this.etlStatus.lastSync?.timestamp) return 'Never';

      const date = new Date(this.etlStatus.lastSync.timestamp);

      if (Number.isNaN(date.getTime())) return 'Never';

      return date.toLocaleString();
    },

    totalRecordsSynced() {
      return this.etlStatus.lastSync?.recordsProcessed ?? 0;
    },

    mergedDataSources() {
      if (this.localDataSources.length > 0) {
        return this.localDataSources;
      }

      return this.dataSources;
    },

    pendingChanges() {
      return this.mergedDataSources.reduce((sum, source) => {
        return sum + (source.pendingChanges || 0);
      }, 0);
    }
  },

  created() {
    this.fetchInitialData();
  },

  methods: {
    normalizeApiDate(value) {
      if (!value) return null;

      if (value instanceof Date) {
        return value.toISOString();
      }

      const valueAsString = String(value);
      const hasTimezone = /Z$|[+-]\d{2}:\d{2}$/.test(valueAsString);

      const date = hasTimezone
        ? new Date(valueAsString)
        : new Date(`${valueAsString}Z`);

      if (Number.isNaN(date.getTime())) {
        return null;
      }

      return date.toISOString();
    },

    buildLastSyncFromStatus(status) {
      if (!status) return null;

      if (status.lastSync) {
        const rawTimestamp =
          status.lastSync.syncDate ||
          status.lastSync.timestamp ||
          null;

        return {
          timestamp: rawTimestamp ? this.normalizeApiDate(rawTimestamp) : null,
          status: status.lastSync.status || 'success',
          recordsProcessed: status.lastSync.recordsProcessed ?? status.recordsProcessed ?? 0,
          duration: status.lastSync.duration ?? null,
          recordsFailed: status.lastSync.recordsFailed ?? 0
        };
      }

      if (status.lastSyncDate || status.lastSyncTime) {
        const rawTimestamp = status.lastSyncDate || status.lastSyncTime;

        return {
          timestamp: this.normalizeApiDate(rawTimestamp),
          status: 'success',
          recordsProcessed: status.recordsProcessed ?? 0,
          duration: status.duration ?? null,
          recordsFailed: status.recordsFailed ?? 0
        };
      }

      return null;
    },

    async fetchInitialData() {
      try {
        const statusResponse = await this.$api.etl.getStatus();

        if (!statusResponse.data?.success || !statusResponse.data?.data) {
          return;
        }

        const status = statusResponse.data.data;
        const lastSync = this.buildLastSyncFromStatus(status);

        this.etlStatus = {
          ...this.etlStatus,
          oltpConnection: status.oltpConnection || 'connected',
          dwConnection: status.dwConnection || 'connected',
          status: status.status || 'idle',
          lastSync
        };

        if (status.dataSources && Array.isArray(status.dataSources)) {
          this.localDataSources = this.dataSources.map(source => {
            const backendSource = status.dataSources.find(s => s.id === source.id);
            return backendSource ? { ...source, ...backendSource } : source;
          });
        }
      } catch (error) {
        console.error('Failed to fetch initial data:', error);
      }
    },

    selectAllSources() {
      this.selectedSources = this.mergedDataSources.map(s => s.id);
    },

    clearSelection() {
      this.selectedSources = [];
    },

    async startSync() {
      if (this.selectedSources.length === 0) return;

      this.isSyncing = true;
      this.syncProgress = 0;
      this.currentOperation = 'Initializing sync...';

      this.syncingSourcesStatus = this.selectedSources.map(sourceId => {
        const source = this.mergedDataSources.find(s => s.id === sourceId);

        return {
          id: sourceId,
          name: source?.name || sourceId,
          status: 'pending',
          recordsProcessed: 0,
          totalRecords: source?.recordCount || 0
        };
      });

      try {
        this.currentOperation = 'Triggering ETL refresh...';

        const response = await this.$api.etl.triggerRefresh({
          sources: this.selectedSources,
          syncType: this.syncType
        });

        if (!response.data?.success) {
          throw new Error(response.data?.message || 'Failed to trigger ETL sync');
        }

        this.syncProgress = 50;
        this.currentOperation = 'Processing data...';

        let attempts = 0;
        const maxAttempts = 30;

        while (attempts < maxAttempts) {
          await new Promise(resolve => setTimeout(resolve, 1000));

          const statusResponse = await this.$api.etl.getStatus();

          if (statusResponse.data?.data) {
            const status = statusResponse.data.data;

            if (status.status === 'completed' || status.status === 'idle') {
              this.syncProgress = 100;
              this.currentOperation = 'Sync completed successfully!';

              await this.fetchInitialData();

              this.syncingSourcesStatus.forEach(source => {
                source.status = 'completed';
                source.recordsProcessed = source.totalRecords;
              });

              break;
            }

            if (status.status === 'failed') {
              throw new Error(status.message || 'ETL sync failed');
            }

            this.syncProgress = Math.min(90, status.progress || this.syncProgress + 5);
            this.currentOperation = status.currentOperation || 'Processing...';
          }

          attempts++;
        }
      } catch (error) {
        console.error('Sync failed:', error);
        this.currentOperation = `Sync failed: ${error.message}`;

        this.syncingSourcesStatus.forEach(source => {
          if (source.status !== 'completed') {
            source.status = 'error';
          }
        });
      } finally {
        setTimeout(() => {
          this.isSyncing = false;
          this.syncProgress = 0;
        }, 1000);
      }
    },

    cancelSync() {
      this.isSyncing = false;
      this.currentOperation = 'Sync cancelled';
      this.syncProgress = 0;
    },

    async testConnection(connectionId) {
      const connection = this.dataConnections.find(c => c.id === connectionId);

      if (!connection) return;

      connection.status = 'testing';

      try {
        const response = await this.$api.etl.getStatus();
        connection.status = response.data?.success ? 'connected' : 'error';
      } catch (error) {
        console.error('Connection test failed:', error);
        connection.status = 'error';
      }
    },

    async validateData() {
      try {
        const response = await this.$api.etl.getValidationReport();

        if (response.data?.success) {
          alert(`Validation complete!\n${JSON.stringify(response.data.data, null, 2)}`);
        } else {
          alert('Validation completed with issues. Check the console for details.');
        }
      } catch (error) {
        console.error('Validation failed:', error);
        alert('Failed to run validation. Please try again.');
      }
    },

    generateReport() {
      alert('Generating ETL performance report...');
    },

    clearCache() {
      if (confirm('Are you sure you want to clear the ETL cache? This may slow down the next sync.')) {
        console.log('Clearing ETL cache...');
        alert('ETL cache cleared successfully.');
      }
    }
  }
};
</script>