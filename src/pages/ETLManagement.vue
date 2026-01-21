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
      lastSync: '2024-01-20T14:30:00',
      dataSources: [
        { id: 'artworks', name: 'Artworks', status: 'connected', recordCount: 1247 },
        { id: 'exhibitions', name: 'Exhibitions', status: 'connected', recordCount: 85 },
        { id: 'visitors', name: 'Visitors', status: 'connected', recordCount: 15420 },
        { id: 'staff', name: 'Staff', status: 'connected', recordCount: 42 },
        { id: 'loans', name: 'Loans', status: 'connected', recordCount: 156 }
      ],
      syncStats: {
        totalRecords: 16950,
        lastSyncDuration: '2m 34s',
        successRate: 99.8,
        failedRecords: 3
      }
    };
  },

  methods: {
    startSync(sourceId) {
      this.isSyncing = true;
      this.syncProgress = 0;

      const interval = setInterval(() => {
        this.syncProgress += Math.random() * 15;
        if (this.syncProgress >= 100) {
          this.syncProgress = 100;
          clearInterval(interval);
          setTimeout(() => {
            this.isSyncing = false;
            this.lastSync = new Date().toISOString();
          }, 500);
        }
      }, 300);
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
