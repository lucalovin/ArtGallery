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
        <p class="text-sm text-gray-500">Total Value</p>
        <p class="text-2xl font-bold text-green-600">{{ formatCurrency(stats.totalValue) }}</p>
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
              <p class="text-sm text-gray-500">{{ loan.artist }}</p>
              <div class="flex items-center mt-2 space-x-4 text-sm text-gray-600">
                <span>📍 {{ loan.borrowerName }}</span>
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

        <!-- Insurance & Value Info -->
        <div class="mt-4 pt-4 border-t border-gray-100 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
          <div>
            <p class="text-gray-500">Insured Value</p>
            <p class="font-medium text-gray-900">{{ formatCurrency(loan.insuredValue) }}</p>
          </div>
          <div>
            <p class="text-gray-500">Insurance Provider</p>
            <p class="font-medium text-gray-900">{{ loan.insuranceProvider }}</p>
          </div>
          <div>
            <p class="text-gray-500">Loan Fee</p>
            <p class="font-medium text-gray-900">{{ formatCurrency(loan.loanFee) }}</p>
          </div>
          <div>
            <p class="text-gray-500">Days Remaining</p>
            <p class="font-medium" :class="loan.daysRemaining < 30 ? 'text-orange-600' : 'text-gray-900'">
              {{ loan.daysRemaining }} days
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
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
      ],
      stats: { active: 12, pending: 3, returningSoon: 4, totalValue: 8500000 },
      loans: []
    };
  },

  computed: {
    filteredLoans() {
      if (this.activeStatus === 'all') return this.loans;
      return this.loans.filter(l => l.status === this.activeStatus);
    }
  },

  created() {
    this.loadLoans();
  },

  methods: {
    async loadLoans() {
      this.isLoading = true;
      await new Promise(resolve => setTimeout(resolve, 500));
      this.loans = [
        { id: 1, artworkTitle: 'Water Lilies', artist: 'Claude Monet', borrowerName: 'Metropolitan Museum', startDate: '2024-01-15', endDate: '2024-06-15', status: 'Active', insuredValue: 2500000, insuranceProvider: 'ArtGuard', loanFee: 50000, daysRemaining: 145 },
        { id: 2, artworkTitle: 'The Thinker', artist: 'Auguste Rodin', borrowerName: 'Louvre Museum', startDate: '2024-02-01', endDate: '2024-08-01', status: 'Active', insuredValue: 3000000, insuranceProvider: 'MasterArt Insurance', loanFee: 75000, daysRemaining: 192 },
        { id: 3, artworkTitle: 'Girl with Pearl Earring', artist: 'Johannes Vermeer', borrowerName: 'National Gallery', startDate: '2024-03-01', endDate: '2024-05-01', status: 'Pending', insuredValue: 1500000, insuranceProvider: 'Heritage Protect', loanFee: 35000, daysRemaining: 100 },
        { id: 4, artworkTitle: 'Starry Night', artist: 'Vincent van Gogh', borrowerName: 'MoMA', startDate: '2023-06-01', endDate: '2023-12-01', status: 'Returned', insuredValue: 2000000, insuranceProvider: 'ArtGuard', loanFee: 45000, daysRemaining: 0 }
      ];
      this.isLoading = false;
    },

    editLoan(loan) {
      this.$router.push({ name: 'EditLoan', params: { id: loan.id } });
    },

    deleteLoan(loan) {
      this.loans = this.loans.filter(l => l.id !== loan.id);
    },

    formatCurrency(value) {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
    },

    formatDateRange(start, end) {
      const opts = { month: 'short', year: 'numeric' };
      return `${new Date(start).toLocaleDateString('en-US', opts)} - ${new Date(end).toLocaleDateString('en-US', opts)}`;
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
