<template>
  <!--
    AddEditArtwork.vue - Add/Edit Artwork Form Page
    Art Gallery Management System
  -->
  <div class="add-edit-artwork-page">
    <!-- Back Button -->
    <button
      @click="goBack"
      class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6 transition-colors"
    >
      <span class="mr-2">←</span>
      Back to {{ isEditMode ? 'Artwork' : 'Inventory' }}
    </button>

    <!-- Page Header -->
    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Artwork' : 'Add New Artwork' }}
      </h1>
      <p class="text-gray-500 mt-1">
        {{ isEditMode ? 'Update artwork information' : 'Enter details for the new artwork' }}
      </p>
    </header>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
      <div class="animate-pulse space-y-6">
        <div class="h-8 bg-gray-200 rounded w-1/3"></div>
        <div class="h-12 bg-gray-200 rounded"></div>
        <div class="h-12 bg-gray-200 rounded"></div>
        <div class="h-32 bg-gray-200 rounded"></div>
      </div>
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Basic Information Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Basic Information</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Title -->
          <div class="md:col-span-2">
            <label for="title" class="block text-sm font-medium text-gray-700 mb-1">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              id="title"
              v-model="form.title"
              type="text"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.title ? 'border-red-300' : 'border-gray-200'"
              placeholder="Enter artwork title"
            />
            <p v-if="errors.title" class="mt-1 text-sm text-red-500">{{ errors.title }}</p>
          </div>

          <!-- Artist -->
          <div>
            <label for="artist" class="block text-sm font-medium text-gray-700 mb-1">
              Artist <span class="text-red-500">*</span>
            </label>
            <input
              id="artist"
              v-model="form.artist"
              type="text"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.artist ? 'border-red-300' : 'border-gray-200'"
              placeholder="Enter artist name"
            />
            <p v-if="errors.artist" class="mt-1 text-sm text-red-500">{{ errors.artist }}</p>
          </div>

          <!-- Year -->
          <div>
            <label for="year" class="block text-sm font-medium text-gray-700 mb-1">
              Year Created <span class="text-red-500">*</span>
            </label>
            <input
              id="year"
              v-model.number="form.year"
              type="number"
              required
              min="1"
              :max="currentYear"
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.year ? 'border-red-300' : 'border-gray-200'"
              placeholder="e.g., 1889"
            />
            <p v-if="errors.year" class="mt-1 text-sm text-red-500">{{ errors.year }}</p>
          </div>

          <!-- Category -->
          <div>
            <label for="category" class="block text-sm font-medium text-gray-700 mb-1">
              Category <span class="text-red-500">*</span>
            </label>
            <select
              id="category"
              v-model="form.category"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.category ? 'border-red-300' : 'border-gray-200'"
            >
              <option value="">Select category</option>
              <option v-for="category in categories" :key="category" :value="category">
                {{ category }}
              </option>
            </select>
            <p v-if="errors.category" class="mt-1 text-sm text-red-500">{{ errors.category }}</p>
          </div>

          <!-- Status -->
          <div>
            <label for="status" class="block text-sm font-medium text-gray-700 mb-1">
              Status <span class="text-red-500">*</span>
            </label>
            <select
              id="status"
              v-model="form.status"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.status ? 'border-red-300' : 'border-gray-200'"
            >
              <option value="">Select status</option>
              <option v-for="status in statuses" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
            <p v-if="errors.status" class="mt-1 text-sm text-red-500">{{ errors.status }}</p>
          </div>

          <!-- Medium -->
          <div>
            <label for="medium" class="block text-sm font-medium text-gray-700 mb-1">
              Medium
            </label>
            <input
              id="medium"
              v-model="form.medium"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="e.g., Oil on canvas"
            />
          </div>

          <!-- Dimensions -->
          <div>
            <label for="dimensions" class="block text-sm font-medium text-gray-700 mb-1">
              Dimensions
            </label>
            <input
              id="dimensions"
              v-model="form.dimensions"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="e.g., 73.7 cm × 92.1 cm"
            />
          </div>

          <!-- Description -->
          <div class="md:col-span-2">
            <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              id="description"
              v-model="form.description"
              rows="4"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="Enter artwork description..."
            ></textarea>
          </div>
        </div>
      </div>

      <!-- Location & Value Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Location & Value</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Location -->
          <div>
            <label for="location" class="block text-sm font-medium text-gray-700 mb-1">
              Current Location
            </label>
            <input
              id="location"
              v-model="form.location"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="e.g., Gallery A, Room 3"
            />
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
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
          </div>

          <!-- Estimated Value -->
          <div>
            <label for="estimatedValue" class="block text-sm font-medium text-gray-700 mb-1">
              Estimated Value (USD)
            </label>
            <input
              id="estimatedValue"
              v-model.number="form.estimatedValue"
              type="number"
              min="0"
              step="100"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="Enter estimated value"
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
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="https://example.com/image.jpg"
            />
          </div>
        </div>
      </div>

      <!-- Tags Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Tags</h2>
        
        <!-- Tag Input -->
        <div class="flex items-center space-x-2 mb-4">
          <input
            v-model="newTag"
            type="text"
            class="flex-1 px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="Add a tag..."
            @keydown.enter.prevent="addTag"
          />
          <button
            type="button"
            @click="addTag"
            class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
          >
            Add
          </button>
        </div>

        <!-- Tag List -->
        <div v-if="form.tags.length > 0" class="flex flex-wrap gap-2">
          <span 
            v-for="(tag, index) in form.tags" 
            :key="index"
            class="inline-flex items-center px-3 py-1 bg-primary-100 text-primary-700 rounded-full text-sm"
          >
            {{ tag }}
            <button
              type="button"
              @click="removeTag(index)"
              class="ml-2 hover:text-primary-900"
            >
              ×
            </button>
          </span>
        </div>
        <p v-else class="text-sm text-gray-400">No tags added yet</p>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-4">
        <button
          type="button"
          @click="goBack"
          class="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors font-medium"
        >
          Cancel
        </button>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="isSubmitting">{{ isEditMode ? 'Updating...' : 'Creating...' }}</span>
          <span v-else>{{ isEditMode ? 'Update Artwork' : 'Create Artwork' }}</span>
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapActions } from 'vuex';

