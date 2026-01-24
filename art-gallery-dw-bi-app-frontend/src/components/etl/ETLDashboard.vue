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
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
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
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
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
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4" />
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
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
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
                  v-for="source in dataSources" 
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
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Syncing...
                </span>
                <span v-else>Start Sync</span>
              </button>
              <button
                @click="selectAllSources"
                class="btn btn-secondary"
              >
                Select All
              </button>
              <button
                @click="clearSelection"
                class="btn btn-secondary"
              >
                Clear Selection
              </button>
              <button
                v-if="isSyncing"
                @click="cancelSync"
                class="btn btn-danger"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>

        <!-- Progress Panel (visible during sync) -->
        <div v-if="isSyncing || syncProgress > 0" class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
            <h2 class="text-lg font-semibold text-gray-900">Sync Progress</h2>
          </div>
          <div class="p-6">
            <!-- Overall Progress -->
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

            <!-- Current Operation -->
            <div v-if="currentOperation" class="p-4 bg-blue-50 rounded-lg">
              <p class="text-sm text-blue-700">
                <strong>Current:</strong> {{ currentOperation }}
              </p>
            </div>

            <!-- Source Progress List -->
            <div class="mt-4 space-y-3">
              <div 
                v-for="source in syncingSourcesStatus" 
                :key="source.id"
                class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
              >
                <div class="flex items-center">
                  <span v-if="source.status === 'completed'" class="w-5 h-5 text-green-500">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
                    </svg>
                  </span>
                  <span v-else-if="source.status === 'syncing'" class="w-5 h-5 text-blue-500 animate-spin">
                    <svg fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                    </svg>
                  </span>
                  <span v-else-if="source.status === 'error'" class="w-5 h-5 text-red-500">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
                    </svg>
                  </span>
                  <span v-else class="w-5 h-5 text-gray-400">
                    <svg fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clip-rule="evenodd"/>
                    </svg>
                  </span>
                  <span class="ml-2 text-sm font-medium text-gray-900">{{ source.name }}</span>
                </div>
                <span class="text-sm text-gray-500">{{ source.recordsProcessed }} / {{ source.totalRecords }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- ETL Log Component -->
        <etl-log 
          :logs="recentLogs"
          :is-loading="isLoadingLogs"
          @refresh="fetchLogs"
          @view-all="viewAllLogs"
        />
      </div>

      <!-- Right Column: Status & Schedule -->
      <div class="space-y-6">
        <!-- ETL Status Component -->
        <etl-status 
          :status="etlStatus"
          :connections="dataConnections"
          @test-connection="testConnection"
        />

        <!-- Schedule Panel -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
            <h2 class="text-lg font-semibold text-gray-900">Sync Schedule</h2>
          </div>
          <div class="p-6">
            <!-- Auto Sync Toggle -->
            <div class="flex items-center justify-between mb-4">
              <span class="text-sm font-medium text-gray-700">Auto Sync</span>
              <button
                @click="toggleAutoSync"
                :class="[
                  'relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
                  autoSyncEnabled ? 'bg-primary-600' : 'bg-gray-200'
                ]"
              >
                <span
                  :class="[
                    'inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
                    autoSyncEnabled ? 'translate-x-6' : 'translate-x-1'
                  ]"
                ></span>
              </button>
            </div>

            <!-- Schedule Options -->
            <div v-if="autoSyncEnabled" class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Frequency</label>
                <select v-model="syncFrequency" class="form-input">
                  <option value="hourly">Every Hour</option>
                  <option value="daily">Daily</option>
                  <option value="weekly">Weekly</option>
                </select>
              </div>

              <div v-if="syncFrequency === 'daily' || syncFrequency === 'weekly'">
                <label class="block text-sm font-medium text-gray-700 mb-1">Time</label>
                <input type="time" v-model="syncTime" class="form-input" />
              </div>

              <div v-if="syncFrequency === 'weekly'">
                <label class="block text-sm font-medium text-gray-700 mb-1">Day</label>
                <select v-model="syncDay" class="form-input">
                  <option v-for="day in weekDays" :key="day.value" :value="day.value">
                    {{ day.label }}
                  </option>
                </select>
              </div>

              <div class="pt-4 border-t border-gray-100">
                <p class="text-sm text-gray-600">
                  <strong>Next scheduled sync:</strong><br />
                  {{ nextScheduledSync }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
            <h2 class="text-lg font-semibold text-gray-900">Quick Actions</h2>
          </div>
          <div class="p-4 space-y-2">
            <button @click="validateData" class="w-full btn btn-secondary justify-start">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Validate Data Integrity
            </button>
            <button @click="generateReport" class="w-full btn btn-secondary justify-start">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              Generate ETL Report
            </button>
            <button @click="clearCache" class="w-full btn btn-secondary justify-start">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
              Clear ETL Cache
            </button>
            <button @click="viewDWSchema" class="w-full btn btn-secondary justify-start">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4" />
              </svg>
              View DW Schema
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions, mapGetters } from 'vuex';
import ETLStatus from './ETLStatus.vue';
import ETLLog from './ETLLog.vue';

