<template>
  <!--
    AddEditLoan.vue - Add/Edit Loan Form
    Art Gallery Management System
  -->
  <div class="add-edit-loan-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to Loans
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Loan Agreement' : 'New Loan Agreement' }}
      </h1>
    </header>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Artwork Selection -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Artwork Details</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Artwork <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.artworkId"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select Artwork</option>
              <option v-for="artwork in availableArtworks" :key="artwork.id" :value="artwork.id">
                {{ artwork.title }} - {{ artwork.artist }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              v-model="form.status"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="Pending">Pending Approval</option>
              <option value="Active">Active</option>
              <option value="Returned">Returned</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Borrower Details -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Borrower Information</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Institution Name <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.borrowerName"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Contact Person</label>
            <input
              v-model="form.contactPerson"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Contact Email</label>
            <input
              v-model="form.contactEmail"
              type="email"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Contact Phone</label>
            <input
              v-model="form.contactPhone"
              type="tel"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <!-- Loan Period -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Loan Period</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
            <input
              v-model="form.startDate"
              type="date"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
            <input
              v-model="form.endDate"
              type="date"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <!-- Financial Details -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Financial & Insurance</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Loan Fee (USD)</label>
            <input
              v-model.number="form.loanFee"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Insured Value (USD)</label>
            <input
              v-model.number="form.insuredValue"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Insurance Provider</label>
            <input
              v-model="form.insuranceProvider"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Policy Number</label>
            <input
              v-model="form.policyNumber"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <!-- Notes -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Additional Notes</h2>
        <textarea
          v-model="form.notes"
          rows="4"
          class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          placeholder="Special conditions, handling instructions, etc."
        ></textarea>
      </div>

      <div class="flex justify-end space-x-4">
        <button
          type="button"
          @click="goBack"
          class="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 font-medium"
        >
          Cancel
        </button>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium disabled:opacity-50"
        >
          {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
/**
 * AddEditLoan Page - OPTIONS API
 */
export default {
  name: 'AddEditLoanPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      isSubmitting: false,
      availableArtworks: [
        { id: 1, title: 'Starry Night', artist: 'Vincent van Gogh' },
        { id: 2, title: 'Water Lilies', artist: 'Claude Monet' },
        { id: 3, title: 'The Thinker', artist: 'Auguste Rodin' },
        { id: 4, title: 'Girl with Pearl Earring', artist: 'Johannes Vermeer' }
      ],
      form: {
        artworkId: '',
        status: 'Pending',
        borrowerName: '',
        contactPerson: '',
        contactEmail: '',
        contactPhone: '',
        startDate: '',
        endDate: '',
        loanFee: null,
        insuredValue: null,
        insuranceProvider: '',
        policyNumber: '',
        notes: ''
      }
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    }
  },

  created() {
    if (this.isEditMode) {
      this.loadLoan();
    }
  },

  methods: {
    async loadLoan() {
      this.form = {
        artworkId: 2,
        status: 'Active',
        borrowerName: 'Metropolitan Museum',
        contactPerson: 'Dr. Jane Wilson',
        contactEmail: 'jane.wilson@metmuseum.org',
        contactPhone: '+1 212-555-0123',
        startDate: '2024-01-15',
        endDate: '2024-06-15',
        loanFee: 50000,
        insuredValue: 2500000,
        insuranceProvider: 'ArtGuard',
        policyNumber: 'AG-2024-00456',
        notes: 'Handle with care. Climate controlled transport required.'
      };
    },

    async handleSubmit() {
      this.isSubmitting = true;
      await new Promise(resolve => setTimeout(resolve, 500));
      this.$router.push('/loans');
    },

    goBack() {
      this.$router.push('/loans');
    }
  }
};
</script>

<style scoped>
.add-edit-loan-page {
  padding: 1.5rem;
  max-width: 900px;
  margin: 0 auto;
}
</style>
