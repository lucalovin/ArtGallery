<template>
  <!--
    RestorationCard.vue - Artwork Restoration Record Card Component
    Art Gallery Management System
  -->
  <div 
    class="restoration-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg"
    :class="statusBorderClass"
  >
    <!-- Header with Status -->
    <div class="px-4 py-3 bg-gray-50 border-b border-gray-100 flex items-center justify-between">
      <div class="flex items-center">
        <span :class="statusBadgeClasses">{{ restoration.status }}</span>
        <span class="ml-2 text-xs text-gray-500">{{ restoration.type || 'Conservation' }}</span>
      </div>
      <span class="text-xs text-gray-400">ID: {{ restoration.id }}</span>
    </div>

    <!-- Main Content -->
    <div class="p-4">
      <!-- Artwork Info -->
      <div class="flex items-start mb-4">
        <div v-if="restoration.artworkImage" class="w-16 h-16 rounded-lg overflow-hidden flex-shrink-0 mr-3">
          <img :src="restoration.artworkImage" :alt="restoration.artworkTitle" class="w-full h-full object-cover" />
        </div>
        <div v-else class="w-16 h-16 rounded-lg bg-gray-200 flex items-center justify-center flex-shrink-0 mr-3">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="font-semibold text-gray-900 truncate">{{ restoration.artworkTitle }}</h3>
          <p class="text-sm text-gray-600">{{ restoration.artistName }}</p>
        </div>
      </div>

      <!-- Restoration Type -->
      <div class="mb-4">
        <h4 class="text-sm font-medium text-gray-900 mb-1">{{ restoration.treatmentType }}</h4>
        <p class="text-sm text-gray-600 line-clamp-2">{{ restoration.description }}</p>
      </div>

      <!-- Conservator Info -->
      <div class="flex items-center mb-4 p-3 bg-gray-50 rounded-lg">
        <div class="w-10 h-10 rounded-full bg-primary-100 flex items-center justify-center mr-3">
          <span class="text-primary-600 font-medium text-sm">{{ conservatorInitials }}</span>
        </div>
        <div>
          <p class="text-sm font-medium text-gray-900">{{ restoration.conservatorName }}</p>
          <p class="text-xs text-gray-500">Conservator</p>
        </div>
      </div>

      <!-- Dates -->
      <div class="grid grid-cols-2 gap-3 mb-4">
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">Started</p>
          <p class="font-medium text-sm">{{ formattedStartDate }}</p>
        </div>
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">{{ restoration.completedDate ? 'Completed' : 'Est. Completion' }}</p>
          <p class="font-medium text-sm">{{ formattedEndDate }}</p>
        </div>
      </div>

      <!-- Progress Bar -->
      <div v-if="restoration.status === 'In Progress'" class="mb-4">
        <div class="flex justify-between text-xs text-gray-500 mb-1">
          <span>Progress</span>
          <span>{{ restoration.progress || 0 }}%</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2">
          <div 
            class="bg-primary-500 h-2 rounded-full transition-all duration-300"
            :style="{ width: `${restoration.progress || 0}%` }"
          ></div>
        </div>
      </div>

      <!-- Cost -->
      <div v-if="restoration.estimatedCost" class="flex justify-between items-center text-sm mb-4">
        <span class="text-gray-500">Estimated Cost:</span>
        <span class="font-medium" v-currency="restoration.estimatedCost"></span>
      </div>

      <!-- Priority Badge -->
      <div v-if="restoration.priority" class="mb-4">
        <span :class="priorityBadgeClasses">
          {{ restoration.priority }} Priority
        </span>
      </div>
    </div>

    <!-- Actions -->
    <div class="px-4 py-3 bg-gray-50 border-t border-gray-100 flex items-center justify-end space-x-2">
      <button
        type="button"
        @click="$emit('view', restoration)"
        class="btn btn-secondary btn-sm"
      >
        Details
      </button>
      <button
        type="button"
        @click="$emit('edit', restoration)"
        class="btn btn-secondary btn-sm"
      >
        Edit
      </button>
      <button
        v-if="canComplete"
        type="button"
        @click="$emit('complete', restoration)"
        class="btn btn-primary btn-sm"
      >
        Mark Complete
      </button>
      <button
        type="button"
        @click="confirmDelete"
        class="btn btn-danger btn-sm"
      >
        Delete
      </button>
    </div>
  </div>
</template>

<script>
/**
 * RestorationCard Component
 * Displays restoration/conservation work records
 */
export default {
  name: 'RestorationCard',

  props: {
    restoration: {
      type: Object,
      required: true
    }
  },

  emits: ['view', 'edit', 'delete', 'complete'],

  computed: {
    statusBorderClass() {
      const status = (this.restoration.status || '').toLowerCase();
      const statusClasses = {
        'pending': 'border-l-4 border-l-yellow-400',
        'in progress': 'border-l-4 border-l-blue-400',
        'completed': 'border-l-4 border-l-green-400',
        'on hold': 'border-l-4 border-l-orange-400',
        'cancelled': 'border-l-4 border-l-gray-400'
      };
      return statusClasses[status] || '';
    },

    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const status = (this.restoration.status || '').toLowerCase();
      const statusStyles = {
        'pending': 'bg-yellow-100 text-yellow-800',
        'in progress': 'bg-blue-100 text-blue-800',
        'completed': 'bg-green-100 text-green-800',
        'on hold': 'bg-orange-100 text-orange-800',
        'cancelled': 'bg-gray-100 text-gray-800'
      };
      return `${base} ${statusStyles[status] || 'bg-gray-100 text-gray-800'}`;
    },

    priorityBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const priority = (this.restoration.priority || '').toLowerCase();
      const priorityStyles = {
        'high': 'bg-red-100 text-red-800',
        'medium': 'bg-yellow-100 text-yellow-800',
        'low': 'bg-green-100 text-green-800'
      };
      return `${base} ${priorityStyles[priority] || 'bg-gray-100 text-gray-800'}`;
    },

    conservatorInitials() {
      if (!this.restoration.conservatorName) return '?';
      return this.restoration.conservatorName
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
    },

    formattedStartDate() {
      if (!this.restoration.startDate) return 'N/A';
      return new Date(this.restoration.startDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    formattedEndDate() {
      const date = this.restoration.completedDate || this.restoration.estimatedEndDate;
      if (!date) return 'TBD';
      return new Date(date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    canComplete() {
      const status = (this.restoration.status || '').toLowerCase();
      return status === 'in progress';
    }
  },

  methods: {
    confirmDelete() {
      if (confirm(`Are you sure you want to delete this restoration record for "${this.restoration.artworkTitle}"?`)) {
        this.$emit('delete', this.restoration);
      }
    }
  }
};
</script>
