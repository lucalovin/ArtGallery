<template>
  <!--
    ExhibitionForm.vue - Exhibition Create/Edit Form Component
    Art Gallery Management System
  -->
  <div class="exhibition-form">
    <!-- Form Header -->
    <div class="mb-6">
      <h2 class="text-2xl font-display font-bold text-gray-900">
        {{ isEditMode ? 'Edit Exhibition' : 'Create New Exhibition' }}
      </h2>
      <p class="text-gray-600 mt-1">
        {{ isEditMode ? 'Update exhibition details' : 'Fill in the exhibition information' }}
      </p>
    </div>

    <!-- Error Alert -->
    <div v-if="error" class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
      <div class="flex items-start space-x-3">
        <svg class="w-5 h-5 text-red-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Basic Info Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-3 mb-6">
          Basic Information
        </h3>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Title -->
          <div class="lg:col-span-2">
            <label for="title" class="block text-sm font-medium text-gray-700 mb-1">
              Exhibition Title <span class="text-red-500">*</span>
            </label>
            <input
              id="title"
              v-model="form.title"
              type="text"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.title }"
              placeholder="Enter exhibition title"
              required
              v-focus
            />
            <p v-if="errors.title" class="mt-1 text-sm text-red-600">{{ errors.title }}</p>
          </div>

          <!-- Start Date -->
          <div>
            <label for="startDate" class="block text-sm font-medium text-gray-700 mb-1">
              Start Date <span class="text-red-500">*</span>
            </label>
            <input
              id="startDate"
              v-model="form.startDate"
              type="date"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.startDate }"
              required
            />
            <p v-if="errors.startDate" class="mt-1 text-sm text-red-600">{{ errors.startDate }}</p>
          </div>

          <!-- End Date -->
          <div>
            <label for="endDate" class="block text-sm font-medium text-gray-700 mb-1">
              End Date <span class="text-red-500">*</span>
            </label>
            <input
              id="endDate"
              v-model="form.endDate"
              type="date"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.endDate }"
              :min="form.startDate"
              required
            />
            <p v-if="errors.endDate" class="mt-1 text-sm text-red-600">{{ errors.endDate }}</p>
          </div>

          <!-- Location -->
          <div>
            <label for="location" class="block text-sm font-medium text-gray-700 mb-1">
              Location <span class="text-red-500">*</span>
            </label>
            <input
              id="location"
              v-model="form.location"
              type="text"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.location }"
              placeholder="e.g., Main Gallery, West Wing"
              required
            />
            <p v-if="errors.location" class="mt-1 text-sm text-red-600">{{ errors.location }}</p>
          </div>

          <!-- Curator -->
          <div>
            <label for="curator" class="block text-sm font-medium text-gray-700 mb-1">
              Curator
            </label>
            <input
              id="curator"
              v-model="form.curator"
              type="text"
              class="form-input w-full"
              placeholder="Enter curator name"
            />
          </div>

          <!-- Description -->
          <div class="lg:col-span-2">
            <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              id="description"
              v-model="form.description"
              rows="4"
              class="form-input w-full"
              placeholder="Describe the exhibition theme, highlights, etc."
            ></textarea>
            <p class="mt-1 text-sm text-gray-500">{{ descriptionCharCount }} / 1000 characters</p>
          </div>

          <!-- Image URL -->
          <div class="lg:col-span-2">
            <label for="imageUrl" class="block text-sm font-medium text-gray-700 mb-1">
              Banner Image URL
            </label>
            <input
              id="imageUrl"
              v-model="form.imageUrl"
              type="url"
              class="form-input w-full"
              placeholder="https://example.com/banner.jpg"
            />
            <div v-if="form.imageUrl" class="mt-2">
              <img
                :src="form.imageUrl"
                :alt="form.title"
                class="w-full h-32 object-cover rounded-lg border border-gray-200"
                @error="handlePreviewError"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Artwork Selection Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center justify-between border-b border-gray-100 pb-3 mb-6">
          <h3 class="text-lg font-semibold text-gray-900">
            Artworks in Exhibition
          </h3>
          <span class="text-sm text-gray-500">
            {{ selectedArtworksCount }} selected
          </span>
        </div>

        <!-- Search Available Artworks -->
        <div class="mb-4">
          <div class="relative">
            <svg class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="artworkSearch"
              type="text"
              placeholder="Search artworks to add..."
              class="form-input pl-10 w-full"
            />
          </div>
        </div>

        <!-- Available Artworks List -->
        <div class="border border-gray-200 rounded-lg max-h-64 overflow-y-auto">
          <div 
            v-for="artwork in filteredAvailableArtworks" 
            :key="artwork.id"
            class="flex items-center justify-between p-3 hover:bg-gray-50 border-b border-gray-100 last:border-b-0"
          >
            <div class="flex items-center space-x-3">
              <input
                type="checkbox"
                :id="'artwork-' + artwork.id"
                :value="artwork.id"
                v-model="form.selectedArtworks"
                class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
              />
              <label :for="'artwork-' + artwork.id" class="flex items-center space-x-3 cursor-pointer">
                <div class="w-12 h-12 bg-gray-100 rounded overflow-hidden flex-shrink-0">
                  <img
                    v-if="artwork.imageUrl"
                    :src="artwork.imageUrl"
                    :alt="artwork.title"
                    class="w-full h-full object-cover"
                  />
                </div>
                <div>
                  <p class="text-sm font-medium text-gray-900">{{ artwork.title }}</p>
                  <p class="text-xs text-gray-500">{{ artwork.artist }} • {{ artwork.year }}</p>
                </div>
              </label>
            </div>
            <span :class="getArtworkStatusClasses(artwork.status)">
              {{ artwork.status }}
            </span>
          </div>

          <!-- Empty State -->
          <div v-if="filteredAvailableArtworks.length === 0" class="p-8 text-center text-gray-500">
            <p>No artworks found matching your search.</p>
          </div>
        </div>

        <!-- Selected Artworks Preview -->
        <div v-if="selectedArtworksCount > 0" class="mt-4">
          <h4 class="text-sm font-medium text-gray-700 mb-2">Selected Artworks:</h4>
          <div class="flex flex-wrap gap-2">
            <span 
              v-for="artworkId in form.selectedArtworks" 
              :key="artworkId"
              class="inline-flex items-center px-3 py-1 rounded-full text-sm bg-primary-100 text-primary-800"
            >
              {{ getArtworkTitle(artworkId) }}
              <button 
                type="button"
                @click="removeArtwork(artworkId)"
                class="ml-2 text-primary-600 hover:text-primary-800"
              >
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                </svg>
              </button>
            </span>
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-3">
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
          <span v-if="loading" class="flex items-center space-x-2">
            <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span>{{ isEditMode ? 'Updating...' : 'Creating...' }}</span>
          </span>
          <span v-else>{{ isEditMode ? 'Update Exhibition' : 'Create Exhibition' }}</span>
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';

