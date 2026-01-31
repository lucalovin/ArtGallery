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
            <label for="artistId" class="block text-sm font-medium text-gray-700 mb-1">
              Artist <span class="text-red-500">*</span>
            </label>
            <select
              id="artistId"
              v-model="form.artistId"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              :class="errors.artistId ? 'border-red-300' : 'border-gray-200'"
            >
              <option value="">Select an artist</option>
              <option v-for="artist in artists" :key="artist.id" :value="artist.id">
                {{ artist.name }}{{ artist.description ? ` (${artist.description})` : '' }}
              </option>
            </select>
            <p v-if="errors.artistId" class="mt-1 text-sm text-red-500">{{ errors.artistId }}</p>
          </div>

          <!-- Year -->
          <div>
            <label for="yearCreated" class="block text-sm font-medium text-gray-700 mb-1">
              Year Created <span class="text-red-500">*</span>
            </label>
            <input
              id="yearCreated"
              v-model.number="form.yearCreated"
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
        </div>
      </div>

      <!-- Location & Value Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Location & Value</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Location -->
          <div>
            <label for="locationId" class="block text-sm font-medium text-gray-700 mb-1">
              Current Location
            </label>
            <select
              id="locationId"
              v-model="form.locationId"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">Select a location</option>
              <option v-for="location in locations" :key="location.id" :value="location.id">
                {{ location.name }}{{ location.description ? ` - ${location.description}` : '' }}
              </option>
            </select>
          </div>

          <!-- Collection -->
          <div>
            <label for="collectionId" class="block text-sm font-medium text-gray-700 mb-1">
              Collection
            </label>
            <select
              id="collectionId"
              v-model="form.collectionId"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">Select a collection</option>
              <option v-for="collection in collections" :key="collection.id" :value="collection.id">
                {{ collection.name }}
              </option>
            </select>
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
        </div>
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
 * Properly respects OLTP FK relationships: Artist, Collection, Location
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
      // Lookup data from database
      artists: [],
      collections: [],
      locations: [],
      form: {
        title: '',
        artistId: '',          // FK to Artist table
        yearCreated: null,
        medium: '',
        collectionId: '',      // FK to Collection table
        locationId: '',        // FK to Location table
        estimatedValue: null
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

  async created() {
    await this.loadLookups();
  },

  methods: {
    ...mapActions({
      createArtworkAction: 'artwork/createArtwork',
      updateArtworkAction: 'artwork/updateArtwork',
      fetchArtworkById: 'artwork/fetchArtworkById'
    }),

    async loadLookups() {
      try {
        // Fetch all lookups in parallel
        const [artistsRes, collectionsRes, locationsRes] = await Promise.all([
          this.$api.lookups.getArtists(),
          this.$api.lookups.getCollections(),
          this.$api.lookups.getLocations()
        ]);

        // Handle ApiResponse format: { success: true, data: [...] }
        if (artistsRes.data?.success && artistsRes.data?.data) {
          this.artists = artistsRes.data.data;
        } else if (Array.isArray(artistsRes.data)) {
          this.artists = artistsRes.data;
        }
        
        if (collectionsRes.data?.success && collectionsRes.data?.data) {
          this.collections = collectionsRes.data.data;
        } else if (Array.isArray(collectionsRes.data)) {
          this.collections = collectionsRes.data;
        }
        
        if (locationsRes.data?.success && locationsRes.data?.data) {
          this.locations = locationsRes.data.data;
        } else if (Array.isArray(locationsRes.data)) {
          this.locations = locationsRes.data;
        }
      } catch (error) {
        console.error('Error loading lookups:', error);
      }
    },

    async loadArtwork() {
      this.isLoading = true;

      try {
        const artwork = await this.fetchArtworkById(this.id);
        if (artwork) {
          this.form = {
            title: artwork.title || '',
            artistId: artwork.artistId || '',
            yearCreated: artwork.yearCreated || artwork.year || null,
            medium: artwork.medium || '',
            collectionId: artwork.collectionId || '',
            locationId: artwork.locationId || '',
            estimatedValue: artwork.estimatedValue || null
          };
        }
      } catch (error) {
        console.error('Error loading artwork:', error);
        this.$router.push('/artworks');
      } finally {
        this.isLoading = false;
      }
    },

    resetForm() {
      this.form = {
        title: '',
        artistId: '',
        yearCreated: null,
        medium: '',
        collectionId: '',
        locationId: '',
        estimatedValue: null
      };
      this.errors = {};
    },

    validateForm() {
      this.errors = {};

      if (!this.form.title.trim()) {
        this.errors.title = 'Title is required';
      }

      if (!this.form.artistId) {
        this.errors.artistId = 'Artist is required';
      }

      if (!this.form.yearCreated) {
        this.errors.year = 'Year is required';
      } else if (this.form.yearCreated > this.currentYear) {
        this.errors.year = 'Year cannot be in the future';
      }

      return Object.keys(this.errors).length === 0;
    },

    async handleSubmit() {
      if (!this.validateForm()) {
        return;
      }

      this.isSubmitting = true;

      try {
        // Prepare data for API (matching backend DTO)
        const artworkData = {
          title: this.form.title,
          artistId: parseInt(this.form.artistId),
          yearCreated: this.form.yearCreated,
          medium: this.form.medium || null,
          collectionId: this.form.collectionId ? parseInt(this.form.collectionId) : null,
          locationId: this.form.locationId ? parseInt(this.form.locationId) : null,
          estimatedValue: this.form.estimatedValue || null
        };

        if (this.isEditMode) {
          await this.$api.artworks.update(this.id, artworkData);
        } else {
          await this.$api.artworks.create(artworkData);
        }

        // Navigate back to inventory
        this.$router.push('/artworks');
      } catch (error) {
        console.error('Error saving artwork:', error);
        alert('Failed to save artwork. Please try again.');
      } finally {
        this.isSubmitting = false;
      }
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
