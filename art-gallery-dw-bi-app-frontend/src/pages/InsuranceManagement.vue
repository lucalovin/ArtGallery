<template>
  <!--
    InsuranceManagement.vue - Insurance Records Management Page
    Art Gallery Management System
    Manages insurance records that link artworks to insurance policies
  -->
  <div class="insurance-management-page">
    <header class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Insurance Records</h1>
        <p class="text-gray-500 mt-1">Manage insurance coverage for artworks</p>
      </div>
      <router-link
        to="/insurance/new"
        class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">+</span>
        Add Insurance
      </router-link>
    </header>

    <!-- Statistics Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Records</p>
        <p class="text-2xl font-bold text-gray-900">{{ stats.totalInsurances }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Policies</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.totalPolicies }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Insured Artworks</p>
        <p class="text-2xl font-bold text-blue-600">{{ insurances.length }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Insured Value</p>
        <p class="text-2xl font-bold text-amber-600">${{ formatNumber(stats.totalInsuredAmount) }}</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-wrap gap-4">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search by artwork or provider..."
          class="flex-1 min-w-64 px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
        />
        <button
          @click="loadInsurances"
          class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200"
        >
          Refresh
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
    </div>

    <!-- Insurance Records Table -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Artwork</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Policy Provider</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Insured Amount</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="insurance in filteredInsurances" :key="insurance.id" class="hover:bg-gray-50">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
              {{ insurance.artworkTitle || `Artwork #${insurance.artworkId}` }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
              {{ insurance.policyProvider || `Policy #${insurance.policyId}` }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
              ${{ formatNumber(insurance.insuredAmount) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
              <router-link
                :to="`/insurance/${insurance.id}/edit`"
                class="text-primary-600 hover:text-primary-900 mr-4"
              >
                Edit
              </router-link>
              <button
                @click="confirmDelete(insurance)"
                class="text-red-600 hover:text-red-900"
              >
                Delete
              </button>
            </td>
          </tr>
          <tr v-if="filteredInsurances.length === 0">
            <td colspan="4" class="px-6 py-12 text-center text-gray-500">
              No insurance records found.
              <router-link to="/insurance/new" class="text-primary-600 hover:text-primary-700 ml-1">
                Add one now
              </router-link>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 max-w-md w-full mx-4">
        <h3 class="text-lg font-semibold text-gray-900 mb-2">Delete Insurance Record</h3>
        <p class="text-gray-600 mb-6">
          Are you sure you want to delete the insurance for 
          <strong>{{ insuranceToDelete?.artworkTitle || `Artwork #${insuranceToDelete?.artworkId}` }}</strong>?
          This action cannot be undone.
        </p>
        <div class="flex justify-end space-x-3">
          <button
            @click="showDeleteModal = false"
            class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
          >
            Cancel
          </button>
          <button
            @click="deleteInsurance"
            class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * InsuranceManagement Page - OPTIONS API
 * Manages insurance records linking artworks to policies
 */
export default {
  name: 'InsuranceManagementPage',

  data() {
    return {
      isLoading: true,
      insurances: [],
      searchQuery: '',
      showDeleteModal: false,
      insuranceToDelete: null,
      stats: {
        totalInsurances: 0,
        totalPolicies: 0,
        totalInsuredAmount: 0
      }
    };
  },

  computed: {
    filteredInsurances() {
      if (!this.searchQuery) {
        return this.insurances;
      }

      const query = this.searchQuery.toLowerCase();
      return this.insurances.filter(i =>
        i.artworkTitle?.toLowerCase().includes(query) ||
        i.policyProvider?.toLowerCase().includes(query)
      );
    }
  },

  async created() {
    await this.loadInsurances();
    await this.loadStatistics();
  },

  methods: {
    async loadInsurances() {
      this.isLoading = true;
      try {
        const response = await this.$api.insurance.getAll();
        // API returns { success: true, data: { items: [...], totalCount: N } }
        if (response.data?.success && response.data?.data) {
          const data = response.data.data;
          this.insurances = Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
        } else if (Array.isArray(response.data)) {
          this.insurances = response.data;
        } else {
          this.insurances = [];
        }
      } catch (error) {
        console.error('Failed to load insurances:', error);
        this.insurances = [];
      } finally {
        this.isLoading = false;
      }
    },

    async loadStatistics() {
      try {
        const response = await this.$api.insurance.getStatistics();
        if (response.data?.success && response.data?.data) {
          const data = response.data.data;
          this.stats = {
            totalInsurances: data.totalInsurances || this.insurances.length,
            totalPolicies: data.totalPolicies || 0,
            totalInsuredAmount: data.totalInsuredAmount || 0
          };
        }
      } catch (error) {
        console.error('Failed to load statistics:', error);
        // Calculate from loaded insurances
        this.stats = {
          totalInsurances: this.insurances.length,
          totalPolicies: [...new Set(this.insurances.map(i => i.policyId))].length,
          totalInsuredAmount: this.insurances.reduce((sum, i) => sum + (i.insuredAmount || 0), 0)
        };
      }
    },

    confirmDelete(insurance) {
      this.insuranceToDelete = insurance;
      this.showDeleteModal = true;
    },

    async deleteInsurance() {
      if (!this.insuranceToDelete) return;
      
      try {
        await this.$api.insurance.delete(this.insuranceToDelete.id);
        this.insurances = this.insurances.filter(i => i.id !== this.insuranceToDelete.id);
        await this.loadStatistics();
      } catch (error) {
        console.error('Failed to delete insurance:', error);
        alert('Failed to delete insurance record. Please try again.');
      } finally {
        this.showDeleteModal = false;
        this.insuranceToDelete = null;
      }
    },

    formatNumber(num) {
      return (num || 0).toLocaleString();
    }
  }
};
</script>

<style scoped>
.insurance-management-page {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}
</style>
