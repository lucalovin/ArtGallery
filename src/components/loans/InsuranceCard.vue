<template>
  <!--
    InsuranceCard.vue - Artwork Insurance Record Card Component
    Art Gallery Management System
  -->
  <div 
    class="insurance-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg"
    :class="statusBorderClass"
  >
    <!-- Header -->
    <div class="px-4 py-3 bg-gray-50 border-b border-gray-100 flex items-center justify-between">
      <div class="flex items-center">
        <span :class="statusBadgeClasses">{{ policyStatus }}</span>
      </div>
      <span class="text-xs text-gray-400">Policy #{{ insurance.policyNumber }}</span>
    </div>

    <!-- Main Content -->
    <div class="p-4">
      <!-- Artwork Info -->
      <div class="flex items-start mb-4">
        <div v-if="insurance.artworkImage" class="w-16 h-16 rounded-lg overflow-hidden flex-shrink-0 mr-3">
          <img :src="insurance.artworkImage" :alt="insurance.artworkTitle" class="w-full h-full object-cover" />
        </div>
        <div v-else class="w-16 h-16 rounded-lg bg-gray-200 flex items-center justify-center flex-shrink-0 mr-3">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="font-semibold text-gray-900 truncate">{{ insurance.artworkTitle }}</h3>
          <p class="text-sm text-gray-600">{{ insurance.artistName }}</p>
        </div>
      </div>

      <!-- Insurance Provider -->
      <div class="mb-4 p-3 bg-gray-50 rounded-lg">
        <div class="flex items-center">
          <svg class="w-5 h-5 text-primary-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          <div>
            <p class="text-sm font-medium text-gray-900">{{ insurance.providerName }}</p>
            <p class="text-xs text-gray-500">Insurance Provider</p>
          </div>
        </div>
      </div>

      <!-- Coverage Details -->
      <div class="grid grid-cols-2 gap-3 mb-4">
        <div class="p-3 bg-primary-50 rounded-lg text-center">
          <p class="text-xs text-primary-600 mb-1">Coverage Value</p>
          <p class="font-bold text-primary-700" v-currency="insurance.coverageAmount"></p>
        </div>
        <div class="p-3 bg-gray-50 rounded-lg text-center">
          <p class="text-xs text-gray-500 mb-1">Annual Premium</p>
          <p class="font-bold text-gray-700" v-currency="insurance.premiumAmount"></p>
        </div>
      </div>

      <!-- Policy Period -->
      <div class="grid grid-cols-2 gap-3 mb-4">
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">Start Date</p>
          <p class="font-medium text-sm">{{ formattedStartDate }}</p>
        </div>
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">Expiry Date</p>
          <p class="font-medium text-sm" :class="{ 'text-red-600': isExpiringSoon }">{{ formattedExpiryDate }}</p>
        </div>
      </div>

      <!-- Expiry Warning -->
      <div v-if="expiryWarning" class="mb-4 p-3 rounded-lg" :class="expiryWarningClasses">
        <div class="flex items-center">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <span class="text-sm font-medium">{{ expiryWarning }}</span>
        </div>
      </div>

      <!-- Coverage Type -->
      <div v-if="insurance.coverageType" class="mb-4">
        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
          {{ insurance.coverageType }}
        </span>
      </div>

      <!-- Deductible -->
      <div v-if="insurance.deductible" class="flex justify-between items-center text-sm mb-4">
        <span class="text-gray-500">Deductible:</span>
        <span class="font-medium" v-currency="insurance.deductible"></span>
      </div>
    </div>

    <!-- Actions -->
    <div class="px-4 py-3 bg-gray-50 border-t border-gray-100 flex items-center justify-end space-x-2">
      <button
        type="button"
        @click="$emit('view', insurance)"
        class="btn btn-secondary btn-sm"
      >
        Details
      </button>
      <button
        type="button"
        @click="$emit('edit', insurance)"
        class="btn btn-secondary btn-sm"
      >
        Edit
      </button>
      <button
        v-if="canRenew"
        type="button"
        @click="$emit('renew', insurance)"
        class="btn btn-primary btn-sm"
      >
        Renew
      </button>
      <button
        type="button"
        @click="$emit('claim', insurance)"
        class="btn btn-secondary btn-sm"
      >
        File Claim
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
 * InsuranceCard Component
 * Displays insurance policy records for artworks
 */
export default {
  name: 'InsuranceCard',

  props: {
    insurance: {
      type: Object,
      required: true
    }
  },

  emits: ['view', 'edit', 'delete', 'renew', 'claim'],

  computed: {
    policyStatus() {
      const now = new Date();
      const expiryDate = new Date(this.insurance.expiryDate);
      const startDate = new Date(this.insurance.startDate);

      if (now < startDate) {
        return 'Pending';
      }
      if (now > expiryDate) {
        return 'Expired';
      }
      return 'Active';
    },

    statusBorderClass() {
      const statusClasses = {
        'Pending': 'border-l-4 border-l-yellow-400',
        'Active': 'border-l-4 border-l-green-400',
        'Expired': 'border-l-4 border-l-red-400'
      };
      return statusClasses[this.policyStatus] || '';
    },

    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const statusStyles = {
        'Pending': 'bg-yellow-100 text-yellow-800',
        'Active': 'bg-green-100 text-green-800',
        'Expired': 'bg-red-100 text-red-800'
      };
      return `${base} ${statusStyles[this.policyStatus] || 'bg-gray-100 text-gray-800'}`;
    },

    formattedStartDate() {
      if (!this.insurance.startDate) return 'N/A';
      return new Date(this.insurance.startDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    formattedExpiryDate() {
      if (!this.insurance.expiryDate) return 'N/A';
      return new Date(this.insurance.expiryDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    daysUntilExpiry() {
      if (!this.insurance.expiryDate) return null;
      const now = new Date();
      const expiry = new Date(this.insurance.expiryDate);
      return Math.ceil((expiry - now) / (1000 * 60 * 60 * 24));
    },

    isExpiringSoon() {
      return this.daysUntilExpiry !== null && this.daysUntilExpiry <= 30 && this.daysUntilExpiry > 0;
    },

    expiryWarning() {
      if (this.policyStatus === 'Expired') {
        return 'Policy has expired. Renewal required.';
      }
      if (this.daysUntilExpiry !== null && this.daysUntilExpiry <= 30 && this.daysUntilExpiry > 0) {
        return `Policy expires in ${this.daysUntilExpiry} days`;
      }
      if (this.daysUntilExpiry !== null && this.daysUntilExpiry <= 7 && this.daysUntilExpiry > 0) {
        return `Urgent: Policy expires in ${this.daysUntilExpiry} days!`;
      }
      return null;
    },

    expiryWarningClasses() {
      if (this.policyStatus === 'Expired') {
        return 'bg-red-100 text-red-700';
      }
      if (this.daysUntilExpiry !== null && this.daysUntilExpiry <= 7) {
        return 'bg-red-100 text-red-700';
      }
      return 'bg-yellow-100 text-yellow-700';
    },

    canRenew() {
      return this.policyStatus === 'Expired' || this.isExpiringSoon;
    }
  },

  methods: {
    confirmDelete() {
      if (confirm(`Are you sure you want to delete this insurance policy for "${this.insurance.artworkTitle}"?`)) {
        this.$emit('delete', this.insurance);
      }
    }
  }
};
</script>
