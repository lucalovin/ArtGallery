<template>
  <!--
    LoanManagement.vue - Loan Management Page
    Art Gallery Management System
  -->
  <div class="loan-management-page">
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Loan Management</h1>
        <p class="text-gray-500 mt-1">Track artwork loans and agreements</p>
      </div>
      <router-link
        to="/loans/new"
        class="mt-4 md:mt-0 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">➕</span>
        New Loan
      </router-link>
    </header>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Active Loans</p>
        <p class="text-2xl font-bold text-primary-600">{{ stats.active }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Pending Approval</p>
        <p class="text-2xl font-bold text-yellow-600">{{ stats.pending }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Returning Soon</p>
        <p class="text-2xl font-bold text-orange-600">{{ stats.returningSoon }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Loans</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.total }}</p>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="mb-6">
      <nav class="flex space-x-2 bg-gray-100 rounded-lg p-1">
        <button
          v-for="tab in statusTabs"
          :key="tab.value"
          @click="activeStatus = tab.value"
          class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
          :class="activeStatus === tab.value 
            ? 'bg-white text-primary-600 shadow-sm' 
            : 'text-gray-600 hover:text-gray-800'"
        >
          {{ tab.label }}
        </button>
      </nav>
    </div>

    <!-- Loans List -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="n in 4" :key="n" class="bg-white rounded-xl p-6 animate-pulse">
        <div class="h-4 bg-gray-200 rounded w-1/3"></div>
      </div>
    </div>

    <div v-else class="space-y-4">
      <div 
        v-for="loan in filteredLoans" 
        :key="loan.id"
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-6"
      >
        <div class="flex flex-col md:flex-row md:items-center md:justify-between">
          <div class="flex items-start space-x-4">
            <div class="w-16 h-16 bg-gray-100 rounded-lg flex items-center justify-center text-2xl">
              🖼️
            </div>
            <div>
              <h3 class="font-semibold text-gray-900">{{ loan.artworkTitle }}</h3>
              <p class="text-sm text-gray-500">{{ loan.artistName || 'Unknown Artist' }}</p>
              <div class="flex items-center mt-2 space-x-4 text-sm text-gray-600">
                <span>📍 {{ loan.exhibitorName || 'Unknown Exhibitor' }}</span>
                <span>📅 {{ formatDateRange(loan.startDate, loan.endDate) }}</span>
              </div>
            </div>
          </div>

          <div class="mt-4 md:mt-0 flex items-center space-x-4">
            <span 
              class="px-3 py-1 text-sm font-medium rounded-full"
              :class="getStatusClass(loan.status)"
            >
              {{ loan.status }}
            </span>
            <div class="flex space-x-2">
              <button @click="editLoan(loan)" class="p-2 hover:bg-gray-100 rounded-lg">✏️</button>
              <button @click="deleteLoan(loan)" class="p-2 hover:bg-red-50 rounded-lg">🗑️</button>
            </div>
          </div>
        </div>

        <!-- Loan Details -->
        <div class="mt-4 pt-4 border-t border-gray-100 grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
          <div>
            <p class="text-gray-500">Days Remaining</p>
            <p class="font-medium" :class="loan.daysRemaining !== null && loan.daysRemaining < 30 ? 'text-orange-600' : 'text-gray-900'">
              {{ loan.daysRemaining !== null ? loan.daysRemaining + ' days' : 'No end date' }}
            </p>
          </div>
          <div>
            <p class="text-gray-500">Start Date</p>
            <p class="font-medium text-gray-900">{{ formatDate(loan.startDate) }}</p>
          </div>
          <div>
            <p class="text-gray-500">End Date</p>
            <p class="font-medium text-gray-900">{{ loan.endDate ? formatDate(loan.endDate) : 'Ongoing' }}</p>
          </div>
        </div>

        <!-- Conditions -->
        <div v-if="loan.conditions" class="mt-3 pt-3 border-t border-gray-100">
          <p class="text-sm text-gray-500">Conditions</p>
          <p class="text-sm text-gray-700">{{ loan.conditions }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';

/**
 * LoanManagement Page
 */
export default {
  name: 'LoanManagementPage',

  data() {
    return {
      isLoading: true,
      activeStatus: 'all',
      statusTabs: [
        { label: 'All Loans', value: 'all' },
        { label: 'Active', value: 'Active' },
        { label: 'Pending', value: 'Pending' },
        { label: 'Returned', value: 'Returned' }
      ]
    };
  },

  computed: {
    ...mapState({
      loans: state => state.loans?.loans || []
    }),

    stats() {
      return {
        active: this.loans.filter(l => l.status === 'Active').length,
        pending: this.loans.filter(l => l.status === 'Pending').length,
        returningSoon: this.loans.filter(l => l.status === 'Active' && l.daysRemaining !== null && l.daysRemaining < 30).length,
        total: this.loans.length
      };
    },

    filteredLoans() {
      if (this.activeStatus === 'all') return this.loans;
      return this.loans.filter(l => l.status === this.activeStatus);
    }
  },

  created() {
    this.loadLoans();
  },

  methods: {
    ...mapActions({
      fetchLoans: 'loans/fetchLoans',
      deleteLoanAction: 'loans/deleteLoan'
    }),

    async loadLoans() {
      this.isLoading = true;
      try {
        await this.fetchLoans();
      } catch (error) {
        console.error('Error loading loans:', error);
      } finally {
        this.isLoading = false;
      }
    },

    editLoan(loan) {
      this.$router.push({ name: 'EditLoan', params: { id: loan.id } });
    },

    async deleteLoan(loan) {
      if (confirm(`Are you sure you want to delete this loan for "${loan.artworkTitle}"?`)) {
        try {
          await this.deleteLoanAction(loan.id);
        } catch (error) {
          console.error('Error deleting loan:', error);
        }
      }
    },

    formatCurrency(value) {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
    },

    formatDate(dateStr) {
      if (!dateStr) return 'N/A';
      return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    },

    formatDateRange(start, end) {
      const opts = { month: 'short', year: 'numeric' };
      const startStr = start ? new Date(start).toLocaleDateString('en-US', opts) : 'N/A';
      const endStr = end ? new Date(end).toLocaleDateString('en-US', opts) : 'Ongoing';
      return `${startStr} - ${endStr}`;
    },

    getStatusClass(status) {
      const classes = {
        Active: 'bg-green-100 text-green-700',
        Pending: 'bg-yellow-100 text-yellow-700',
        Returned: 'bg-gray-100 text-gray-600',
        Overdue: 'bg-red-100 text-red-700'
      };
      return classes[status] || classes.Pending;
    }
  }
};
</script>

<style scoped>
.loan-management-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
