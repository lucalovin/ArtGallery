<template>
  <!--
    LoanForm.vue - Artwork Loan Create/Edit Form Component
    Art Gallery Management System
  -->
  <div class="loan-form bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Form Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
      <h2 class="text-xl font-semibold text-gray-900">
        {{ isEditMode ? 'Edit Loan Agreement' : 'Create New Loan Agreement' }}
      </h2>
      <p class="text-sm text-gray-500 mt-1">
        {{ isEditMode ? 'Update loan details' : 'Fill in the details for the artwork loan' }}
      </p>
    </div>

    <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
      <!-- Loan Type Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Loan Type
        </h3>

        <div class="flex space-x-6">
          <label class="flex items-center">
            <input
              type="radio"
              v-model="form.loanType"
              value="Outgoing"
              class="form-radio text-primary-600"
            />
            <span class="ml-2 text-sm">Outgoing (We lend to others)</span>
          </label>
          <label class="flex items-center">
            <input
              type="radio"
              v-model="form.loanType"
              value="Incoming"
              class="form-radio text-primary-600"
            />
            <span class="ml-2 text-sm">Incoming (We borrow from others)</span>
          </label>
        </div>
      </div>

      <!-- Artwork Selection -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Artwork Selection
        </h3>

        <div v-if="form.loanType === 'Outgoing'">
          <label for="artworkId" class="block text-sm font-medium text-gray-700 mb-1">
            Select Artwork <span class="text-red-500">*</span>
          </label>
          <select
            id="artworkId"
            v-model="form.artworkId"
            class="form-input"
            :class="{ 'border-red-500': errors.artworkId }"
          >
            <option value="">Select an artwork to loan</option>
            <option v-for="artwork in availableArtworks" :key="artwork.id" :value="artwork.id">
              {{ artwork.title }} - {{ artwork.artist }}
            </option>
          </select>
          <p v-if="errors.artworkId" class="text-red-500 text-xs mt-1">{{ errors.artworkId }}</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="artworkTitle" class="block text-sm font-medium text-gray-700 mb-1">
              Artwork Title <span class="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="artworkTitle"
              v-model.trim="form.artworkTitle"
              class="form-input"
              :class="{ 'border-red-500': errors.artworkTitle }"
              placeholder="Title of the artwork"
            />
            <p v-if="errors.artworkTitle" class="text-red-500 text-xs mt-1">{{ errors.artworkTitle }}</p>
          </div>
          <div>
            <label for="artistName" class="block text-sm font-medium text-gray-700 mb-1">
              Artist Name <span class="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="artistName"
              v-model.trim="form.artistName"
              class="form-input"
              :class="{ 'border-red-500': errors.artistName }"
              placeholder="Artist name"
            />
            <p v-if="errors.artistName" class="text-red-500 text-xs mt-1">{{ errors.artistName }}</p>
          </div>
        </div>
      </div>

      <!-- Institution Details -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          {{ form.loanType === 'Outgoing' ? 'Borrower Details' : 'Lender Details' }}
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="institutionName" class="block text-sm font-medium text-gray-700 mb-1">
              Institution Name <span class="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="institutionName"
              v-model.trim="form.institutionName"
              class="form-input"
              :class="{ 'border-red-500': errors.institutionName }"
              placeholder="e.g., Metropolitan Museum of Art"
            />
            <p v-if="errors.institutionName" class="text-red-500 text-xs mt-1">{{ errors.institutionName }}</p>
          </div>

          <div>
            <label for="contactPerson" class="block text-sm font-medium text-gray-700 mb-1">
              Contact Person
            </label>
            <input
              type="text"
              id="contactPerson"
              v-model.trim="form.contactPerson"
              class="form-input"
              placeholder="Contact name"
            />
          </div>

          <div>
            <label for="contactEmail" class="block text-sm font-medium text-gray-700 mb-1">
              Contact Email
            </label>
            <input
              type="email"
              id="contactEmail"
              v-model.trim="form.contactEmail"
              class="form-input"
              placeholder="email@institution.org"
            />
          </div>

          <div>
            <label for="contactPhone" class="block text-sm font-medium text-gray-700 mb-1">
              Contact Phone
            </label>
            <input
              type="tel"
              id="contactPhone"
              v-model.trim="form.contactPhone"
              class="form-input"
              placeholder="+1 (555) 000-0000"
            />
          </div>
        </div>
      </div>

      <!-- Loan Period -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Loan Period
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="startDate" class="block text-sm font-medium text-gray-700 mb-1">
              Start Date <span class="text-red-500">*</span>
            </label>
            <input
              type="date"
              id="startDate"
              v-model="form.startDate"
              class="form-input"
              :class="{ 'border-red-500': errors.startDate }"
            />
            <p v-if="errors.startDate" class="text-red-500 text-xs mt-1">{{ errors.startDate }}</p>
          </div>

          <div>
            <label for="endDate" class="block text-sm font-medium text-gray-700 mb-1">
              End Date <span class="text-red-500">*</span>
            </label>
            <input
              type="date"
              id="endDate"
              v-model="form.endDate"
              class="form-input"
              :class="{ 'border-red-500': errors.endDate }"
              :min="form.startDate"
            />
            <p v-if="errors.endDate" class="text-red-500 text-xs mt-1">{{ errors.endDate }}</p>
          </div>
        </div>

        <div v-if="loanDuration" class="p-3 bg-blue-50 rounded-lg">
          <p class="text-sm text-blue-700">
            <strong>Loan Duration:</strong> {{ loanDuration }}
          </p>
        </div>
      </div>

      <!-- Financial Details -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Financial Details
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label for="insuranceValue" class="block text-sm font-medium text-gray-700 mb-1">
              Insurance Value <span class="text-red-500">*</span>
            </label>
            <div class="relative">
              <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
              <input
                type="number"
                id="insuranceValue"
                v-model.number="form.insuranceValue"
                class="form-input pl-8"
                :class="{ 'border-red-500': errors.insuranceValue }"
                placeholder="100000"
                min="0"
              />
            </div>
            <p v-if="errors.insuranceValue" class="text-red-500 text-xs mt-1">{{ errors.insuranceValue }}</p>
          </div>

          <div>
            <label for="loanFee" class="block text-sm font-medium text-gray-700 mb-1">
              Loan Fee
            </label>
            <div class="relative">
              <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
              <input
                type="number"
                id="loanFee"
                v-model.number="form.loanFee"
                class="form-input pl-8"
                placeholder="5000"
                min="0"
              />
            </div>
          </div>

          <div>
            <label for="shippingCost" class="block text-sm font-medium text-gray-700 mb-1">
              Shipping Cost
            </label>
            <div class="relative">
              <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
              <input
                type="number"
                id="shippingCost"
                v-model.number="form.shippingCost"
                class="form-input pl-8"
                placeholder="2000"
                min="0"
              />
            </div>
          </div>
        </div>

        <div v-if="totalCost > 0" class="p-3 bg-gray-50 rounded-lg">
          <p class="text-sm text-gray-700">
            <strong>Total Cost:</strong> <span v-currency="totalCost"></span>
          </p>
        </div>
      </div>

      <!-- Purpose -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Purpose & Conditions
        </h3>

        <div>
          <label for="purpose" class="block text-sm font-medium text-gray-700 mb-1">
            Purpose of Loan <span class="text-red-500">*</span>
          </label>
          <select
            id="purpose"
            v-model="form.purpose"
            class="form-input"
            :class="{ 'border-red-500': errors.purpose }"
          >
            <option value="">Select purpose</option>
            <option v-for="purpose in loanPurposes" :key="purpose" :value="purpose">
              {{ purpose }}
            </option>
          </select>
          <p v-if="errors.purpose" class="text-red-500 text-xs mt-1">{{ errors.purpose }}</p>
        </div>

        <div>
          <label for="exhibitionName" class="block text-sm font-medium text-gray-700 mb-1">
            Exhibition Name (if applicable)
          </label>
          <input
            type="text"
            id="exhibitionName"
            v-model.trim="form.exhibitionName"
            class="form-input"
            placeholder="Name of the exhibition"
          />
        </div>

        <div>
          <label for="conditions" class="block text-sm font-medium text-gray-700 mb-1">
            Special Conditions
          </label>
          <textarea
            id="conditions"
            v-model.trim="form.conditions"
            class="form-input"
            rows="3"
            placeholder="Any special handling, display, or environmental requirements..."
          ></textarea>
        </div>

        <div>
          <label for="notes" class="block text-sm font-medium text-gray-700 mb-1">
            Additional Notes
          </label>
          <textarea
            id="notes"
            v-model.trim="form.notes"
            class="form-input"
            rows="2"
            placeholder="Any additional notes..."
          ></textarea>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-3 pt-4 border-t border-gray-200">
        <button
          type="button"
          @click="handleCancel"
          class="btn btn-secondary"
        >
          Cancel
        </button>
        <button
          type="button"
          @click="resetForm"
          class="btn btn-secondary"
        >
          Reset
        </button>
        <button
          type="submit"
          :disabled="!isFormValid || isSubmitting"
          class="btn btn-primary"
          :class="{ 'opacity-50 cursor-not-allowed': !isFormValid || isSubmitting }"
        >
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>{{ isEditMode ? 'Update Loan' : 'Create Loan' }}</span>
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex';

