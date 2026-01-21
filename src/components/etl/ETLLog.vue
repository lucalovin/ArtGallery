<template>
  <!--
    ETLLog.vue - ETL Activity Log Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-log bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
      <h2 class="text-lg font-semibold text-gray-900">Activity Log</h2>
      <div class="flex items-center space-x-2">
        <button
          @click="$emit('refresh')"
          :disabled="isLoading"
          class="p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
          title="Refresh logs"
        >
          <svg 
            class="w-5 h-5"
            :class="{ 'animate-spin': isLoading }"
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        </button>
        <button
          @click="$emit('view-all')"
          class="text-sm text-primary-600 hover:text-primary-700 font-medium"
        >
          View All
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="px-6 py-3 border-b border-gray-100 bg-gray-50">
      <div class="flex flex-wrap gap-2">
        <button
          v-for="level in logLevels"
          :key="level.value"
          @click="toggleFilter(level.value)"
          :class="[
            'px-3 py-1 rounded-full text-xs font-medium transition-colors',
            activeFilters.includes(level.value)
              ? level.activeClass
              : 'bg-gray-200 text-gray-600 hover:bg-gray-300'
          ]"
        >
          {{ level.label }}
        </button>
      </div>
    </div>

    <!-- Log Entries -->
    <div class="divide-y divide-gray-100 max-h-80 overflow-y-auto">
      <template v-if="filteredLogs.length > 0">
        <div
          v-for="log in filteredLogs"
          :key="log.id"
          class="px-6 py-3 hover:bg-gray-50 transition-colors"
        >
          <div class="flex items-start">
            <!-- Level Icon -->
            <span :class="getLogIconClasses(log.level)" class="flex-shrink-0 mt-0.5">
              <svg v-if="log.level === 'error'" class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
              </svg>
              <svg v-else-if="log.level === 'warning'" class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
              </svg>
              <svg v-else-if="log.level === 'success'" class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
              </svg>
              <svg v-else class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/>
              </svg>
            </span>

            <!-- Log Content -->
            <div class="ml-3 flex-1 min-w-0">
              <p class="text-sm text-gray-900">{{ log.message }}</p>
              <div class="flex items-center mt-1 text-xs text-gray-500">
                <span>{{ formatTimestamp(log.timestamp) }}</span>
                <span class="mx-2">•</span>
                <span class="px-1.5 py-0.5 bg-gray-100 rounded text-gray-600">{{ log.source }}</span>
              </div>
            </div>

            <!-- Expand Button (if details exist) -->
            <button
              v-if="log.details"
              @click="toggleLogDetails(log.id)"
              class="ml-2 text-gray-400 hover:text-gray-600"
            >
              <svg 
                class="w-4 h-4 transition-transform"
                :class="{ 'rotate-180': expandedLogs.includes(log.id) }"
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>
          </div>

          <!-- Expanded Details -->
          <transition name="expand">
            <div 
              v-if="log.details && expandedLogs.includes(log.id)"
              class="mt-2 ml-7 p-3 bg-gray-50 rounded-lg text-xs font-mono text-gray-600 overflow-x-auto"
            >
              <pre>{{ formatDetails(log.details) }}</pre>
            </div>
          </transition>
        </div>
      </template>

      <!-- Empty State -->
      <div v-else class="px-6 py-12 text-center">
        <svg class="w-12 h-12 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <p class="mt-4 text-gray-500">No log entries found</p>
        <p class="text-sm text-gray-400">Try adjusting your filters</p>
      </div>
    </div>

    <!-- Footer Stats -->
    <div class="px-6 py-3 border-t border-gray-100 bg-gray-50 flex items-center justify-between text-xs text-gray-500">
      <span>Showing {{ filteredLogs.length }} of {{ logs.length }} entries</span>
      <div class="flex items-center space-x-4">
        <span class="flex items-center">
          <span class="w-2 h-2 rounded-full bg-red-500 mr-1"></span>
          {{ errorCount }} errors
        </span>
        <span class="flex items-center">
          <span class="w-2 h-2 rounded-full bg-yellow-500 mr-1"></span>
          {{ warningCount }} warnings
        </span>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ETLLog Component - OPTIONS API
 * Displays and filters ETL operation logs
 */
export default {
  name: 'ETLLog',

  props: {
    logs: {
      type: Array,
      default: () => []
    },
    isLoading: {
      type: Boolean,
      default: false
    }
  },

  emits: ['refresh', 'view-all'],

  data() {
    return {
      activeFilters: ['info', 'warning', 'error', 'success'],
      expandedLogs: [],
      logLevels: [
        { value: 'info', label: 'Info', activeClass: 'bg-blue-100 text-blue-800' },
        { value: 'success', label: 'Success', activeClass: 'bg-green-100 text-green-800' },
        { value: 'warning', label: 'Warning', activeClass: 'bg-yellow-100 text-yellow-800' },
        { value: 'error', label: 'Error', activeClass: 'bg-red-100 text-red-800' }
      ]
    };
  },

  computed: {
    filteredLogs() {
      if (this.activeFilters.length === 0) return this.logs;
      return this.logs.filter(log => this.activeFilters.includes(log.level));
    },

    errorCount() {
      return this.logs.filter(log => log.level === 'error').length;
    },

    warningCount() {
      return this.logs.filter(log => log.level === 'warning').length;
    }
  },

  methods: {
    toggleFilter(level) {
      const index = this.activeFilters.indexOf(level);
      if (index > -1) {
        this.activeFilters.splice(index, 1);
      } else {
        this.activeFilters.push(level);
      }
    },

    toggleLogDetails(logId) {
      const index = this.expandedLogs.indexOf(logId);
      if (index > -1) {
        this.expandedLogs.splice(index, 1);
      } else {
        this.expandedLogs.push(logId);
      }
    },

    getLogIconClasses(level) {
      const classes = {
        'info': 'text-blue-500',
        'success': 'text-green-500',
        'warning': 'text-yellow-500',
        'error': 'text-red-500'
      };
      return classes[level] || 'text-gray-500';
    },

    formatTimestamp(timestamp) {
      const date = new Date(timestamp);
      const now = new Date();
      const diff = now - date;

      // Less than 1 minute
      if (diff < 60000) {
        return 'Just now';
      }

      // Less than 1 hour
      if (diff < 3600000) {
        const minutes = Math.floor(diff / 60000);
        return `${minutes}m ago`;
      }

      // Less than 24 hours
      if (diff < 86400000) {
        const hours = Math.floor(diff / 3600000);
        return `${hours}h ago`;
      }

      // More than 24 hours - show date
      return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    formatDetails(details) {
      if (typeof details === 'string') return details;
      return JSON.stringify(details, null, 2);
    }
  }
};
</script>

<style scoped>
.expand-enter-active,
.expand-leave-active {
  transition: all 0.2s ease;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
}

.expand-enter-to,
.expand-leave-from {
  opacity: 1;
  max-height: 200px;
}
</style>
