<template>
  <!--
    VisitorCard.vue - Visitor Display Card Component
    Art Gallery Management System
  -->
  <div 
    class="visitor-card bg-white rounded-xl shadow-sm border border-gray-100 p-4 transition-all duration-300 hover:shadow-lg hover:border-gray-200"
  >
    <!-- Header -->
    <div class="flex items-start justify-between mb-4">
      <div class="flex items-center space-x-3">
        <!-- Avatar -->
        <div :class="avatarClasses">
          {{ initials }}
        </div>
        <div>
          <h3 class="text-lg font-semibold text-gray-900">{{ fullName }}</h3>
          <p class="text-sm text-gray-500">{{ visitor.email }}</p>
        </div>
      </div>
      
      <!-- Member Badge -->
      <span v-if="visitor.isMember" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
        <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
          <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/>
        </svg>
        Member
      </span>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-3 mb-4">
      <div class="bg-gray-50 rounded-lg p-2 text-center">
        <p class="text-lg font-bold text-gray-900">{{ visitor.visitCount || 0 }}</p>
        <p class="text-xs text-gray-500">Visits</p>
      </div>
      <div class="bg-gray-50 rounded-lg p-2 text-center">
        <p class="text-lg font-bold text-gray-900">{{ visitor.reviewCount || 0 }}</p>
        <p class="text-xs text-gray-500">Reviews</p>
      </div>
      <div class="bg-gray-50 rounded-lg p-2 text-center">
        <p class="text-lg font-bold text-primary-600" v-currency="totalSpent"></p>
        <p class="text-xs text-gray-500">Spent</p>
      </div>
    </div>

    <!-- Last Visit -->
    <div v-if="visitor.lastVisit" class="flex items-center text-sm text-gray-500 mb-4">
      <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
      Last visit: {{ formattedLastVisit }}
    </div>

    <!-- Contact Info -->
    <div v-if="visitor.phone" class="flex items-center text-sm text-gray-500 mb-4">
      <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
      </svg>
      {{ visitor.phone }}
    </div>

    <!-- Actions -->
    <div class="flex items-center justify-end space-x-2 pt-3 border-t border-gray-100">
      <button
        type="button"
        @click="$emit('view', visitor)"
        class="btn btn-secondary btn-sm"
      >
        View
      </button>
      <button
        type="button"
        @click="$emit('edit', visitor)"
        class="btn btn-secondary btn-sm"
      >
        Edit
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
 * VisitorCard Component
 */
export default {
  name: 'VisitorCard',

  props: {
    visitor: {
      type: Object,
      required: true
    }
  },

  emits: ['view', 'edit', 'delete'],

  computed: {
    fullName() {
      return `${this.visitor.firstName || ''} ${this.visitor.lastName || ''}`.trim() || 'Unknown';
    },

    initials() {
      const first = (this.visitor.firstName || '')[0] || '';
      const last = (this.visitor.lastName || '')[0] || '';
      return `${first}${last}`.toUpperCase() || '?';
    },

    avatarClasses() {
      const colors = [
        'bg-primary-500',
        'bg-secondary-500',
        'bg-green-500',
        'bg-purple-500',
        'bg-pink-500',
        'bg-indigo-500'
      ];
      const index = (this.visitor.id || 0) % colors.length;
      return `w-12 h-12 rounded-full ${colors[index]} text-white flex items-center justify-center font-semibold text-lg`;
    },

    totalSpent() {
      return this.visitor.totalSpent || 0;
    },

    formattedLastVisit() {
      if (!this.visitor.lastVisit) return '';
      return new Date(this.visitor.lastVisit).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    }
  },

  methods: {
    confirmDelete() {
      if (confirm(`Are you sure you want to delete ${this.fullName}?`)) {
        this.$emit('delete', this.visitor);
      }
    }
  }
};
</script>
