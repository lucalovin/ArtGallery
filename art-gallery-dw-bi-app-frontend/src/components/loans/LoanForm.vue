<template>
  <!--
    LoanForm.vue - Artwork Loan Create/Edit Form Component
    Art Gallery Management System
    
    Simplified to match API: artworkId, exhibitorId, startDate, endDate, conditions
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
      <!-- Artwork Selection -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Artwork Selection
        </h3>

        <div>
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
            <option v-for="artwork in artworksList" :key="artwork.id" :value="artwork.id">
              {{ artwork.title }} - {{ artwork.artistName || 'Unknown Artist' }}
            </option>
          </select>
          <p v-if="errors.artworkId" class="text-red-500 text-xs mt-1">{{ errors.artworkId }}</p>
        </div>
      </div>

      <!-- Exhibitor Selection -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Borrower (Exhibitor)
        </h3>

        <div>
          <label for="exhibitorId" class="block text-sm font-medium text-gray-700 mb-1">
            Select Exhibitor <span class="text-red-500">*</span>
          </label>
          <select
            id="exhibitorId"
            v-model="form.exhibitorId"
            class="form-input"
            :class="{ 'border-red-500': errors.exhibitorId }"
          >
            <option value="">Select an exhibitor</option>
            <option v-for="exhibitor in exhibitorsList" :key="exhibitor.id" :value="exhibitor.id">
              {{ exhibitor.name }}
            </option>
          </select>
          <p v-if="errors.exhibitorId" class="text-red-500 text-xs mt-1">{{ errors.exhibitorId }}</p>
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
              End Date
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
            <p class="text-xs text-gray-500 mt-1">Leave empty for an open-ended loan</p>
          </div>
        </div>

        <div v-if="loanDuration" class="p-3 bg-blue-50 rounded-lg">
          <p class="text-sm text-blue-700">
            <strong>Loan Duration:</strong> {{ loanDuration }}
          </p>
        </div>
      </div>

      <!-- Conditions -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Conditions
        </h3>

        <div>
          <label for="conditions" class="block text-sm font-medium text-gray-700 mb-1">
            Loan Conditions
          </label>
          <textarea
            id="conditions"
            v-model.trim="form.conditions"
            class="form-input"
            rows="4"
            placeholder="Any special handling, display, environmental requirements, or other conditions..."
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
import { mapActions } from 'vuex';

/**
 * LoanForm Component
 * Create and edit artwork loan agreements
 * 
 * API fields: artworkId, exhibitorId, startDate, endDate, conditions
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
    },
    exhibitorsList: {
      type: Array,
      default: () => []
    }
  },

  emits: ['submit', 'cancel'],

  data() {
    return {
      form: {
        artworkId: '',
        exhibitorId: '',
        startDate: '',
        endDate: '',
        conditions: ''
      },
      errors: {},
      isSubmitting: false
    };
  },

  computed: {
    isEditMode() {
      return this.loan !== null && this.loan.id !== undefined;
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

    isFormValid() {
      return (
        this.form.artworkId &&
        this.form.exhibitorId &&
        this.form.startDate &&
        (!this.form.endDate || new Date(this.form.endDate) > new Date(this.form.startDate)) &&
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
      if (!value) {
        this.errors.artworkId = 'Please select an artwork';
      } else {
        delete this.errors.artworkId;
      }
    },

    'form.exhibitorId'(value) {
      if (!value) {
        this.errors.exhibitorId = 'Please select an exhibitor';
      } else {
        delete this.errors.exhibitorId;
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

    'form.endDate'() {
      this.validateEndDate();
    }
  },

  methods: {
    ...mapActions('loans', ['createLoan', 'updateLoan']),

    populateForm(loan) {
      // Format dates for input fields (YYYY-MM-DD)
      const formatDate = (dateStr) => {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toISOString().split('T')[0];
      };

      this.form = {
        artworkId: loan.artworkId || '',
        exhibitorId: loan.exhibitorId || '',
        startDate: formatDate(loan.startDate),
        endDate: formatDate(loan.endDate),
        conditions: loan.conditions || ''
      };
      this.errors = {};
    },

    resetForm() {
      this.form = {
        artworkId: '',
        exhibitorId: '',
        startDate: '',
        endDate: '',
        conditions: ''
      };
      this.errors = {};

      if (this.loan) {
        this.populateForm(this.loan);
      }
    },

    validateEndDate() {
      if (this.form.endDate && this.form.startDate && 
          new Date(this.form.endDate) <= new Date(this.form.startDate)) {
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
        // Build loan data matching API structure
        const loanData = {
          artworkId: parseInt(this.form.artworkId),
          exhibitorId: parseInt(this.form.exhibitorId),
          startDate: this.form.startDate,
          endDate: this.form.endDate || null,
          conditions: this.form.conditions || null
        };

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

<style scoped>
.loan-form {
  max-width: 800px;
}

.form-input {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  line-height: 1.25rem;
  transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input.border-red-500 {
  border-color: #ef4444;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 0.375rem;
  font-weight: 500;
  font-size: 0.875rem;
  transition: all 0.15s ease-in-out;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #2563eb;
}

.btn-secondary {
  background-color: #f3f4f6;
  color: #374151;
}

.btn-secondary:hover {
  background-color: #e5e7eb;
}
</style>
