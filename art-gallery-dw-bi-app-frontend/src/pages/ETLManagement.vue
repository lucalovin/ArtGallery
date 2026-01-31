<template>
  <!--
    ETLManagement.vue - ETL Management Page
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-management-page">
    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">ETL Management</h1>
      <p class="text-gray-500 mt-1">Data Warehouse synchronization and ETL operations</p>
    </header>

    <!-- ETL Dashboard Component -->
    <etl-dashboard
      :data-sources="dataSources"
      :is-syncing="isSyncing"
      :sync-progress="syncProgress"
      :last-sync="lastSync"
      :sync-stats="syncStats"
      @start-sync="startSync"
      @stop-sync="stopSync"
    />
  </div>
</template>

<script>
import ETLDashboard from '@/components/etl/ETLDashboard.vue';

/**
 * ETLManagement Page
 */
export default {
  name: 'ETLManagementPage',

  components: {
    'etl-dashboard': ETLDashboard
  },

  data() {
    return {
      isSyncing: false,
      syncProgress: 0,
      lastSync: null,
      dataSources: [
        { id: 'artworks', name: 'Artworks', status: 'connected', recordCount: 0 },
        { id: 'exhibitions', name: 'Exhibitions', status: 'connected', recordCount: 0 },
        { id: 'visitors', name: 'Visitors', status: 'connected', recordCount: 0 },
        { id: 'staff', name: 'Staff', status: 'connected', recordCount: 0 },
        { id: 'loans', name: 'Loans', status: 'connected', recordCount: 0 }
      ],
      syncStats: {
        totalRecords: 0,
        lastSyncDuration: 'N/A',
        successRate: 0,
        failedRecords: 0
      }
    };
  },

  created() {
    this.fetchETLStatus();
  },

  methods: {
    async fetchETLStatus() {
      try {
        const response = await this.$api.etl.getStatus();
        if (response.data?.success && response.data?.data) {
          const status = response.data.data;
          this.lastSync = status.lastSync?.syncDate || status.lastSync?.timestamp;
          
          // Use data sources from backend if provided
          if (status.dataSources && Array.isArray(status.dataSources)) {
            this.dataSources = status.dataSources.map(source => ({
              id: source.id,
              name: source.name,
              status: source.status || 'connected',
              recordCount: source.recordCount || 0
            }));
          }

          // Update sync stats
          if (status.stats) {
            this.syncStats = {
              totalRecords: status.stats.totalRecords || 0,
              lastSyncDuration: status.stats.duration || 'N/A',
              successRate: status.stats.successRate || 0,
              failedRecords: status.stats.failedRecords || 0
            };
          }
        }
      } catch (error) {
        console.error('Failed to fetch ETL status:', error);
      }
    },

    async startSync(sourceId) {
      this.isSyncing = true;
      this.syncProgress = 0;

      try {
        const response = await this.$api.etl.triggerRefresh();
        if (response.data?.success) {
          // Poll for progress
          const pollInterval = setInterval(async () => {
            try {
              const statusResponse = await this.$api.etl.getStatus();
              if (statusResponse.data?.data) {
                const status = statusResponse.data.data;
                this.syncProgress = status.progress || this.syncProgress + 10;
                
                if (status.status === 'completed' || status.status === 'idle' || this.syncProgress >= 100) {
                  this.syncProgress = 100;
                  clearInterval(pollInterval);
                  this.lastSync = new Date().toISOString();
                  await this.fetchETLStatus();
                  setTimeout(() => {
                    this.isSyncing = false;
                  }, 500);
                }
              }
            } catch (err) {
              console.error('Poll error:', err);
            }
          }, 1000);
        }
      } catch (error) {
        console.error('Sync failed:', error);
        this.isSyncing = false;
      }
    },

    stopSync() {
      this.isSyncing = false;
      this.syncProgress = 0;
    }
  }
};
</script>

<style scoped>
.etl-management-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
