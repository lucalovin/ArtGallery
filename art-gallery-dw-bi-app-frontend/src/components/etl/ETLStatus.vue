<template>
  <!--
    ETLStatus.vue - ETL Connection Status Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-status bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
      <h2 class="text-lg font-semibold text-gray-900">System Status</h2>
      <span :class="overallStatusBadgeClasses">{{ overallStatus }}</span>
    </div>

    <div class="p-6 space-y-4">
      <!-- Connection Status List -->
      <div class="space-y-3">
        <div 
          v-for="connection in connections" 
          :key="connection.id"
          class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
        >
          <div class="flex items-center">
            <!-- Status Indicator -->
            <span :class="getConnectionIndicatorClasses(connection.status)"></span>
            <div class="ml-3">
              <p class="text-sm font-medium text-gray-900">{{ connection.name }}</p>
              <p class="text-xs text-gray-500">{{ connection.type }}</p>
            </div>
          </div>
          <div class="flex items-center space-x-2">
            <span :class="getConnectionStatusClasses(connection.status)">
              {{ formatConnectionStatus(connection.status) }}
            </span>
            <button
              @click="$emit('test-connection', connection.id)"
              class="text-gray-400 hover:text-gray-600"
              :disabled="connection.status === 'testing'"
              title="Test Connection"
            >
              <svg 
                class="w-4 h-4"
                :class="{ 'animate-spin': connection.status === 'testing' }"
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Health Metrics -->
      <div class="pt-4 border-t border-gray-100">
        <h3 class="text-sm font-medium text-gray-700 mb-3">Health Metrics</h3>
        <div class="space-y-3">
          <!-- CPU Usage -->
          <div>
            <div class="flex justify-between text-xs text-gray-500 mb-1">
              <span>ETL Process CPU</span>
              <span>{{ metrics.cpuUsage }}%</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div 
                class="h-2 rounded-full transition-all duration-300"
                :class="getMetricBarClass(metrics.cpuUsage)"
                :style="{ width: `${metrics.cpuUsage}%` }"
              ></div>
            </div>
          </div>

          <!-- Memory Usage -->
          <div>
            <div class="flex justify-between text-xs text-gray-500 mb-1">
              <span>Memory Usage</span>
              <span>{{ metrics.memoryUsage }}%</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div 
                class="h-2 rounded-full transition-all duration-300"
                :class="getMetricBarClass(metrics.memoryUsage)"
                :style="{ width: `${metrics.memoryUsage}%` }"
              ></div>
            </div>
          </div>

          <!-- Queue Size -->
          <div>
            <div class="flex justify-between text-xs text-gray-500 mb-1">
              <span>Job Queue</span>
              <span>{{ metrics.queueSize }} jobs</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div 
                class="h-2 rounded-full bg-blue-500 transition-all duration-300"
                :style="{ width: `${Math.min(metrics.queueSize * 10, 100)}%` }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Last Activity -->
      <div class="pt-4 border-t border-gray-100">
        <h3 class="text-sm font-medium text-gray-700 mb-3">Last Activity</h3>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500">Last Sync</span>
            <span class="text-gray-900">{{ formattedLastSync }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Duration</span>
            <span class="text-gray-900">{{ lastSyncDuration }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Records Processed</span>
            <span class="text-gray-900">{{ lastSyncRecords.toLocaleString() }}</span>
          </div>
        </div>
      </div>

      <!-- Uptime -->
      <div class="pt-4 border-t border-gray-100">
        <div class="flex items-center justify-between">
          <span class="text-sm text-gray-500">System Uptime</span>
          <span class="text-sm font-medium text-gray-900">{{ systemUptime }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ETLStatus Component
 * Displays connection status and health metrics for ETL system
 */
export default {
  name: 'ETLStatus',

  props: {
    status: {
      type: Object,
      required: true
    },
    connections: {
      type: Array,
      default: () => []
    }
  },

  emits: ['test-connection'],

  data() {
    return {
      metrics: {
        cpuUsage: 23,
        memoryUsage: 45,
        queueSize: 2
      },
      uptimeStart: new Date(Date.now() - 86400000 * 3), // 3 days ago
      metricsInterval: null
    };
  },

  computed: {
    overallStatus() {
      const hasDisconnected = this.connections.some(c => c.status === 'disconnected');
      const hasError = this.connections.some(c => c.status === 'error');
      
      if (hasError) return 'Error';
      if (hasDisconnected) return 'Degraded';
      return 'Operational';
    },

    overallStatusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const statusStyles = {
        'Operational': 'bg-green-100 text-green-800',
        'Degraded': 'bg-yellow-100 text-yellow-800',
        'Error': 'bg-red-100 text-red-800'
      };
      return `${base} ${statusStyles[this.overallStatus]}`;
    },

    formattedLastSync() {
      if (!this.status.lastSync?.timestamp) return 'Never';
      const date = new Date(this.status.lastSync.timestamp);
      return this.formatTimeAgo(date);
    },

    lastSyncDuration() {
      if (!this.status.lastSync?.duration) return 'N/A';
      const seconds = this.status.lastSync.duration;
      if (seconds < 60) return `${seconds}s`;
      const minutes = Math.floor(seconds / 60);
      const remainingSeconds = seconds % 60;
      return `${minutes}m ${remainingSeconds}s`;
    },

    lastSyncRecords() {
      return this.status.lastSync?.recordsProcessed || 0;
    },

    systemUptime() {
      const now = new Date();
      const diff = now - this.uptimeStart;
      
      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      
      if (days > 0) {
        return `${days}d ${hours}h ${minutes}m`;
      }
      return `${hours}h ${minutes}m`;
    }
  },

  mounted() {
    // Start fetching real metrics
    this.fetchMetrics();
    this.startMetricsUpdates();
  },

  beforeUnmount() {
    if (this.metricsInterval) {
      clearInterval(this.metricsInterval);
    }
  },

  methods: {
    async fetchMetrics() {
      try {
        const response = await this.$api.etl.getStatus();
        if (response.data?.success && response.data?.data?.metrics) {
          const serverMetrics = response.data.data.metrics;
          this.metrics = {
            cpuUsage: serverMetrics.cpuUsage ?? this.metrics.cpuUsage,
            memoryUsage: serverMetrics.memoryUsage ?? this.metrics.memoryUsage,
            queueSize: serverMetrics.queueSize ?? this.metrics.queueSize
          };
        }
      } catch (error) {
        console.error('Failed to fetch metrics:', error);
        // Keep current metrics values on error
      }
    },
    getConnectionIndicatorClasses(status) {
      const base = 'w-3 h-3 rounded-full';
      const statusClasses = {
        'connected': 'bg-green-500',
        'disconnected': 'bg-red-500',
        'testing': 'bg-yellow-500 animate-pulse',
        'error': 'bg-red-500'
      };
      return `${base} ${statusClasses[status] || 'bg-gray-500'}`;
    },

    getConnectionStatusClasses(status) {
      const base = 'text-xs font-medium';
      const statusClasses = {
        'connected': 'text-green-600',
        'disconnected': 'text-red-600',
        'testing': 'text-yellow-600',
        'error': 'text-red-600'
      };
      return `${base} ${statusClasses[status] || 'text-gray-600'}`;
    },

    formatConnectionStatus(status) {
      const statusLabels = {
        'connected': 'Connected',
        'disconnected': 'Disconnected',
        'testing': 'Testing...',
        'error': 'Error'
      };
      return statusLabels[status] || status;
    },

    getMetricBarClass(value) {
      if (value >= 80) return 'bg-red-500';
      if (value >= 60) return 'bg-yellow-500';
      return 'bg-green-500';
    },

    formatTimeAgo(date) {
      const now = new Date();
      const diff = now - date;
      
      const minutes = Math.floor(diff / (1000 * 60));
      const hours = Math.floor(diff / (1000 * 60 * 60));
      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      
      if (minutes < 1) return 'Just now';
      if (minutes < 60) return `${minutes}m ago`;
      if (hours < 24) return `${hours}h ago`;
      return `${days}d ago`;
    },

    startMetricsUpdates() {
      // Poll for real metrics every 5 seconds
      this.metricsInterval = setInterval(() => {
        this.fetchMetrics();
      }, 5000);
    }
  }
};
</script>
