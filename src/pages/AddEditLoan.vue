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
import { mapState, mapActions } from 'vuex';

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
      form: {
        artworkId: '',
        artworkTitle: '',
        artist: '',
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
        notes: '',
        daysRemaining: 0
      }
    };
  },

  computed: {
    ...mapState({
      artworks: state => state.artwork?.artworks || [],
      loans: state => state.loans?.loans || []
    }),

    availableArtworks() {
      return this.artworks.map(a => ({ id: a.id, title: a.title, artist: a.artist }));
    },

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
    ...mapActions({
      createLoan: 'loans/createLoan',
      updateLoan: 'loans/updateLoan',
      fetchLoans: 'loans/fetchLoans'
    }),

    async loadLoan() {
      const loan = this.loans.find(l => l.id === parseInt(this.id));
      if (loan) {
        this.form = { ...loan };
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        // Get artwork details
        const artwork = this.artworks.find(a => a.id === parseInt(this.form.artworkId));
        if (artwork) {
          this.form.artworkTitle = artwork.title;
          this.form.artist = artwork.artist;
        }
        
        // Calculate days remaining
        if (this.form.endDate) {
          const end = new Date(this.form.endDate);
          const now = new Date();
          this.form.daysRemaining = Math.max(0, Math.ceil((end - now) / (1000 * 60 * 60 * 24)));
        }

        if (this.isEditMode) {
          await this.updateLoan({ id: parseInt(this.id), ...this.form });
        } else {
          await this.createLoan(this.form);
        }
        this.$router.push('/loans');
      } catch (error) {
        console.error('Error saving loan:', error);
        alert('Failed to save loan. Please try again.');
      } finally {
        this.isSubmitting = false;
      }
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