/**
 * LoanForm Component
 * Create and edit artwork loan agreements
 */
export default {
  name: 'LoanForm',

  props: {
    loan: {
      type: Object,
      default: null
    },
    artworksList: {
      type: Array,
      default: () => []
    }
  },

  emits: ['submit', 'cancel'],

  data() {
    return {
      form: {
        loanType: 'Outgoing',
        artworkId: '',
        artworkTitle: '',
        artistName: '',
        institutionName: '',
        contactPerson: '',
        contactEmail: '',
        contactPhone: '',
        startDate: '',
        endDate: '',
        insuranceValue: null,
        loanFee: null,
        shippingCost: null,
        purpose: '',
        exhibitionName: '',
        conditions: '',
        notes: ''
      },
      errors: {},
      isSubmitting: false,
      loanPurposes: [
        'Exhibition',
        'Research',
        'Conservation Study',
        'Educational Program',
        'Traveling Exhibition',
        'Museum Exchange',
        'Special Event',
        'Long-term Display',
        'Other'
      ]
    };
  },

  computed: {
    isEditMode() {
      return this.loan !== null && this.loan.id !== undefined;
    },

    availableArtworks() {
      // Filter to show only artworks that are available for loan
      return this.artworksList.filter(a => 
        a.status === 'Available' || a.status === 'On Display'
      );
    },

    loanDuration() {
      if (!this.form.startDate || !this.form.endDate) return '';
      
      const start = new Date(this.form.startDate);
      const end = new Date(this.form.endDate);
      const diffTime = end - start;
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      if (diffDays < 0) return '';
      
      const months = Math.floor(diffDays / 30);
      const days = diffDays % 30;
      
      if (months > 0 && days > 0) {
        return `${months} month${months !== 1 ? 's' : ''} and ${days} day${days !== 1 ? 's' : ''}`;
      } else if (months > 0) {
        return `${months} month${months !== 1 ? 's' : ''}`;
      } else {
        return `${diffDays} day${diffDays !== 1 ? 's' : ''}`;
      }
    },

    totalCost() {
      return (this.form.loanFee || 0) + (this.form.shippingCost || 0);
    },

    isFormValid() {
      const hasArtwork = this.form.loanType === 'Outgoing' 
        ? !!this.form.artworkId
        : (!!this.form.artworkTitle && !!this.form.artistName);

      return (
        hasArtwork &&
        this.form.institutionName &&
        this.form.startDate &&
        this.form.endDate &&
        new Date(this.form.endDate) > new Date(this.form.startDate) &&
        this.form.insuranceValue > 0 &&
        this.form.purpose &&
        Object.keys(this.errors).length === 0
      );
    }
  },

  watch: {
    loan: {
      handler(newLoan) {
        if (newLoan) {
          this.populateForm(newLoan);
        } else {
          this.resetForm();
        }
      },
      immediate: true,
      deep: true
    },

    'form.artworkId'(value) {
      if (this.form.loanType === 'Outgoing' && !value) {
        this.errors.artworkId = 'Please select an artwork';
      } else {
        delete this.errors.artworkId;
      }
    },

    'form.artworkTitle'(value) {
      if (this.form.loanType === 'Incoming' && !value) {
        this.errors.artworkTitle = 'Artwork title is required';
      } else {
        delete this.errors.artworkTitle;
      }
    },

    'form.institutionName'(value) {
      if (!value) {
        this.errors.institutionName = 'Institution name is required';
      } else {
        delete this.errors.institutionName;
      }
    },

    'form.startDate'(value) {
      if (!value) {
        this.errors.startDate = 'Start date is required';
      } else {
        delete this.errors.startDate;
      }
      this.validateEndDate();
    },

    'form.endDate'(value) {
      this.validateEndDate();
    },

    'form.insuranceValue'(value) {
      if (!value || value <= 0) {
        this.errors.insuranceValue = 'Insurance value is required';
      } else {
        delete this.errors.insuranceValue;
      }
    },

    'form.purpose'(value) {
      if (!value) {
        this.errors.purpose = 'Purpose is required';
      } else {
        delete this.errors.purpose;
      }
    }
  },

  methods: {
    ...mapActions('loans', ['createLoan', 'updateLoan']),

    populateForm(loan) {
      this.form = {
        loanType: loan.loanType || 'Outgoing',
        artworkId: loan.artworkId || '',
        artworkTitle: loan.artworkTitle || '',
        artistName: loan.artistName || '',
        institutionName: loan.loanType === 'Outgoing' 
          ? (loan.borrowerName || '') 
          : (loan.lenderName || ''),
        contactPerson: loan.contactPerson || '',
        contactEmail: loan.contactEmail || '',
        contactPhone: loan.contactPhone || '',
        startDate: loan.startDate || '',
        endDate: loan.endDate || '',
        insuranceValue: loan.insuranceValue || null,
        loanFee: loan.loanFee || null,
        shippingCost: loan.shippingCost || null,
        purpose: loan.purpose || '',
        exhibitionName: loan.exhibitionName || '',
        conditions: loan.conditions || '',
        notes: loan.notes || ''
      };
      this.errors = {};
    },

    resetForm() {
      this.form = {
        loanType: 'Outgoing',
        artworkId: '',
        artworkTitle: '',
        artistName: '',
        institutionName: '',
        contactPerson: '',
        contactEmail: '',
        contactPhone: '',
        startDate: '',
        endDate: '',
        insuranceValue: null,
        loanFee: null,
        shippingCost: null,
        purpose: '',
        exhibitionName: '',
        conditions: '',
        notes: ''
      };
      this.errors = {};

      if (this.loan) {
        this.populateForm(this.loan);
      }
    },

    validateEndDate() {
      if (!this.form.endDate) {
        this.errors.endDate = 'End date is required';
      } else if (this.form.startDate && new Date(this.form.endDate) <= new Date(this.form.startDate)) {
        this.errors.endDate = 'End date must be after start date';
      } else {
        delete this.errors.endDate;
      }
    },

    async handleSubmit() {
      if (!this.isFormValid) {
        return;
      }

      this.isSubmitting = true;

      try {
        // Build loan data
        const loanData = {
          ...this.form,
          borrowerName: this.form.loanType === 'Outgoing' ? this.form.institutionName : 'Our Gallery',
          lenderName: this.form.loanType === 'Incoming' ? this.form.institutionName : 'Our Gallery'
        };

        // If outgoing, get artwork details
        if (this.form.loanType === 'Outgoing' && this.form.artworkId) {
          const artwork = this.artworksList.find(a => a.id === this.form.artworkId);
          if (artwork) {
            loanData.artworkTitle = artwork.title;
            loanData.artistName = artwork.artist;
            loanData.artworkImage = artwork.imageUrl;
          }
        }

        if (this.isEditMode) {
          loanData.id = this.loan.id;
          await this.updateLoan(loanData);
        } else {
          await this.createLoan(loanData);
        }

        this.$emit('submit', loanData);
      } catch (error) {
        console.error('Error saving loan:', error);
        alert('Failed to save loan. Please try again.');
      } finally {
        this.isSubmitting = false;
      }
    },

    handleCancel() {
      this.$emit('cancel');
    }
  }
};
</script>
