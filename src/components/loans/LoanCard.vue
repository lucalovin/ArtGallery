<template>
  <!--
    LoanCard.vue - Artwork Loan Display Card Component
    Art Gallery Management System
  -->
  <div 
    class="loan-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg"
    :class="{ 'border-l-4': true, [statusBorderClass]: true }"
  >
    <!-- Header -->
    <div class="px-4 py-3 bg-gray-50 border-b border-gray-100 flex items-center justify-between">
      <div class="flex items-center">
        <span :class="statusBadgeClasses">{{ loanStatus }}</span>
        <span class="ml-2 text-xs text-gray-500">{{ loan.loanType || 'Outgoing' }}</span>
      </div>
      <span class="text-xs text-gray-400">ID: {{ loan.id }}</span>
    </div>

    <!-- Main Content -->
    <div class="p-4">
      <!-- Artwork Info -->
      <div class="flex items-start mb-4">
        <div v-if="loan.artworkImage" class="w-16 h-16 rounded-lg overflow-hidden flex-shrink-0 mr-3">
          <img :src="loan.artworkImage" :alt="loan.artworkTitle" class="w-full h-full object-cover" />
        </div>
        <div v-else class="w-16 h-16 rounded-lg bg-gray-200 flex items-center justify-center flex-shrink-0 mr-3">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="font-semibold text-gray-900 truncate">{{ loan.artworkTitle }}</h3>
          <p class="text-sm text-gray-600">{{ loan.artistName }}</p>
        </div>
      </div>

      <!-- Loan Direction -->
      <div class="flex items-center justify-between mb-4 p-3 bg-gray-50 rounded-lg">
        <div class="text-center">
          <p class="text-xs text-gray-500 mb-1">From</p>
          <p class="font-medium text-sm text-gray-900">{{ loan.lenderName || 'Our Gallery' }}</p>
        </div>
        <div class="flex-shrink-0 px-3">
          <svg class="w-6 h-6 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
          </svg>
        </div>
        <div class="text-center">
          <p class="text-xs text-gray-500 mb-1">To</p>
          <p class="font-medium text-sm text-gray-900">{{ loan.borrowerName || 'External Institution' }}</p>
        </div>
      </div>

      <!-- Dates -->
      <div class="grid grid-cols-2 gap-3 mb-4">
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">Start Date</p>
          <p class="font-medium text-sm">{{ formattedStartDate }}</p>
        </div>
        <div class="text-center p-2 bg-gray-50 rounded-lg">
          <p class="text-xs text-gray-500">End Date</p>
          <p class="font-medium text-sm">{{ formattedEndDate }}</p>
        </div>
      </div>

      <!-- Duration Info -->
      <div v-if="durationInfo" class="text-center mb-4">
        <p class="text-sm" :class="durationClasses">{{ durationInfo }}</p>
      </div>

      <!-- Insurance & Fees -->
      <div class="grid grid-cols-2 gap-3 text-sm mb-4">
        <div v-if="loan.insuranceValue">
          <span class="text-gray-500">Insurance:</span>
          <span class="font-medium ml-1" v-currency="loan.insuranceValue"></span>
        </div>
        <div v-if="loan.loanFee">
          <span class="text-gray-500">Fee:</span>
          <span class="font-medium ml-1" v-currency="loan.loanFee"></span>
        </div>
      </div>

      <!-- Purpose/Notes -->
      <div v-if="loan.purpose" class="text-sm text-gray-600 mb-4 line-clamp-2">
        {{ loan.purpose }}
      </div>
    </div>

    <!-- Actions -->
    <div class="px-4 py-3 bg-gray-50 border-t border-gray-100 flex items-center justify-end space-x-2">
      <button
        type="button"
        @click="$emit('view', loan)"
        class="btn btn-secondary btn-sm"
      >
        Details
      </button>
      <button
        type="button"
        @click="$emit('edit', loan)"
        class="btn btn-secondary btn-sm"
        :disabled="!canEdit"
      >
        Edit
      </button>
      <button
        v-if="canReturn"
        type="button"
        @click="$emit('return', loan)"
        class="btn btn-primary btn-sm"
      >
        Return
      </button>
      <button
        v-if="canExtend"
        type="button"
        @click="$emit('extend', loan)"
        class="btn btn-secondary btn-sm"
      >
        Extend
      </button>
      <button
        type="button"
        @click="confirmDelete"
        class="btn btn-danger btn-sm"
        :disabled="!canDelete"
      >
        Delete
      </button>
    </div>
  </div>
