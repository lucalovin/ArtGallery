<template>
  <!--
    ETLHistory.vue - ETL Sync History Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-history bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
      <div>
        <h2 class="text-lg font-semibold text-gray-900">Sync History</h2>
        <p class="text-sm text-gray-500 mt-1">View past ETL operations and performance metrics</p>
      </div>
      <div class="flex items-center space-x-3">
        <!-- Date Range Filter -->
        <select v-model="dateRange" class="form-input text-sm">
          <option value="7">Last 7 days</option>
          <option value="30">Last 30 days</option>
          <option value="90">Last 90 days</option>
          <option value="all">All time</option>
        </select>
        <button
          @click="exportHistory"
          class="btn btn-secondary btn-sm"
        >
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Export
        </button>
      </div>
    </div>

    <!-- Summary Stats -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="text-center">
          <p class="text-2xl font-bold text-gray-900">{{ summaryStats.totalSyncs }}</p>
          <p class="text-xs text-gray-500">Total Syncs</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-green-600">{{ summaryStats.successRate }}%</p>
          <p class="text-xs text-gray-500">Success Rate</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-gray-900">{{ formatNumber(summaryStats.totalRecords) }}</p>
          <p class="text-xs text-gray-500">Records Processed</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-gray-900">{{ summaryStats.avgDuration }}</p>
          <p class="text-xs text-gray-500">Avg Duration</p>
        </div>
      </div>
    </div>

    <!-- History Table -->
    <div class="overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th 
              v-for="column in columns"
              :key="column.key"
              class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              @click="sortBy(column.key)"
            >
              <div class="flex items-center">
                {{ column.label }}
                <svg 
                  v-if="sortColumn === column.key"
                  class="w-4 h-4 ml-1"
                  :class="{ 'rotate-180': sortDirection === 'desc' }"
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
                </svg>
              </div>
            </th>
            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr 
            v-for="sync in paginatedHistory" 
            :key="sync.id"
            class="hover:bg-gray-50"
          >
            <!-- Timestamp -->
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="text-sm text-gray-900">{{ formatDate(sync.startTime) }}</div>
              <div class="text-xs text-gray-500">{{ formatTime(sync.startTime) }}</div>
            </td>

            <!-- Type -->
            <td class="px-6 py-4 whitespace-nowrap">
              <span 
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="sync.type === 'full' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800'"
              >
                {{ sync.type === 'full' ? 'Full Refresh' : 'Incremental' }}
              </span>
            </td>

            <!-- Sources -->
            <td class="px-6 py-4">
              <div class="flex flex-wrap gap-1">
                <span 
                  v-for="source in sync.sources.slice(0, 3)" 
                  :key="source"
                  class="px-2 py-0.5 text-xs bg-gray-100 text-gray-600 rounded"
                >
                  {{ source }}
                </span>
                <span 
                  v-if="sync.sources.length > 3"
                  class="px-2 py-0.5 text-xs bg-gray-100 text-gray-600 rounded"
                >
                  +{{ sync.sources.length - 3 }} more
                </span>
              </div>
            </td>

            <!-- Status -->
            <td class="px-6 py-4 whitespace-nowrap">
              <span :class="getStatusClasses(sync.status)">
                {{ sync.status }}
              </span>
            </td>

            <!-- Records -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
              {{ formatNumber(sync.recordsProcessed) }}
            </td>

            <!-- Duration -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
              {{ formatDuration(sync.duration) }}
            </td>

            <!-- Actions -->
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
              <button
                @click="viewDetails(sync)"
                class="text-primary-600 hover:text-primary-800 mr-3"
              >
                Details
              </button>
              <button
                v-if="sync.status === 'failed'"
                @click="retrySyncClick(sync)"
                class="text-yellow-600 hover:text-yellow-800"
              >
                Retry
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Empty State -->
    <div v-if="filteredHistory.length === 0" class="py-12 text-center">
      <svg class="w-16 h-16 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <p class="mt-4 text-gray-500">No sync history found for the selected period</p>
    </div>

    <!-- Pagination -->
    <div class="px-6 py-4 border-t border-gray-100 flex items-center justify-between">
      <div class="text-sm text-gray-500">
        Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredHistory.length }} entries
      </div>
      <div class="flex items-center space-x-2">
        <button
          @click="prevPage"
          :disabled="currentPage === 1"
          class="px-3 py-1 border border-gray-300 rounded-md text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Previous
        </button>
        <span class="text-sm text-gray-600">
          Page {{ currentPage }} of {{ totalPages }}
        </span>
        <button
          @click="nextPage"
          :disabled="currentPage === totalPages"
          class="px-3 py-1 border border-gray-300 rounded-md text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Next
        </button>
      </div>
    </div>

    <!-- Details Modal -->
    <div 
      v-if="selectedSync" 
      class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
      @click.self="closeDetails"
    >
      <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[80vh] overflow-y-auto">
        <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900">Sync Details</h3>
          <button @click="closeDetails" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div class="p-6 space-y-6">
          <!-- Status Banner -->
          <div 
            class="p-4 rounded-lg"
            :class="selectedSync.status === 'success' ? 'bg-green-50' : selectedSync.status === 'failed' ? 'bg-red-50' : 'bg-yellow-50'"
          >
            <div class="flex items-center">
              <svg v-if="selectedSync.status === 'success'" class="w-6 h-6 text-green-500 mr-3" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
              </svg>
              <svg v-else class="w-6 h-6 text-red-500 mr-3" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
              </svg>
              <div>
                <p class="font-medium" :class="selectedSync.status === 'success' ? 'text-green-800' : 'text-red-800'">
                  {{ selectedSync.status === 'success' ? 'Sync Completed Successfully' : 'Sync Failed' }}
                </p>
                <p v-if="selectedSync.error" class="text-sm text-red-600 mt-1">{{ selectedSync.error }}</p>
              </div>
            </div>
          </div>

          <!-- Details Grid -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-sm text-gray-500">Start Time</p>
              <p class="font-medium">{{ formatDateTime(selectedSync.startTime) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">End Time</p>
              <p class="font-medium">{{ formatDateTime(selectedSync.endTime) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Duration</p>
              <p class="font-medium">{{ formatDuration(selectedSync.duration) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Sync Type</p>
              <p class="font-medium">{{ selectedSync.type === 'full' ? 'Full Refresh' : 'Incremental' }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Records Processed</p>
              <p class="font-medium">{{ formatNumber(selectedSync.recordsProcessed) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Initiated By</p>
              <p class="font-medium">{{ selectedSync.initiatedBy || 'System' }}</p>
            </div>
          </div>

          <!-- Sources Breakdown -->
          <div>
            <h4 class="text-sm font-medium text-gray-700 mb-3">Sources Breakdown</h4>
            <div class="space-y-2">
              <div 
                v-for="(detail, source) in selectedSync.sourceDetails" 
                :key="source"
                class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
              >
                <span class="text-sm font-medium">{{ source }}</span>
                <div class="flex items-center space-x-4 text-sm">
                  <span class="text-gray-500">{{ detail.records }} records</span>
                  <span :class="detail.status === 'success' ? 'text-green-600' : 'text-red-600'">
                    {{ detail.status }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ETLHistory Component
 * Displays historical ETL sync operations
 */
export default {
  name: 'ETLHistory',

  props: {
    history: {
      type: Array,
      default: () => []
    }
  },

  emits: ['retry'],

  data() {
    return {
      dateRange: '30',
      sortColumn: 'startTime',
      sortDirection: 'desc',
      currentPage: 1,
      itemsPerPage: 10,
      selectedSync: null,
      columns: [
        { key: 'startTime', label: 'Date/Time' },
        { key: 'type', label: 'Type' },
        { key: 'sources', label: 'Sources' },
        { key: 'status', label: 'Status' },
        { key: 'recordsProcessed', label: 'Records' },
        { key: 'duration', label: 'Duration' }
      ],
      // Sample data for demonstration
      sampleHistory: [
        {
          id: 1,
          startTime: new Date().toISOString(),
          endTime: new Date(Date.now() + 120000).toISOString(),
          type: 'incremental',
          sources: ['artworks', 'artists', 'sales'],
          status: 'success',
          recordsProcessed: 1547,
          duration: 120,
          initiatedBy: 'System',
          sourceDetails: {
            artworks: { records: 542, status: 'success' },
            artists: { records: 128, status: 'success' },
            sales: { records: 877, status: 'success' }
          }
        },
        {
          id: 2,
          startTime: new Date(Date.now() - 86400000).toISOString(),
          endTime: new Date(Date.now() - 86400000 + 300000).toISOString(),
          type: 'full',
          sources: ['artworks', 'artists', 'exhibitions', 'visitors', 'sales', 'loans'],
          status: 'success',
          recordsProcessed: 4521,
          duration: 300,
          initiatedBy: 'Admin User',
          sourceDetails: {
            artworks: { records: 542, status: 'success' },
            artists: { records: 128, status: 'success' },
            exhibitions: { records: 45, status: 'success' },
            visitors: { records: 2341, status: 'success' },
            sales: { records: 877, status: 'success' },
            loans: { records: 67, status: 'success' }
          }
        },
        {
          id: 3,
          startTime: new Date(Date.now() - 172800000).toISOString(),
          endTime: new Date(Date.now() - 172800000 + 45000).toISOString(),
          type: 'incremental',
          sources: ['visitors'],
          status: 'failed',
          recordsProcessed: 234,
          duration: 45,
          error: 'Connection timeout after 45 seconds',
          initiatedBy: 'System',
          sourceDetails: {
            visitors: { records: 234, status: 'failed' }
          }
        },
        {
          id: 4,
          startTime: new Date(Date.now() - 259200000).toISOString(),
          endTime: new Date(Date.now() - 259200000 + 180000).toISOString(),
          type: 'incremental',
          sources: ['artworks', 'sales'],
          status: 'success',
          recordsProcessed: 1089,
          duration: 180,
          initiatedBy: 'System',
          sourceDetails: {
            artworks: { records: 212, status: 'success' },
            sales: { records: 877, status: 'success' }
          }
        }
      ]
    };
  },

  computed: {
    allHistory() {
      return this.history.length > 0 ? this.history : this.sampleHistory;
    },

    filteredHistory() {
      let filtered = [...this.allHistory];

      // Apply date filter
      if (this.dateRange !== 'all') {
        const daysAgo = new Date();
        daysAgo.setDate(daysAgo.getDate() - parseInt(this.dateRange));
        filtered = filtered.filter(s => new Date(s.startTime) >= daysAgo);
      }

      // Apply sorting
      filtered.sort((a, b) => {
        let aVal = a[this.sortColumn];
        let bVal = b[this.sortColumn];

        if (this.sortColumn === 'startTime') {
          aVal = new Date(aVal);
          bVal = new Date(bVal);
        }

        if (aVal < bVal) return this.sortDirection === 'asc' ? -1 : 1;
        if (aVal > bVal) return this.sortDirection === 'asc' ? 1 : -1;
        return 0;
      });

      return filtered;
    },

    paginatedHistory() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      return this.filteredHistory.slice(start, start + this.itemsPerPage);
    },

    totalPages() {
      return Math.ceil(this.filteredHistory.length / this.itemsPerPage);
    },

    paginationStart() {
      return (this.currentPage - 1) * this.itemsPerPage + 1;
    },

    paginationEnd() {
      return Math.min(this.currentPage * this.itemsPerPage, this.filteredHistory.length);
    },

    summaryStats() {
      const syncs = this.filteredHistory;
      const successCount = syncs.filter(s => s.status === 'success').length;
      const totalRecords = syncs.reduce((sum, s) => sum + s.recordsProcessed, 0);
      const avgDuration = syncs.length > 0 
        ? Math.round(syncs.reduce((sum, s) => sum + s.duration, 0) / syncs.length)
        : 0;

      return {
        totalSyncs: syncs.length,
        successRate: syncs.length > 0 ? Math.round((successCount / syncs.length) * 100) : 0,
        totalRecords: totalRecords,
        avgDuration: this.formatDuration(avgDuration)
      };
    }
  },

  watch: {
    dateRange() {
      this.currentPage = 1;
    }
  },

  methods: {
    sortBy(column) {
      if (this.sortColumn === column) {
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      } else {
        this.sortColumn = column;
        this.sortDirection = 'desc';
      }
    },

    prevPage() {
      if (this.currentPage > 1) {
        this.currentPage--;
      }
    },

    nextPage() {
      if (this.currentPage < this.totalPages) {
        this.currentPage++;
      }
    },

    getStatusClasses(status) {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const statusStyles = {
        'success': 'bg-green-100 text-green-800',
        'failed': 'bg-red-100 text-red-800',
        'partial': 'bg-yellow-100 text-yellow-800',
        'running': 'bg-blue-100 text-blue-800'
      };
      return `${base} ${statusStyles[status] || 'bg-gray-100 text-gray-800'}`;
    },

    formatDate(dateString) {
      return new Date(dateString).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    formatTime(dateString) {
      return new Date(dateString).toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    formatDateTime(dateString) {
      return new Date(dateString).toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      });
    },

    formatDuration(seconds) {
      if (seconds < 60) return `${seconds}s`;
      const minutes = Math.floor(seconds / 60);
      const remainingSeconds = seconds % 60;
      if (minutes < 60) return `${minutes}m ${remainingSeconds}s`;
      const hours = Math.floor(minutes / 60);
      const remainingMinutes = minutes % 60;
      return `${hours}h ${remainingMinutes}m`;
    },

    formatNumber(num) {
      return num.toLocaleString();
    },

    viewDetails(sync) {
      this.selectedSync = sync;
    },

    closeDetails() {
      this.selectedSync = null;
    },

    retrySyncClick(sync) {
      if (confirm('Are you sure you want to retry this sync?')) {
        this.$emit('retry', sync);
      }
    },

    exportHistory() {
      // Export as CSV
      const headers = ['Date', 'Type', 'Sources', 'Status', 'Records', 'Duration'];
      const rows = this.filteredHistory.map(sync => [
        this.formatDateTime(sync.startTime),
        sync.type,
        sync.sources.join('; '),
        sync.status,
        sync.recordsProcessed,
        this.formatDuration(sync.duration)
      ]);

      const csv = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
      const blob = new Blob([csv], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `etl-history-${new Date().toISOString().split('T')[0]}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
    }
  }
};
</script>
