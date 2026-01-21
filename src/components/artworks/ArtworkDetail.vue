<template>
  <!--
    ArtworkDetail.vue - Artwork Detail View Component
    Art Gallery Management System
  -->
  <div class="artwork-detail">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="flex flex-col items-center space-y-4">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="text-gray-600">Loading artwork details...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-xl p-8 text-center">
      <svg class="w-12 h-12 text-red-500 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
      </svg>
      <h3 class="text-lg font-semibold text-red-800 mb-2">Error Loading Artwork</h3>
      <p class="text-red-600 mb-4">{{ error }}</p>
      <button @click="fetchArtwork" class="btn btn-primary">Try Again</button>
    </div>

    <!-- Artwork Content -->
    <div v-else-if="artwork" class="space-y-8">
      <!-- Breadcrumb -->
      <nav class="flex items-center space-x-2 text-sm text-gray-500">
        <router-link to="/" class="hover:text-primary-600">Home</router-link>
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
        </svg>
        <router-link to="/artworks" class="hover:text-primary-600">Artworks</router-link>
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
        </svg>
        <span class="text-gray-900 font-medium">{{ artwork.title }}</span>
      </nav>

      <!-- Main Content Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Image Section -->
        <div class="space-y-4">
          <!-- Main Image -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div class="aspect-square bg-gray-100 relative">
              <img
                v-if="artwork.imageUrl"
                :src="artwork.imageUrl"
                :alt="artwork.title"
                class="w-full h-full object-contain"
                @error="handleImageError"
              />
              <div v-else class="w-full h-full flex items-center justify-center">
                <svg class="w-24 h-24 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>

              <!-- Status Badge -->
              <div class="absolute top-4 left-4">
                <span :class="statusBadgeClasses">
                  {{ artwork.status || 'Available' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Quick Stats -->
          <div class="grid grid-cols-2 gap-4">
            <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
              <p class="text-sm text-gray-500 mb-1">Year Created</p>
              <p class="text-2xl font-bold text-gray-900">{{ artwork.year || 'Unknown' }}</p>
            </div>
            <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
              <p class="text-sm text-gray-500 mb-1">Estimated Value</p>
              <p class="text-2xl font-bold text-primary-600" v-currency="artwork.estimatedValue"></p>
            </div>
          </div>
        </div>

        <!-- Details Section -->
        <div class="space-y-6">
          <!-- Header -->
          <div>
            <div class="flex items-start justify-between">
              <div>
                <span class="text-sm font-medium text-primary-600 uppercase tracking-wider">
                  {{ artwork.medium }}
                </span>
                <h1 class="text-3xl font-display font-bold text-gray-900 mt-1">
                  {{ artwork.title }}
                </h1>
                <p class="text-xl text-gray-600 mt-2">
                  by <span class="font-medium text-gray-900">{{ artistName }}</span>
                </p>
              </div>
              
              <!-- Action Buttons -->
              <div class="flex items-center space-x-2">
                <button
                  @click="editArtwork"
                  class="btn btn-secondary"
                >
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                  Edit
                </button>
                <button
                  @click="confirmDelete"
                  class="btn btn-danger"
                >
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  Delete
                </button>
              </div>
            </div>
          </div>

          <!-- Description -->
          <div v-if="artwork.description" class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-900 mb-3">Description</h2>
            <p class="text-gray-600 leading-relaxed">{{ artwork.description }}</p>
          </div>

          <!-- Details Card -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-900 mb-4">Artwork Details</h2>
            
            <dl class="space-y-4">
              <!-- Style -->
              <div v-if="artwork.style" class="flex justify-between">
                <dt class="text-gray-500">Art Style</dt>
                <dd class="text-gray-900 font-medium">{{ artwork.style }}</dd>
              </div>

              <!-- Dimensions -->
              <div v-if="dimensions" class="flex justify-between">
                <dt class="text-gray-500">Dimensions</dt>
                <dd class="text-gray-900 font-medium">{{ dimensions }}</dd>
              </div>

              <!-- Location -->
              <div v-if="artwork.location" class="flex justify-between">
                <dt class="text-gray-500">Current Location</dt>
                <dd class="text-gray-900 font-medium">{{ artwork.location }}</dd>
              </div>

              <!-- Acquisition Date -->
              <div v-if="artwork.acquisitionDate" class="flex justify-between">
                <dt class="text-gray-500">Acquisition Date</dt>
                <dd class="text-gray-900 font-medium">{{ formattedAcquisitionDate }}</dd>
              </div>

              <!-- Acquisition Method -->
              <div v-if="artwork.acquisitionMethod" class="flex justify-between">
                <dt class="text-gray-500">Acquisition Method</dt>
                <dd class="text-gray-900 font-medium">{{ artwork.acquisitionMethod }}</dd>
              </div>
            </dl>
          </div>

          <!-- Provenance / History -->
          <div v-if="artwork.provenance" class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-900 mb-3">Provenance</h2>
            <p class="text-gray-600 leading-relaxed">{{ artwork.provenance }}</p>
          </div>

          <!-- Related Exhibitions -->
          <div v-if="relatedExhibitions.length > 0" class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-900 mb-4">Exhibition History</h2>
            <ul class="space-y-3">
              <li 
                v-for="exhibition in relatedExhibitions" 
                :key="exhibition.id"
                class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
              >
                <div>
                  <p class="font-medium text-gray-900">{{ exhibition.title }}</p>
                  <p class="text-sm text-gray-500">{{ exhibition.dates }}</p>
                </div>
                <router-link 
                  :to="{ name: 'exhibition-detail', params: { id: exhibition.id } }"
                  class="text-primary-600 hover:text-primary-700 text-sm font-medium"
                >
                  View →
                </router-link>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Metadata Footer -->
      <div class="bg-gray-50 rounded-xl p-4 flex items-center justify-between text-sm text-gray-500">
        <div class="flex items-center space-x-6">
          <span>ID: {{ artwork.id }}</span>
          <span v-if="artwork.createdAt">Added: {{ formattedCreatedAt }}</span>
          <span v-if="artwork.updatedAt">Updated: {{ formattedUpdatedAt }}</span>
        </div>
        <div class="flex items-center space-x-3">
          <button @click="printArtwork" class="hover:text-gray-700">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
            </svg>
          </button>
          <button @click="shareArtwork" class="hover:text-gray-700">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Not Found State -->
    <div v-else class="bg-gray-50 rounded-xl p-12 text-center">
      <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <h3 class="text-lg font-semibold text-gray-900 mb-2">Artwork Not Found</h3>
      <p class="text-gray-600 mb-4">The artwork you're looking for doesn't exist or has been removed.</p>
      <router-link to="/artworks" class="btn btn-primary">Back to Artworks</router-link>
    </div>
  </div>
</template>

<script>
import { mapState, mapGetters, mapActions } from 'vuex';

/**
 * ArtworkDetail Component - OPTIONS API
 */
export default {
  name: 'ArtworkDetail',

  /**
   * Emits - Action events
   */
  emits: ['delete'],

  /**
   * data() - Local state
   */
  data() {
    return {
      loading: true,
      error: '',
      relatedExhibitions: []
    };
  },

  /**
   * Computed
   */
  computed: {
    // Map Vuex state
    ...mapState('artwork', ['currentArtwork']),
    ...mapGetters('artwork', ['getArtworkById']),

    /**
     * Get artwork from route param or Vuex
     */
    artwork() {
      const id = this.$route.params.id;
      return this.getArtworkById(id) || this.currentArtwork;
    },

    /**
     * Get artist display name
     */
    artistName() {
      if (!this.artwork?.artist) return 'Unknown Artist';
      
      if (typeof this.artwork.artist === 'object') {
        return this.artwork.artist.name || 'Unknown Artist';
      }
      
      return this.artwork.artist;
    },

    /**
     * Format dimensions
     */
    dimensions() {
      if (!this.artwork) return '';
      const { width, height, depth } = this.artwork;
      
      if (!width && !height) return '';
      
      let dims = `${width || '?'} × ${height || '?'}`;
      if (depth) dims += ` × ${depth}`;
      dims += ' cm';
      
      return dims;
    },

    /**
     * Status badge classes
     */
    statusBadgeClasses() {
      const base = 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium';
      const status = (this.artwork?.status || 'available').toLowerCase();

      const statusStyles = {
        'available': 'bg-green-100 text-green-800',
        'on display': 'bg-blue-100 text-blue-800',
        'on loan': 'bg-yellow-100 text-yellow-800',
        'in restoration': 'bg-orange-100 text-orange-800',
        'in storage': 'bg-gray-100 text-gray-800',
        'sold': 'bg-purple-100 text-purple-800'
      };

      return `${base} ${statusStyles[status] || statusStyles['available']}`;
    },

    /**
     * Format acquisition date
     */
    formattedAcquisitionDate() {
      if (!this.artwork?.acquisitionDate) return '';
      return new Date(this.artwork.acquisitionDate).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    },

    /**
     * Format created date
     */
    formattedCreatedAt() {
      if (!this.artwork?.createdAt) return '';
      return new Date(this.artwork.createdAt).toLocaleDateString();
    },

    /**
     * Format updated date
     */
    formattedUpdatedAt() {
      if (!this.artwork?.updatedAt) return '';
      return new Date(this.artwork.updatedAt).toLocaleDateString();
    }
  },

  /**
   * Watch - Monitor route changes
   */
  watch: {
    /**
     * Watch route params for navigation between artworks
     */
    '$route.params.id': {
      immediate: true,
      handler(newId) {
        if (newId) {
          this.fetchArtwork();
        }
      }
    }
  },

  /**
   * created - Lifecycle hook
   */
  created() {
    console.log('[ArtworkDetail] Created for ID:', this.$route.params.id);
  },

  /**
   * mounted - Lifecycle hook
   */
  mounted() {
    this.fetchArtwork();
  },

  /**
   * Methods
   */
  methods: {
    // Map Vuex actions
    ...mapActions('artwork', ['fetchArtworkById', 'deleteArtwork']),

    /**
     * Fetch artwork data
     */
    async fetchArtwork() {
      this.loading = true;
      this.error = '';

      try {
        const id = this.$route.params.id;
        await this.fetchArtworkById(id);
        
        // Fetch related exhibitions (mock for now)
        this.fetchRelatedExhibitions();
      } catch (err) {
        this.error = err.message || 'Failed to load artwork';
        console.error('[ArtworkDetail] Fetch error:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetch related exhibitions
     */
    fetchRelatedExhibitions() {
      // Mock data - would come from API
      this.relatedExhibitions = [];
    },

    /**
     * Navigate to edit page
     */
    editArtwork() {
      this.$router.push({
        name: 'artwork-edit',
        params: { id: this.artwork.id }
      });
    },

    /**
     * Confirm and delete artwork
     */
    async confirmDelete() {
      if (!confirm(`Are you sure you want to delete "${this.artwork.title}"? This action cannot be undone.`)) {
        return;
      }

      try {
        await this.deleteArtwork(this.artwork.id);
        this.$emit('delete', this.artwork);
        this.$router.push('/artworks');
      } catch (err) {
        this.error = err.message || 'Failed to delete artwork';
      }
    },

    /**
     * Handle image error
     */
    handleImageError(event) {
      event.target.style.display = 'none';
    },

    /**
     * Print artwork details
     */
    printArtwork() {
      window.print();
    },

    /**
     * Share artwork
     */
    shareArtwork() {
      if (navigator.share) {
        navigator.share({
          title: this.artwork.title,
          text: `Check out "${this.artwork.title}" by ${this.artistName}`,
          url: window.location.href
        });
      } else {
        // Fallback: copy to clipboard
        navigator.clipboard.writeText(window.location.href);
        alert('Link copied to clipboard!');
      }
    }
  }
};
</script>

<style scoped>
/* Print styles */
@media print {
  .btn {
    display: none !important;
  }
}
</style>
