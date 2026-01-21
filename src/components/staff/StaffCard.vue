<template>
  <!--
    StaffCard.vue - Staff Member Display Card Component
    Art Gallery Management System
  -->
  <div 
    class="staff-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg hover:border-gray-200"
  >
    <!-- Header with Image -->
    <div class="relative h-32 bg-gradient-to-br from-primary-400 to-secondary-500">
      <div class="absolute -bottom-12 left-4">
        <div v-if="staff.imageUrl" class="w-24 h-24 rounded-xl overflow-hidden border-4 border-white shadow-md">
          <img
            :src="staff.imageUrl"
            :alt="fullName"
            class="w-full h-full object-cover"
            @error="handleImageError"
          />
        </div>
        <div v-else :class="avatarClasses">
          {{ initials }}
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="pt-14 px-4 pb-4">
      <!-- Name and Position -->
      <div class="mb-3">
        <h3 class="text-lg font-semibold text-gray-900">{{ fullName }}</h3>
        <p class="text-sm text-primary-600 font-medium">{{ staff.position }}</p>
        <p class="text-xs text-gray-500">{{ staff.department }}</p>
      </div>

      <!-- Status Badge -->
      <div class="mb-4">
        <span :class="statusBadgeClasses">
          {{ staff.status || 'Active' }}
        </span>
      </div>

      <!-- Contact Info -->
      <div class="space-y-2 text-sm text-gray-600 mb-4">
        <div v-if="staff.email" class="flex items-center">
          <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
          <span class="truncate">{{ staff.email }}</span>
        </div>
        <div v-if="staff.phone" class="flex items-center">
          <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
          </svg>
          <span>{{ staff.phone }}</span>
        </div>
        <div v-if="staff.hireDate" class="flex items-center">
          <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span>Joined {{ formattedHireDate }}</span>
        </div>
      </div>

      <!-- Skills Tags -->
      <div v-if="staff.skills && staff.skills.length > 0" class="flex flex-wrap gap-1 mb-4">
        <span 
          v-for="skill in displayedSkills" 
          :key="skill"
          class="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full"
        >
          {{ skill }}
        </span>
        <span v-if="remainingSkillsCount > 0" class="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full">
          +{{ remainingSkillsCount }} more
        </span>
      </div>

      <!-- Actions -->
      <div class="flex items-center justify-end space-x-2 pt-3 border-t border-gray-100">
        <button
          type="button"
          @click="$emit('view', staff)"
          class="btn btn-secondary btn-sm"
        >
          View
        </button>
        <button
          type="button"
          @click="$emit('edit', staff)"
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
  </div>
</template>

<script>
/**
 * StaffCard Component
 */
export default {
  name: 'StaffCard',

  props: {
    staff: {
      type: Object,
      required: true
    }
  },

  emits: ['view', 'edit', 'delete'],

  computed: {
    fullName() {
      return `${this.staff.firstName || ''} ${this.staff.lastName || ''}`.trim() || 'Unknown';
    },

    initials() {
      const first = (this.staff.firstName || '')[0] || '';
      const last = (this.staff.lastName || '')[0] || '';
      return `${first}${last}`.toUpperCase() || '?';
    },

    avatarClasses() {
      return 'w-24 h-24 rounded-xl bg-primary-600 text-white flex items-center justify-center font-bold text-2xl border-4 border-white shadow-md';
    },

    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const status = (this.staff.status || 'active').toLowerCase();

      const statusStyles = {
        'active': 'bg-green-100 text-green-800',
        'on leave': 'bg-yellow-100 text-yellow-800',
        'terminated': 'bg-red-100 text-red-800',
        'retired': 'bg-gray-100 text-gray-800'
      };

      return `${base} ${statusStyles[status] || statusStyles['active']}`;
    },

    formattedHireDate() {
      if (!this.staff.hireDate) return '';
      return new Date(this.staff.hireDate).toLocaleDateString('en-US', {
        month: 'short',
        year: 'numeric'
      });
    },

    displayedSkills() {
      if (!this.staff.skills) return [];
      return this.staff.skills.slice(0, 3);
    },

    remainingSkillsCount() {
      if (!this.staff.skills) return 0;
      return Math.max(0, this.staff.skills.length - 3);
    }
  },

  methods: {
    confirmDelete() {
      if (confirm(`Are you sure you want to delete ${this.fullName}?`)) {
        this.$emit('delete', this.staff);
      }
    },

    handleImageError(event) {
      event.target.style.display = 'none';
    }
  }
};
</script>
