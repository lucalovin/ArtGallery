<template>
  <!--
    ArtworkForm.vue - Artwork Create/Edit Form Component
    Art Gallery Management System
  -->
  <div class="artwork-form">
    <!-- Form Header -->
    <div class="mb-6">
      <h2 class="text-2xl font-display font-bold text-gray-900">
        {{ isEditMode ? 'Edit Artwork' : 'Add New Artwork' }}
      </h2>
      <p class="text-gray-600 mt-1">
        {{ isEditMode ? 'Update artwork information' : 'Enter the artwork details below' }}
      </p>
    </div>

    <!-- Error Alert -->
    <div v-if="error" class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
      <div class="flex items-start space-x-3">
        <svg class="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="flex-1">
          <p class="text-red-800 font-medium">{{ error }}</p>
        </div>
        <button type="button" @click="error = ''" class="text-red-500 hover:text-red-700">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Main Form -->
    <form @submit.prevent="handleSubmit" class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
      <!-- Loading Overlay -->
      <div v-if="loading" class="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10 rounded-lg">
        <div class="flex flex-col items-center space-y-3">
          <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600"></div>
          <span class="text-gray-600">{{ isEditMode ? 'Updating...' : 'Saving...' }}</span>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Left Column - Basic Info -->
        <div class="space-y-6">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2">
            Basic Information
          </h3>

          <!-- Title Field -->
          <div>
            <label for="title" class="block text-sm font-medium text-gray-700 mb-1">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              id="title"
              v-model="form.title"
              type="text"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.title }"
              placeholder="Enter artwork title"
              required
              v-focus
            />
            <p v-if="errors.title" class="mt-1 text-sm text-red-600">{{ errors.title }}</p>
          </div>

          <!-- Artist Field -->
          <div>
            <label for="artist" class="block text-sm font-medium text-gray-700 mb-1">
              Artist <span class="text-red-500">*</span>
            </label>
            <input
              id="artist"
              v-model="form.artist"
              type="text"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.artist }"
              placeholder="Enter artist name"
              required
            />
            <p v-if="errors.artist" class="mt-1 text-sm text-red-600">{{ errors.artist }}</p>
          </div>

          <!-- Year Field -->
          <div>
            <label for="year" class="block text-sm font-medium text-gray-700 mb-1">
              Year Created
            </label>
            <input
              id="year"
              v-model.number="form.year"
              type="number"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.year }"
              placeholder="e.g., 1889"
              :min="1"
              :max="currentYear"
            />
            <p v-if="errors.year" class="mt-1 text-sm text-red-600">{{ errors.year }}</p>
          </div>

          <!-- Medium Field -->
          <div>
            <label for="medium" class="block text-sm font-medium text-gray-700 mb-1">
              Medium <span class="text-red-500">*</span>
            </label>
            <select
              id="medium"
              v-model="form.medium"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.medium }"
              required
            >
              <option value="">Select medium</option>
              <option v-for="medium in mediumOptions" :key="medium" :value="medium">
                {{ medium }}
              </option>
            </select>
            <p v-if="errors.medium" class="mt-1 text-sm text-red-600">{{ errors.medium }}</p>
          </div>

          <!-- Style Field -->
          <div>
            <label for="style" class="block text-sm font-medium text-gray-700 mb-1">
              Art Style
            </label>
            <select
              id="style"
              v-model="form.style"
              class="form-input w-full"
            >
              <option value="">Select style</option>
              <option v-for="style in styleOptions" :key="style" :value="style">
                {{ style }}
              </option>
            </select>
          </div>

          <!-- Description Field -->
          <div>
            <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              id="description"
              v-model="form.description"
              rows="4"
              class="form-input w-full"
              placeholder="Enter artwork description..."
            ></textarea>
            <p class="mt-1 text-sm text-gray-500">{{ descriptionCharCount }} / 500 characters</p>
          </div>
        </div>

        <!-- Right Column - Details & Values -->
        <div class="space-y-6">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2">
            Details & Valuation
          </h3>

          <!-- Dimensions -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Dimensions (cm)
            </label>
            <div class="grid grid-cols-3 gap-3">
              <div>
                <input
                  v-model.number="form.width"
                  type="number"
                  class="form-input w-full"
                  placeholder="Width"
                  min="0"
                  step="0.1"
                />
              </div>
              <div>
                <input
                  v-model.number="form.height"
                  type="number"
                  class="form-input w-full"
                  placeholder="Height"
                  min="0"
                  step="0.1"
                />
              </div>
              <div>
                <input
                  v-model.number="form.depth"
                  type="number"
                  class="form-input w-full"
                  placeholder="Depth"
                  min="0"
                  step="0.1"
                />
              </div>
            </div>
          </div>

          <!-- Estimated Value -->
          <div>
            <label for="estimatedValue" class="block text-sm font-medium text-gray-700 mb-1">
              Estimated Value (USD) <span class="text-red-500">*</span>
            </label>
            <div class="relative">
              <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
              <input
                id="estimatedValue"
                v-model.number="form.estimatedValue"
                type="number"
                class="form-input w-full pl-8"
                :class="{ 'border-red-300': errors.estimatedValue }"
                placeholder="0.00"
                min="0"
                step="100"
                required
              />
            </div>
            <p v-if="errors.estimatedValue" class="mt-1 text-sm text-red-600">{{ errors.estimatedValue }}</p>
            <p v-else class="mt-1 text-sm text-gray-500">Formatted: {{ formattedValue }}</p>
          </div>

          <!-- Acquisition Date -->
          <div>
            <label for="acquisitionDate" class="block text-sm font-medium text-gray-700 mb-1">
              Acquisition Date
            </label>
            <input
              id="acquisitionDate"
              v-model="form.acquisitionDate"
              type="date"
              class="form-input w-full"
              :max="today"
            />
          </div>

          <!-- Acquisition Method -->
          <div>
            <label for="acquisitionMethod" class="block text-sm font-medium text-gray-700 mb-1">
              Acquisition Method
            </label>
            <select
              id="acquisitionMethod"
              v-model="form.acquisitionMethod"
              class="form-input w-full"
            >
              <option value="">Select method</option>
              <option v-for="method in acquisitionMethods" :key="method" :value="method">
                {{ method }}
              </option>
            </select>
          </div>

          <!-- Status -->
          <div>
            <label for="status" class="block text-sm font-medium text-gray-700 mb-1">
              Status <span class="text-red-500">*</span>
            </label>
            <select
              id="status"
              v-model="form.status"
              class="form-input w-full"
              required
            >
              <option v-for="status in statusOptions" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </div>

          <!-- Location -->
          <div>
            <label for="location" class="block text-sm font-medium text-gray-700 mb-1">
              Current Location
            </label>
            <input
              id="location"
              v-model="form.location"
              type="text"
              class="form-input w-full"
              placeholder="e.g., Gallery A, Room 3"
            />
          </div>

          <!-- Image URL -->
          <div>
            <label for="imageUrl" class="block text-sm font-medium text-gray-700 mb-1">
              Image URL
            </label>
            <input
              id="imageUrl"
              v-model="form.imageUrl"
              type="url"
              class="form-input w-full"
              placeholder="https://example.com/image.jpg"
            />
            <!-- Image Preview -->
            <div v-if="form.imageUrl" class="mt-2">
              <img
                :src="form.imageUrl"
                :alt="form.title"
                class="w-32 h-32 object-cover rounded-lg border border-gray-200"
                @error="handlePreviewError"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-3 pt-6 mt-6 border-t border-gray-100">
        <button
          type="button"
          @click="handleCancel"
          :disabled="loading"
          class="btn btn-secondary"
        >
          Cancel
        </button>
        <button
          type="button"
          @click="resetForm"
          :disabled="loading"
          class="btn btn-secondary"
        >
          Reset
        </button>
        <button
          type="submit"
          :disabled="loading || !isFormValid"
          class="btn btn-primary"
          :class="{ 'opacity-50 cursor-not-allowed': !isFormValid }"
        >
          {{ isEditMode ? 'Update Artwork' : 'Create Artwork' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapActions } from 'vuex';

/**
 * ArtworkForm Component - OPTIONS API
 */
export default {
  name: 'ArtworkForm',

  /**
   * Props - For edit mode
   */
  props: {
    // Existing artwork data for edit mode
    artwork: {
      type: Object,
      default: null
    },
    // Loading state from parent
    submitting: {
      type: Boolean,
      default: false
    }
  },

  /**
   * Emits - Form events
   */
  emits: ['submit', 'cancel', 'dirty'],

  /**
   * data() - Form state following Lab pattern
   */
  data() {
    return {
      // Form data with default values
      form: {
        title: '',
        artist: '',
        year: null,
        medium: '',
        style: '',
        description: '',
        width: null,
        height: null,
        depth: null,
        estimatedValue: null,
        acquisitionDate: '',
        acquisitionMethod: '',
        status: 'Available',
        location: '',
        imageUrl: ''
      },

      // Validation errors
      errors: {},

      // Local loading state
      loading: false,

      // Error message
      error: '',

      // Form dirty state
      isDirty: false,

      // Dropdown options
      mediumOptions: [
        'Oil on Canvas',
        'Acrylic on Canvas',
        'Watercolor',
        'Tempera',
        'Fresco',
        'Pastel',
        'Charcoal',
        'Ink',
        'Mixed Media',
        'Digital Art',
        'Photography',
        'Sculpture - Bronze',
        'Sculpture - Marble',
        'Sculpture - Wood',
        'Sculpture - Clay',
        'Installation',
        'Video Art',
        'Performance Art',
        'Other'
      ],

      styleOptions: [
        'Renaissance',
        'Baroque',
        'Rococo',
        'Neoclassicism',
        'Romanticism',
        'Impressionism',
        'Post-Impressionism',
        'Expressionism',
        'Cubism',
        'Surrealism',
        'Abstract Expressionism',
        'Pop Art',
        'Minimalism',
        'Contemporary',
        'Modern',
        'Realism',
        'Hyperrealism',
        'Art Nouveau',
        'Art Deco',
        'Other'
      ],

      acquisitionMethods: [
        'Purchase',
        'Donation',
        'Bequest',
        'Gift',
        'Exchange',
        'Commission',
        'Transfer',
        'Found',
        'Other'
      ],

      statusOptions: [
        'Available',
        'On Display',
        'On Loan',
        'In Restoration',
        'In Storage',
        'Sold'
      ]
    };
  },

  /**
   * Computed properties
   */
  computed: {
    /**
     * Check if in edit mode
     */
    isEditMode() {
      return this.artwork !== null && this.artwork.id !== undefined;
    },

    /**
     * Get current year for validation
     */
    currentYear() {
      return new Date().getFullYear();
    },

    /**
     * Get today's date for date input max
     */
    today() {
      return new Date().toISOString().split('T')[0];
    },

    /**
     * Character count for description
     */
    descriptionCharCount() {
      return this.form.description ? this.form.description.length : 0;
    },

    /**
     * Format estimated value for display
     */
    formattedValue() {
      if (!this.form.estimatedValue) return '$0.00';
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(this.form.estimatedValue);
    },

    /**
     * Form validation - computed property pattern
     * Returns true if all required fields are valid
     */
    isFormValid() {
      return (
        this.form.title.trim() !== '' &&
        this.form.artist.trim() !== '' &&
        this.form.medium !== '' &&
        this.form.estimatedValue !== null &&
        this.form.estimatedValue >= 0 &&
        Object.keys(this.errors).length === 0
      );
    }
  },

  /**
   * Watch
   */
  watch: {
    /**
     * Watch artwork prop for edit mode changes
     */
    artwork: {
      immediate: true,
      deep: true,
      handler(newArtwork) {
        if (newArtwork) {
          this.populateForm(newArtwork);
        }
      }
    },

    /**
     * Watch form for changes (deep watch)
     */
    form: {
      deep: true,
      handler() {
        this.isDirty = true;
        this.$emit('dirty', true);
        this.validateForm();
      }
    },

    /**
     * Watch title for validation
     */
    'form.title'(newValue) {
      if (!newValue || newValue.trim() === '') {
        this.errors.title = 'Title is required';
      } else if (newValue.length < 2) {
        this.errors.title = 'Title must be at least 2 characters';
      } else {
        delete this.errors.title;
      }
    },

    /**
     * Watch year for validation
     */
    'form.year'(newValue) {
      if (newValue && (newValue < 1 || newValue > this.currentYear)) {
        this.errors.year = `Year must be between 1 and ${this.currentYear}`;
      } else {
        delete this.errors.year;
      }
    },

    /**
     * Watch submitting prop
     */
    submitting(newValue) {
      this.loading = newValue;
    }
  },

  /**
   * created - Lifecycle hook
   * Initialize form data
   */
  created() {
    console.log('[ArtworkForm] Created, edit mode:', this.isEditMode);
    
    if (this.artwork) {
      this.populateForm(this.artwork);
    }
  },

  /**
   * mounted - Lifecycle hook
   * Focus first input
   */
  mounted() {
    // Add beforeunload listener for unsaved changes
    window.addEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * beforeUnmount - Cleanup
   */
  beforeUnmount() {
    window.removeEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * Methods - Form handlers
   */
  methods: {
    // Map Vuex actions
    ...mapActions('artwork', ['createArtwork', 'updateArtwork']),

    /**
     * Populate form with existing artwork data
     */
    populateForm(artwork) {
      this.form = {
        title: artwork.title || '',
        artist: typeof artwork.artist === 'object' ? artwork.artist.name : (artwork.artist || ''),
        year: artwork.year || null,
        medium: artwork.medium || '',
        style: artwork.style || '',
        description: artwork.description || '',
        width: artwork.width || null,
        height: artwork.height || null,
        depth: artwork.depth || null,
        estimatedValue: artwork.estimatedValue || null,
        acquisitionDate: artwork.acquisitionDate || '',
        acquisitionMethod: artwork.acquisitionMethod || '',
        status: artwork.status || 'Available',
        location: artwork.location || '',
        imageUrl: artwork.imageUrl || ''
      };
      
      // Reset dirty state after populating
      this.$nextTick(() => {
        this.isDirty = false;
      });
    },

    /**
     * Validate entire form
     */
    validateForm() {
      this.errors = {};

      if (!this.form.title || this.form.title.trim() === '') {
        this.errors.title = 'Title is required';
      }

      if (!this.form.artist || this.form.artist.trim() === '') {
        this.errors.artist = 'Artist is required';
      }

      if (!this.form.medium) {
        this.errors.medium = 'Medium is required';
      }

      if (this.form.estimatedValue === null || this.form.estimatedValue < 0) {
        this.errors.estimatedValue = 'Valid estimated value is required';
      }

      if (this.form.year && (this.form.year < 1 || this.form.year > this.currentYear)) {
        this.errors.year = `Year must be between 1 and ${this.currentYear}`;
      }

      return Object.keys(this.errors).length === 0;
    },

    /**
     * Handle form submission
     */
    async handleSubmit() {
      if (!this.validateForm()) {
        this.error = 'Please fix the errors below';
        return;
      }

      this.loading = true;
      this.error = '';

      try {
        const artworkData = {
          ...this.form,
          updatedAt: new Date().toISOString()
        };

        if (this.isEditMode) {
          artworkData.id = this.artwork.id;
        } else {
          artworkData.createdAt = new Date().toISOString();
        }

        this.$emit('submit', artworkData);
        this.isDirty = false;
      } catch (err) {
        this.error = err.message || 'An error occurred while saving';
        console.error('[ArtworkForm] Submit error:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Handle cancel action
     */
    handleCancel() {
      if (this.isDirty) {
        if (!confirm('You have unsaved changes. Are you sure you want to cancel?')) {
          return;
        }
      }
      this.$emit('cancel');
      this.$router.back();
    },

    /**
     * Reset form to initial state
     */
    resetForm() {
      if (this.isEditMode && this.artwork) {
        this.populateForm(this.artwork);
      } else {
        this.form = {
          title: '',
          artist: '',
          year: null,
          medium: '',
          style: '',
          description: '',
          width: null,
          height: null,
          depth: null,
          estimatedValue: null,
          acquisitionDate: '',
          acquisitionMethod: '',
          status: 'Available',
          location: '',
          imageUrl: ''
        };
      }
      this.errors = {};
      this.error = '';
      this.isDirty = false;
    },

    /**
     * Handle image preview error
     */
    handlePreviewError(event) {
      event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"%3E%3Crect fill="%23f3f4f6" width="24" height="24"/%3E%3C/svg%3E';
    },

    /**
     * Handle browser beforeunload
     */
    handleBeforeUnload(event) {
      if (this.isDirty) {
        event.preventDefault();
        event.returnValue = 'You have unsaved changes.';
        return event.returnValue;
      }
    }
  }
};
</script>

<style scoped>
/* Form animations */
.artwork-form {
  @apply relative;
}
</style>
