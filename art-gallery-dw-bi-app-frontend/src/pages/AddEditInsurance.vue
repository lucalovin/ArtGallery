<template>
  <!--
    AddEditInsurance.vue - Add/Edit Insurance Record Form
    Art Gallery Management System
    Links artworks to insurance policies
    Matches backend CreateInsuranceDto: ArtworkId, PolicyId, InsuredAmount
  -->
  <div class="add-edit-insurance-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to Insurance
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Insurance Record' : 'New Insurance Record' }}
      </h1>
      <p class="text-gray-500 mt-1">Link an artwork to an insurance policy</p>
    </header>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Artwork Selection -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Artwork</h2>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Select Artwork <span class="text-red-500">*</span>
          </label>
          <select
            v-model="form.artworkId"
            required
            class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          >
            <option value="" disabled>Select an artwork to insure</option>
            <option v-for="artwork in artworks" :key="artwork.id" :value="artwork.id">
              {{ artwork.name || artwork.title }} {{ artwork.description ? `- ${artwork.description}` : '' }}
            </option>
          </select>
        </div>
      </div>

      <!-- Policy Selection -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Insurance Policy</h2>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Select Policy <span class="text-red-500">*</span>
          </label>
          <select
            v-model="form.policyId"
            required
            class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          >
            <option value="" disabled>Select an insurance policy</option>
            <option v-for="policy in policies" :key="policy.id" :value="policy.id">
              {{ policy.name }} {{ policy.description ? `- ${policy.description}` : '' }}
            </option>
          </select>
        </div>
      </div>

      <!-- Insured Amount -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Coverage Details</h2>
        
        <div class="max-w-md">
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Insured Amount ($) <span class="text-red-500">*</span>
          </label>
          <input
            v-model.number="form.insuredAmount"
            type="number"
            min="0"
            step="1000"
            required
            placeholder="e.g., 100000"
            class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          />
          <p class="text-sm text-gray-500 mt-1">The insured value for this specific artwork under the selected policy</p>
        </div>
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
 * AddEditInsurance Page - OPTIONS API
 * Links artworks to insurance policies
 * Matches backend CreateInsuranceDto: ArtworkId, PolicyId, InsuredAmount
 */
export default {
  name: 'AddEditInsurancePage',

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
      policies: [],
      form: {
        artworkId: '',
        policyId: '',
        insuredAmount: null
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
      await this.loadInsurance();
    }
  },

  methods: {
    async loadLookups() {
      try {
        const [artworksResponse, policiesResponse] = await Promise.all([
          this.$api.lookups.getArtworks(),
          this.$api.lookups.getPolicies()
        ]);
        
        // Extract data from API response wrapper
        this.artworks = artworksResponse.data?.data || artworksResponse.data || [];
        this.policies = policiesResponse.data?.data || policiesResponse.data || [];
      } catch (error) {
        console.error('Failed to load lookups:', error);
      }
    },

    async loadInsurance() {
      try {
        const response = await this.$api.insurance.getById(this.id);
        if (response.data?.success && response.data?.data) {
          const insurance = response.data.data;
          this.form = {
            artworkId: insurance.artworkId || '',
            policyId: insurance.policyId || '',
            insuredAmount: insurance.insuredAmount || null
          };
        }
      } catch (error) {
        console.error('Error loading insurance:', error);
        this.$router.push('/insurance');
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        // Build DTO matching backend CreateInsuranceDto/UpdateInsuranceDto
        const submitData = {
          artworkId: parseInt(this.form.artworkId),
          policyId: parseInt(this.form.policyId),
          insuredAmount: this.form.insuredAmount
        };

        if (this.isEditMode) {
          await this.$api.insurance.update(parseInt(this.id), submitData);
        } else {
          await this.$api.insurance.create(submitData);
        }
        this.$router.push('/insurance');
      } catch (error) {
        console.error('Error saving insurance:', error);
        alert('Failed to save insurance record. Please check all required fields.');
      } finally {
        this.isSubmitting = false;
      }
    },

    goBack() {
      this.$router.push('/insurance');
    }
  }
};
</script>

<style scoped>
.add-edit-insurance-page {
  padding: 1.5rem;
  max-width: 900px;
  margin: 0 auto;
}
</style>
