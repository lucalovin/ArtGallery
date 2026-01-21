<template>
  <!--
    ETLHistoryPage.vue - ETL Sync History Page
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-history-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to ETL Dashboard
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">ETL Sync History</h1>
      <p class="text-gray-500 mt-1">View historical ETL synchronization operations</p>
    </header>

    <etl-history
      :history-data="historyData"
      :is-loading="isLoading"
      @retry-sync="retrySync"
      @export-logs="exportLogs"
    />
  </div>
</template>

<script>
import ETLHistory from '@/components/etl/ETLHistory.vue';

/**
 * ETLHistoryPage
 */
export default {
  name: 'ETLHistoryPage',

  components: {
    'etl-history': ETLHistory
  },

  data() {
    return {
      isLoading: true,
      historyData: []
    };
  },

  created() {
    this.loadHistory();
  },

  methods: {
    goBack() {
      this.$router.push('/etl');
    },

    async loadHistory() {
      this.isLoading = true;
      await new Promise(resolve => setTimeout(resolve, 500));
      
      this.historyData = [
        { id: 1, timestamp: '2024-01-20T14:30:00', source: 'All Sources', status: 'success', recordsProcessed: 16950, duration: '2m 34s', errors: 0 },
        { id: 2, timestamp: '2024-01-20T12:00:00', source: 'Artworks', status: 'success', recordsProcessed: 1247, duration: '45s', errors: 0 },
        { id: 3, timestamp: '2024-01-20T08:00:00', source: 'All Sources', status: 'partial', recordsProcessed: 16947, duration: '2m 48s', errors: 3 },
        { id: 4, timestamp: '2024-01-19T18:00:00', source: 'Visitors', status: 'success', recordsProcessed: 15420, duration: '1m 12s', errors: 0 },
        { id: 5, timestamp: '2024-01-19T12:00:00', source: 'All Sources', status: 'failed', recordsProcessed: 0, duration: '5s', errors: 1 }
      ];
      
      this.isLoading = false;
    },

    retrySync(syncId) {
      console.log('Retrying sync:', syncId);
    },

    exportLogs() {
      console.log('Exporting logs...');
    }
  }
};
</script>

<style scoped>
.etl-history-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