/**
 * ExhibitionForm Component
 */
export default {
  name: 'ExhibitionForm',

  /**
   * Props
   */
  props: {
    exhibition: {
      type: Object,
      default: null
    },
    submitting: {
      type: Boolean,
      default: false
    }
  },

  /**
   * Emits
   */
  emits: ['submit', 'cancel', 'dirty'],

  /**
   * data()
   */
  data() {
    return {
      form: {
        title: '',
        startDate: '',
        endDate: '',
        location: '',
        curator: '',
        description: '',
        imageUrl: '',
        selectedArtworks: []
      },
      errors: {},
      loading: false,
      error: '',
      isDirty: false,
      artworkSearch: ''
    };
  },

  /**
   * Computed
   */
  computed: {
    ...mapState('artwork', { availableArtworks: 'artworks' }),

    isEditMode() {
      return this.exhibition !== null && this.exhibition.id !== undefined;
    },

    descriptionCharCount() {
      return this.form.description ? this.form.description.length : 0;
    },

    selectedArtworksCount() {
      return this.form.selectedArtworks.length;
    },

    /**
     * Filter available artworks based on search
     */
    filteredAvailableArtworks() {
      if (!this.artworkSearch) {
        return this.availableArtworks;
      }
      
      const query = this.artworkSearch.toLowerCase();
      return this.availableArtworks.filter(artwork => {
        const title = (artwork.title || '').toLowerCase();
        const artist = (artwork.artist || '').toLowerCase();
        return title.includes(query) || artist.includes(query);
      });
    },

    /**
     * Form validation
     */
    isFormValid() {
      return (
        this.form.title.trim() !== '' &&
        this.form.startDate !== '' &&
        this.form.endDate !== '' &&
        this.form.location.trim() !== '' &&
        new Date(this.form.endDate) >= new Date(this.form.startDate) &&
        Object.keys(this.errors).length === 0
      );
    }
  },

  /**
   * Watch
   */
  watch: {
    exhibition: {
      immediate: true,
      deep: true,
      handler(newExhibition) {
        if (newExhibition) {
          this.populateForm(newExhibition);
        }
      }
    },

    form: {
      deep: true,
      handler() {
        this.isDirty = true;
        this.$emit('dirty', true);
        this.validateForm();
      }
    },

    'form.endDate'(newValue) {
      if (newValue && this.form.startDate && new Date(newValue) < new Date(this.form.startDate)) {
        this.errors.endDate = 'End date must be after start date';
      } else {
        delete this.errors.endDate;
      }
    },

    submitting(newValue) {
      this.loading = newValue;
    }
  },

  /**
   * created
   */
  created() {
    console.log('[ExhibitionForm] Created, edit mode:', this.isEditMode);
    if (this.exhibition) {
      this.populateForm(this.exhibition);
    }
    this.loadAvailableArtworks();
  },

  /**
   * mounted
   */
  mounted() {
    window.addEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * beforeUnmount
   */
  beforeUnmount() {
    window.removeEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * Methods
   */
  methods: {
    ...mapActions('artwork', ['fetchArtworks']),

    /**
     * Load available artworks
     */
    async loadAvailableArtworks() {
      try {
        await this.fetchArtworks();
      } catch (err) {
        console.error('[ExhibitionForm] Failed to load artworks:', err);
      }
    },

    /**
     * Populate form with existing data
     */
    populateForm(exhibition) {
      this.form = {
        title: exhibition.title || '',
        startDate: exhibition.startDate || '',
        endDate: exhibition.endDate || '',
        location: exhibition.location || '',
        curator: exhibition.curator || '',
        description: exhibition.description || '',
        imageUrl: exhibition.imageUrl || '',
        selectedArtworks: exhibition.artworks?.map(a => a.id || a) || []
      };
      
      this.$nextTick(() => {
        this.isDirty = false;
      });
    },

    /**
     * Validate form
     */
    validateForm() {
      this.errors = {};

      if (!this.form.title || this.form.title.trim() === '') {
        this.errors.title = 'Title is required';
      }

      if (!this.form.startDate) {
        this.errors.startDate = 'Start date is required';
      }

      if (!this.form.endDate) {
        this.errors.endDate = 'End date is required';
      } else if (this.form.startDate && new Date(this.form.endDate) < new Date(this.form.startDate)) {
        this.errors.endDate = 'End date must be after start date';
      }

      if (!this.form.location || this.form.location.trim() === '') {
        this.errors.location = 'Location is required';
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
        const exhibitionData = {
          ...this.form,
          artworks: this.form.selectedArtworks,
          updatedAt: new Date().toISOString()
        };

        if (this.isEditMode) {
          exhibitionData.id = this.exhibition.id;
        } else {
          exhibitionData.createdAt = new Date().toISOString();
        }

        this.$emit('submit', exhibitionData);
        this.isDirty = false;
      } catch (err) {
        this.error = err.message || 'An error occurred while saving';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Handle cancel
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
     * Reset form
     */
    resetForm() {
      if (this.isEditMode && this.exhibition) {
        this.populateForm(this.exhibition);
      } else {
        this.form = {
          title: '',
          startDate: '',
          endDate: '',
          location: '',
          curator: '',
          description: '',
          imageUrl: '',
          selectedArtworks: []
        };
      }
      this.errors = {};
      this.error = '';
      this.isDirty = false;
    },

    /**
     * Get artwork title by ID
     */
    getArtworkTitle(artworkId) {
      const artwork = this.availableArtworks.find(a => a.id === artworkId);
      return artwork ? artwork.title : 'Unknown';
    },

    /**
     * Remove artwork from selection
     */
    removeArtwork(artworkId) {
      const index = this.form.selectedArtworks.indexOf(artworkId);
      if (index > -1) {
        this.form.selectedArtworks.splice(index, 1);
      }
    },

    /**
     * Get artwork status badge classes
     */
    getArtworkStatusClasses(status) {
      const base = 'text-xs px-2 py-0.5 rounded-full';
      const statusStyles = {
        'Available': 'bg-green-100 text-green-800',
        'On Display': 'bg-blue-100 text-blue-800',
        'On Loan': 'bg-yellow-100 text-yellow-800'
      };
      return `${base} ${statusStyles[status] || 'bg-gray-100 text-gray-800'}`;
    },

    /**
     * Handle image preview error
     */
    handlePreviewError(event) {
      event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"%3E%3Crect fill="%23f3f4f6" width="24" height="24"/%3E%3C/svg%3E';
    },

    /**
     * Handle beforeunload
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