/**
 * ETLDashboard Component
 * Main control center for ETL operations
 */
export default {
  name: 'ETLDashboard',

  components: {
    'etl-status': ETLStatus,
    'etl-log': ETLLog
  },

  data() {
    return {
      // Sync controls
      selectedSources: [],
      syncType: 'incremental',
      isSyncing: false,
      syncProgress: 0,
      currentOperation: '',
      
      // Schedule settings
      autoSyncEnabled: false,
      syncFrequency: 'daily',
      syncTime: '02:00',
      syncDay: 'sunday',
      
      // Data sources configuration
      dataSources: [
        { id: 'artworks', name: 'Artworks', recordCount: 0 },
        { id: 'artists', name: 'Artists', recordCount: 0 },
        { id: 'exhibitions', name: 'Exhibitions', recordCount: 0 },
        { id: 'visitors', name: 'Visitors', recordCount: 0 },
        { id: 'sales', name: 'Sales', recordCount: 0 },
        { id: 'loans', name: 'Loans', recordCount: 0 }
      ],
      
      // Syncing status per source
      syncingSourcesStatus: [],
      
      // Week days for schedule
      weekDays: [
        { value: 'sunday', label: 'Sunday' },
        { value: 'monday', label: 'Monday' },
        { value: 'tuesday', label: 'Tuesday' },
        { value: 'wednesday', label: 'Wednesday' },
        { value: 'thursday', label: 'Thursday' },
        { value: 'friday', label: 'Friday' },
        { value: 'saturday', label: 'Saturday' }
      ],
      
      // Logs
      recentLogs: [],
      isLoadingLogs: false,
      
      // ETL Status
      etlStatus: {
        oltpConnection: 'connected',
        dwConnection: 'connected',
        lastSync: null,
        status: 'idle'
      },
      
      // Data connections
      dataConnections: [
        { id: 'oltp', name: 'OLTP Database', type: 'PostgreSQL', status: 'connected' },
        { id: 'dw', name: 'Data Warehouse', type: 'PostgreSQL', status: 'connected' },
        { id: 'staging', name: 'Staging Area', type: 'Redis', status: 'connected' }
      ],

      // Sync interval reference
      syncInterval: null
    };
  },

  computed: {
    lastSyncStatus() {
      if (!this.etlStatus.lastSync) return 'Never';
      return this.etlStatus.lastSync.status === 'success' ? 'Success' : 'Failed';
    },

    lastSyncStatusClass() {
      if (!this.etlStatus.lastSync) return 'text-gray-500';
      return this.etlStatus.lastSync.status === 'success' ? 'text-green-600' : 'text-red-600';
    },

    formattedLastSyncTime() {
      if (!this.etlStatus.lastSync?.timestamp) return 'Never';
      return new Date(this.etlStatus.lastSync.timestamp).toLocaleString();
    },

    totalRecordsSynced() {
      if (!this.etlStatus.lastSync) return 0;
      return this.etlStatus.lastSync.recordsProcessed || 0;
    },

    pendingChanges() {
      // Calculate pending changes from data sources
      return this.dataSources.reduce((sum, source) => {
        return sum + (source.pendingChanges || 0);
      }, 0);
    },

    nextScheduledSync() {
      if (!this.autoSyncEnabled) return 'Auto sync disabled';
      
      const now = new Date();
      let nextSync = new Date();
      
      if (this.syncFrequency === 'hourly') {
        nextSync.setHours(nextSync.getHours() + 1, 0, 0, 0);
      } else if (this.syncFrequency === 'daily') {
        const [hours, minutes] = this.syncTime.split(':');
        nextSync.setHours(parseInt(hours), parseInt(minutes), 0, 0);
        if (nextSync <= now) {
          nextSync.setDate(nextSync.getDate() + 1);
        }
      } else if (this.syncFrequency === 'weekly') {
        const dayIndex = this.weekDays.findIndex(d => d.value === this.syncDay);
        const currentDay = now.getDay();
        let daysUntil = dayIndex - currentDay;
        if (daysUntil <= 0) daysUntil += 7;
        
        const [hours, minutes] = this.syncTime.split(':');
        nextSync.setDate(now.getDate() + daysUntil);
        nextSync.setHours(parseInt(hours), parseInt(minutes), 0, 0);
      }
      
      return nextSync.toLocaleString();
    }
  },

  watch: {
    autoSyncEnabled(newValue) {
      if (newValue) {
        this.setupAutoSync();
      } else {
        this.clearAutoSync();
      }
    },

    syncFrequency() {
      if (this.autoSyncEnabled) {
        this.setupAutoSync();
      }
    }
  },

  created() {
    this.fetchInitialData();
  },

  mounted() {
    this.fetchLogs();
    this.loadScheduleSettings();
  },

  beforeUnmount() {
    this.clearAutoSync();
  },

  methods: {
    ...mapActions('reports', ['fetchETLStatus', 'runETLSync']),

    async fetchInitialData() {
      try {
        // Fetch record counts for each data source
        // In production, this would call the API
        this.dataSources = this.dataSources.map(source => ({
          ...source,
          recordCount: Math.floor(Math.random() * 1000) + 100,
          pendingChanges: Math.floor(Math.random() * 50)
        }));

        // Fetch ETL status
        this.etlStatus = {
          ...this.etlStatus,
          lastSync: {
            timestamp: new Date(Date.now() - 3600000).toISOString(),
            status: 'success',
            recordsProcessed: 1547
          }
        };
      } catch (error) {
        console.error('Failed to fetch initial data:', error);
      }
    },

    async fetchLogs() {
      this.isLoadingLogs = true;
      try {
        // Simulate fetching logs
        await new Promise(resolve => setTimeout(resolve, 500));
        this.recentLogs = [
          { id: 1, timestamp: new Date().toISOString(), level: 'info', message: 'ETL sync completed successfully', source: 'artworks' },
          { id: 2, timestamp: new Date(Date.now() - 3600000).toISOString(), level: 'info', message: 'Processing 245 artwork records', source: 'artworks' },
          { id: 3, timestamp: new Date(Date.now() - 7200000).toISOString(), level: 'warning', message: 'Slow query detected during visitor sync', source: 'visitors' },
          { id: 4, timestamp: new Date(Date.now() - 10800000).toISOString(), level: 'info', message: 'Full sync initiated by admin', source: 'system' },
          { id: 5, timestamp: new Date(Date.now() - 14400000).toISOString(), level: 'error', message: 'Connection timeout - retrying', source: 'exhibitions' }
        ];
      } catch (error) {
        console.error('Failed to fetch logs:', error);
      } finally {
        this.isLoadingLogs = false;
      }
    },

    selectAllSources() {
      this.selectedSources = this.dataSources.map(s => s.id);
    },

    clearSelection() {
      this.selectedSources = [];
    },

    async startSync() {
      if (this.selectedSources.length === 0) return;

      this.isSyncing = true;
      this.syncProgress = 0;
      this.currentOperation = 'Initializing sync...';

      // Initialize syncing status for each selected source
      this.syncingSourcesStatus = this.selectedSources.map(sourceId => {
        const source = this.dataSources.find(s => s.id === sourceId);
        return {
          id: sourceId,
          name: source.name,
          status: 'pending',
          recordsProcessed: 0,
          totalRecords: source.recordCount
        };
      });

      try {
        // Simulate sync process
        for (let i = 0; i < this.syncingSourcesStatus.length; i++) {
          const source = this.syncingSourcesStatus[i];
          source.status = 'syncing';
          this.currentOperation = `Syncing ${source.name}...`;

          // Simulate processing records
          const steps = 10;
          for (let step = 1; step <= steps; step++) {
            await new Promise(resolve => setTimeout(resolve, 200));
            source.recordsProcessed = Math.floor((step / steps) * source.totalRecords);
            this.syncProgress = Math.floor(((i * steps + step) / (this.syncingSourcesStatus.length * steps)) * 100);
          }

          source.status = 'completed';
          source.recordsProcessed = source.totalRecords;
        }

        this.currentOperation = 'Sync completed successfully!';
        this.etlStatus.lastSync = {
          timestamp: new Date().toISOString(),
          status: 'success',
          recordsProcessed: this.syncingSourcesStatus.reduce((sum, s) => sum + s.totalRecords, 0)
        };

        // Add log entry
        this.recentLogs.unshift({
          id: Date.now(),
          timestamp: new Date().toISOString(),
          level: 'info',
          message: `ETL sync completed: ${this.syncingSourcesStatus.length} sources, ${this.etlStatus.lastSync.recordsProcessed} records`,
          source: 'system'
        });

      } catch (error) {
        console.error('Sync failed:', error);
        this.currentOperation = 'Sync failed!';
      } finally {
        setTimeout(() => {
          this.isSyncing = false;
        }, 1000);
      }
    },

    cancelSync() {
      this.isSyncing = false;
      this.currentOperation = 'Sync cancelled';
      this.syncProgress = 0;
    },

    toggleAutoSync() {
      this.autoSyncEnabled = !this.autoSyncEnabled;
    },

    setupAutoSync() {
      this.clearAutoSync();
      // In production, this would set up actual scheduled jobs
      console.log('Auto sync enabled with frequency:', this.syncFrequency);
    },

    clearAutoSync() {
      if (this.syncInterval) {
        clearInterval(this.syncInterval);
        this.syncInterval = null;
      }
    },

    loadScheduleSettings() {
      // Load settings from localStorage
      const settings = localStorage.getItem('etl_schedule_settings');
      if (settings) {
        const parsed = JSON.parse(settings);
        this.autoSyncEnabled = parsed.autoSyncEnabled || false;
        this.syncFrequency = parsed.syncFrequency || 'daily';
        this.syncTime = parsed.syncTime || '02:00';
        this.syncDay = parsed.syncDay || 'sunday';
      }
    },

    async testConnection(connectionId) {
      const connection = this.dataConnections.find(c => c.id === connectionId);
      if (connection) {
        connection.status = 'testing';
        await new Promise(resolve => setTimeout(resolve, 1000));
        connection.status = 'connected';
      }
    },

    validateData() {
      alert('Data validation started. This will check data integrity between OLTP and DW.');
    },

    generateReport() {
      alert('Generating ETL performance report...');
    },

    clearCache() {
      if (confirm('Are you sure you want to clear the ETL cache? This may slow down the next sync.')) {
        alert('ETL cache cleared successfully.');
      }
    },

    viewDWSchema() {
      this.$router.push({ name: 'dw-schema' });
    },

    viewAllLogs() {
      this.$router.push({ name: 'etl-logs' });
    }
  }
};
</script>