/**
 * AddEditArtwork Page Component
 * Form for creating and editing artworks
 */
export default {
  name: 'AddEditArtworkPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      isLoading: false,
      isSubmitting: false,
      newTag: '',
      categories: ['Painting', 'Sculpture', 'Photography', 'Drawing', 'Print', 'Mixed Media', 'Digital Art'],
      statuses: ['On Display', 'In Storage', 'On Loan', 'Under Restoration'],
      form: {
        title: '',
        artist: '',
        year: null,
        category: '',
        status: '',
        medium: '',
        dimensions: '',
        description: '',
        location: '',
        acquisitionDate: '',
        estimatedValue: null,
        imageUrl: '',
        tags: []
      },
      errors: {}
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    },

    currentYear() {
      return new Date().getFullYear();
    }
  },

  watch: {
    id: {
      immediate: true,
      handler(newId) {
        if (newId) {
          this.loadArtwork();
        } else {
          this.resetForm();
        }
      }
    }
  },

  methods: {
    ...mapActions({
      createArtwork: 'artwork/createArtwork',
      updateArtwork: 'artwork/updateArtwork',
      fetchArtwork: 'artwork/fetchArtwork'
    }),

    async loadArtwork() {
      this.isLoading = true;

      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 500));

        // Mock data - in real app, fetch from API
        this.form = {
          title: 'Starry Night',
          artist: 'Vincent van Gogh',
          year: 1889,
          category: 'Painting',
          status: 'On Display',
          medium: 'Oil on canvas',
          dimensions: '73.7 cm × 92.1 cm',
          description: 'The Starry Night is an oil-on-canvas painting by Vincent van Gogh.',
          location: 'Gallery A, Room 3',
          acquisitionDate: '2020-03-15',
          estimatedValue: 1500000,
          imageUrl: '',
          tags: ['Post-Impressionism', 'Night Scene']
        };
      } catch (error) {
        console.error('Error loading artwork:', error);
      } finally {
        this.isLoading = false;
      }
    },

    resetForm() {
      this.form = {
        title: '',
        artist: '',
        year: null,
        category: '',
        status: '',
        medium: '',
        dimensions: '',
        description: '',
        location: '',
        acquisitionDate: '',
        estimatedValue: null,
        imageUrl: '',
        tags: []
      };
      this.errors = {};
    },

    validateForm() {
      this.errors = {};

      if (!this.form.title.trim()) {
        this.errors.title = 'Title is required';
      }

      if (!this.form.artist.trim()) {
        this.errors.artist = 'Artist is required';
      }

      if (!this.form.year) {
        this.errors.year = 'Year is required';
      } else if (this.form.year > this.currentYear) {
        this.errors.year = 'Year cannot be in the future';
      }

      if (!this.form.category) {
        this.errors.category = 'Category is required';
      }

      if (!this.form.status) {
        this.errors.status = 'Status is required';
      }

      return Object.keys(this.errors).length === 0;
    },

    async handleSubmit() {
      if (!this.validateForm()) {
        return;
      }

      this.isSubmitting = true;

      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 800));

        if (this.isEditMode) {
          // await this.updateArtwork({ id: this.id, ...this.form });
          console.log('Updating artwork:', this.form);
        } else {
          // await this.createArtwork(this.form);
          console.log('Creating artwork:', this.form);
        }

        // Navigate back to inventory
        this.$router.push('/artworks');
      } catch (error) {
        console.error('Error saving artwork:', error);
      } finally {
        this.isSubmitting = false;
      }
    },

    addTag() {
      const tag = this.newTag.trim();
      if (tag && !this.form.tags.includes(tag)) {
        this.form.tags.push(tag);
        this.newTag = '';
      }
    },

    removeTag(index) {
      this.form.tags.splice(index, 1);
    },

    goBack() {
      if (this.isEditMode) {
        this.$router.push({ name: 'ArtworkDetail', params: { id: this.id } });
      } else {
        this.$router.push('/artworks');
      }
    }
  }
};
</script>

<style scoped>
.add-edit-artwork-page {
  padding: 1.5rem;
  max-width: 1000px;
  margin: 0 auto;
}
</style>
