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

    <!-- ETL is available only for OLTP/DW context -->
    <div
      v-if="!isOltpSchema"
      class="bg-white rounded-xl shadow-sm border border-gray-100 p-8"
    >
      <div class="flex items-start space-x-4">
        <div class="w-12 h-12 rounded-lg bg-gray-100 flex items-center justify-center text-2xl">
          ℹ️
        </div>

        <div class="flex-1">
          <div class="flex items-center justify-between mb-2">
            <h2 class="text-xl font-semibold text-gray-900">
              ETL not available for this schema
            </h2>

            <span class="px-3 py-1 text-sm font-medium rounded-full bg-gray-100 text-gray-700">
              Schema: {{ currentSchema }}
            </span>
          </div>

          <p class="text-gray-600 leading-relaxed">
            ETL synchronization is available only for the OLTP/DW context.
            The selected schema is a BDD schema fragment/view, so ETL operations
            are disabled here.
          </p>

          <div class="mt-6 p-4 rounded-lg bg-blue-50 border border-blue-100">
            <p class="text-sm text-blue-800">
              Switch the schema selector back to <strong>OLTP</strong> to manage
              ETL synchronization, sync history, and DW refresh operations.
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- ETL Dashboard Component -->
    <etl-dashboard
      v-else
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

  computed: {
    currentSchema() {
      return this.$store?.state?.dataSource?.source || 'OLTP';
    },

    isOltpSchema() {
      return this.currentSchema === 'OLTP';
    }
  },

  watch: {
    currentSchema() {
      if (this.isOltpSchema) {
        this.fetchETLStatus();
      } else {
        this.resetEtlState();
      }
    }
  },

  created() {
    if (this.isOltpSchema) {
      this.fetchETLStatus();
    } else {
      this.resetEtlState();
    }
  },

  methods: {
    resetEtlState() {
      this.isSyncing = false;
      this.syncProgress = 0;
      this.lastSync = null;

      this.syncStats = {
        totalRecords: 0,
        lastSyncDuration: 'N/A',
        successRate: 0,
        failedRecords: 0
      };
    },

    async fetchETLStatus() {
      if (!this.isOltpSchema) {
        this.resetEtlState();
        return;
      }

      try {
        const response = await this.$api.etl.getStatus();

        if (!response.data?.success || !response.data?.data) {
          return;
        }

        const status = response.data.data;
        const lastSync = status.lastSync || null;

        const rawSyncDate =
          lastSync?.syncDate ||
          lastSync?.timestamp ||
          null;

        this.lastSync = rawSyncDate
          ? this.normalizeApiDate(rawSyncDate)
          : null;

        if (status.dataSources && Array.isArray(status.dataSources)) {
          this.dataSources = status.dataSources.map(source => ({
            id: source.id,
            name: source.name,
            status: source.status || 'connected',
            recordCount: source.recordCount ?? 0
          }));
        }

        this.syncStats = {
          // Latest sync affected records, not total source records.
          totalRecords: lastSync?.recordsProcessed ?? 0,

          lastSyncDuration: lastSync?.duration != null
            ? `${lastSync.duration}ms`
            : 'N/A',

          successRate: status.successRate ?? status.stats?.successRate ?? 0,

          failedRecords: lastSync?.recordsFailed ?? status.stats?.failedRecords ?? 0
        };
      } catch (error) {
        console.error('Failed to fetch ETL status:', error);
      }
    },

    async startSync(sourceId) {
      if (!this.isOltpSchema) {
        return;
      }

      this.isSyncing = true;
      this.syncProgress = 0;

      try {
        const response = await this.$api.etl.triggerRefresh(sourceId);

        if (response.data?.success) {
          const pollInterval = setInterval(async () => {
            try {
              const statusResponse = await this.$api.etl.getStatus();

              if (statusResponse.data?.data) {
                const status = statusResponse.data.data;

                this.syncProgress = status.progress || Math.min(this.syncProgress + 10, 95);

                if (
                  status.status === 'completed' ||
                  status.status === 'idle' ||
                  this.syncProgress >= 100
                ) {
                  this.syncProgress = 100;
                  clearInterval(pollInterval);

                  await this.fetchETLStatus();

                  setTimeout(() => {
                    this.isSyncing = false;
                    this.syncProgress = 0;
                  }, 500);
                }
              }
            } catch (err) {
              console.error('Poll error:', err);
              clearInterval(pollInterval);
              this.isSyncing = false;
              this.syncProgress = 0;
            }
          }, 1000);
        } else {
          this.isSyncing = false;
          this.syncProgress = 0;
        }
      } catch (error) {
        console.error('Sync failed:', error);
        this.isSyncing = false;
        this.syncProgress = 0;
      }
    },

    stopSync() {
      this.isSyncing = false;
      this.syncProgress = 0;
    },

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