</template>

<script>
/**
 * LoanCard Component
 * Displays artwork loan information
 */
export default {
  name: 'LoanCard',

  props: {
    loan: {
      type: Object,
      required: true
    }
  },

  emits: ['view', 'edit', 'delete', 'return', 'extend'],

  computed: {
    loanStatus() {
      const now = new Date();
      const startDate = new Date(this.loan.startDate);
      const endDate = new Date(this.loan.endDate);

      if (this.loan.returnedDate) {
        return 'Returned';
      }
      if (now < startDate) {
        return 'Pending';
      }
      if (now > endDate) {
        return 'Overdue';
      }
      return 'Active';
    },

    statusBorderClass() {
      const statusClasses = {
        'Pending': 'border-l-yellow-400',
        'Active': 'border-l-green-400',
        'Overdue': 'border-l-red-400',
        'Returned': 'border-l-gray-400'
      };
      return statusClasses[this.loanStatus] || 'border-l-gray-400';
    },

    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const statusStyles = {
        'Pending': 'bg-yellow-100 text-yellow-800',
        'Active': 'bg-green-100 text-green-800',
        'Overdue': 'bg-red-100 text-red-800',
        'Returned': 'bg-gray-100 text-gray-800'
      };
      return `${base} ${statusStyles[this.loanStatus] || 'bg-gray-100 text-gray-800'}`;
    },

    formattedStartDate() {
      if (!this.loan.startDate) return 'N/A';
      return new Date(this.loan.startDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    formattedEndDate() {
      if (!this.loan.endDate) return 'N/A';
      return new Date(this.loan.endDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    durationInfo() {
      const now = new Date();
      const startDate = new Date(this.loan.startDate);
      const endDate = new Date(this.loan.endDate);

      if (this.loan.returnedDate) {
        return 'Loan completed';
      }

      if (now < startDate) {
        const daysUntil = Math.ceil((startDate - now) / (1000 * 60 * 60 * 24));
        return `Starts in ${daysUntil} day${daysUntil !== 1 ? 's' : ''}`;
      }

      if (now > endDate) {
        const daysOverdue = Math.ceil((now - endDate) / (1000 * 60 * 60 * 24));
        return `${daysOverdue} day${daysOverdue !== 1 ? 's' : ''} overdue`;
      }

      const daysRemaining = Math.ceil((endDate - now) / (1000 * 60 * 60 * 24));
      return `${daysRemaining} day${daysRemaining !== 1 ? 's' : ''} remaining`;
    },

    durationClasses() {
      const status = this.loanStatus;
      if (status === 'Overdue') return 'text-red-600 font-medium';
      if (status === 'Active') return 'text-green-600';
      if (status === 'Pending') return 'text-yellow-600';
      return 'text-gray-500';
    },

    canEdit() {
      return this.loanStatus !== 'Returned';
    },

    canDelete() {
      return this.loanStatus === 'Pending' || this.loanStatus === 'Returned';
    },

    canReturn() {
      return this.loanStatus === 'Active' || this.loanStatus === 'Overdue';
    },

    canExtend() {
      return this.loanStatus === 'Active';
    }
  },

  methods: {
    confirmDelete() {
      if (confirm(`Are you sure you want to delete this loan record for "${this.loan.artworkTitle}"?`)) {
        this.$emit('delete', this.loan);
      }
    }
  }
};
</script>
