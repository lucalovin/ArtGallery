<template>
  <!--
    AddEditLoan.vue - Add/Edit Loan Form
    Art Gallery Management System
    Form respects Loan table schema with ArtworkId and ExhibitorId FKs
  -->
  <div class="add-edit-loan-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2"></span>
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
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Artwork & Exhibitor</h2>
        
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
              <option value="" disabled>Select Artwork</option>
              <option v-for="artwork in artworks" :key="artwork.id" :value="artwork.id">
                {{ artwork.title }} {{ artwork.artistName ? `(${artwork.artistName})` : '' }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Exhibitor <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.exhibitorId"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="" disabled>Select Exhibitor</option>
              <option v-for="exhibitor in exhibitors" :key="exhibitor.id" :value="exhibitor.id">
                {{ exhibitor.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- Loan Period -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Loan Period</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Start Date <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.startDate"
              type="date"
              required
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

      <!-- Conditions -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Conditions</h2>
        <textarea
          v-model="form.conditions"
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
import { mapActions } from 'vuex';

/**
 * AddEditLoan Page - OPTIONS API
 * Form respects Loan table schema: ArtworkId (FK), ExhibitorId (FK), StartDate, EndDate, Conditions
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
      artworks: [],
      exhibitors: [],
      form: {
        artworkId: '',
        exhibitorId: '',
        startDate: '',
        endDate: '',
        conditions: ''
      }
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    }
  },

  async created() {
    await this.loadLookups();
    if (this.isEditMode) {
      await this.loadLoan();
    }
  },

  methods: {
    ...mapActions({
      createLoan: 'loans/createLoan',
      updateLoan: 'loans/updateLoan',
      fetchLoanById: 'loans/fetchLoanById'
    }),

    async loadLookups() {
      try {
        const [artworksResponse, exhibitorsResponse] = await Promise.all([
          this.$api.lookups.getArtworks(),
          this.$api.lookups.getExhibitors()
        ]);
        
        // Extract data from API response wrapper { success: true, data: [...] }
        this.artworks = artworksResponse.data?.data || artworksResponse.data || [];
        this.exhibitors = exhibitorsResponse.data?.data || exhibitorsResponse.data || [];
      } catch (error) {
        console.error('Failed to load lookups:', error);
      }
    },

    async loadLoan() {
      try {
        const loan = await this.fetchLoanById(this.id);
        this.form = {
          artworkId: loan.artworkId || '',
          exhibitorId: loan.exhibitorId || '',
          startDate: loan.startDate ? loan.startDate.split('T')[0] : '',
          endDate: loan.endDate ? loan.endDate.split('T')[0] : '',
          conditions: loan.conditions || ''
        };
      } catch (error) {
        console.error('Error loading loan:', error);
        this.$router.push('/loans');
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        // Build DTO matching backend CreateLoanDto/UpdateLoanDto
        // Ensure dates are in ISO format and empty strings become null
        const submitData = {
          artworkId: parseInt(this.form.artworkId),
          exhibitorId: parseInt(this.form.exhibitorId),
          startDate: this.form.startDate ? new Date(this.form.startDate).toISOString() : null,
          endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null,
          conditions: this.form.conditions || null
        };

        if (this.isEditMode) {
          await this.updateLoan({ id: parseInt(this.id), ...submitData });
        } else {
          await this.createLoan(submitData);
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
