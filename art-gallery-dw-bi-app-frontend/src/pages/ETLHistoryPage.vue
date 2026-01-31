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
      try {
        const response = await this.$api.etl.getHistory({ limit: 50 });
        if (response.data?.success && response.data?.data) {
          this.historyData = response.data.data.map((item, index) => ({
            id: item.id || index + 1,
            timestamp: item.timestamp || item.startTime,
            source: item.source || 'All Sources',
            status: item.status || 'unknown',
            recordsProcessed: item.recordsProcessed || 0,
            duration: item.duration || 'N/A',
            errors: item.errors || 0
          }));
        }
      } catch (error) {
        console.error('Failed to load history:', error);
        this.historyData = [];
      } finally {
        this.isLoading = false;
      }
    },

    async retrySync(syncId) {
      console.log('Retrying sync:', syncId);
      try {
        await this.$api.etl.triggerRefresh();
        // Reload history after trigger
        setTimeout(() => this.loadHistory(), 2000);
      } catch (error) {
        console.error('Retry failed:', error);
      }
    },

    exportLogs() {
      console.log('Exporting logs...');
      // TODO: Implement log export
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
