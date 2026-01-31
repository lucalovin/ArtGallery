<template>
  <!--
    ArtworkCard.vue - Artwork Display Card Component
    Art Gallery Management System
  -->
  <div 
    class="artwork-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg hover:border-gray-200 cursor-pointer"
    @click="viewArtwork"
  >
    <!-- Artwork Image -->
    <div class="relative h-56 overflow-hidden bg-gray-100">
      <img
        v-if="artwork.imageUrl"
        :src="artwork.imageUrl"
        :alt="artwork.title"
        class="w-full h-full object-cover transition-transform duration-300 hover:scale-105"
        loading="lazy"
        @error="handleImageError"
      />
      <div v-else class="w-full h-full flex items-center justify-center">
        <svg class="w-16 h-16 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      </div>

      <!-- Status Badge -->
      <div class="absolute top-3 left-3">
        <span :class="statusBadgeClasses">
          {{ artwork.status || 'Available' }}
        </span>
      </div>

      <!-- Quick Actions on Hover -->
      <div class="absolute inset-0 bg-black bg-opacity-0 hover:bg-opacity-40 transition-all duration-300 flex items-center justify-center opacity-0 hover:opacity-100">
        <div class="flex space-x-2">
          <button
            type="button"
            @click.stop="viewArtwork"
            class="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
            title="View Details"
          >
            <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
            </svg>
          </button>
          <button
            type="button"
            @click.stop="editArtwork"
            class="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
            title="Edit Artwork"
          >
            <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>
          <button
            type="button"
            @click.stop="confirmDelete"
            class="p-2 bg-white rounded-full shadow-lg hover:bg-red-100 transition-colors"
            title="Delete Artwork"
          >
            <svg class="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Content Section -->
    <div class="p-4">
      <!-- Category -->
      <div class="mb-2">
        <span class="text-xs font-medium text-primary-600 uppercase tracking-wider">
          {{ artwork.medium || 'Unknown Medium' }}
        </span>
      </div>

      <!-- Title -->
      <h3 class="text-lg font-semibold text-gray-900 line-clamp-1 mb-1">
        {{ artwork.title }}
      </h3>

      <!-- Artist -->
      <p class="text-gray-600 text-sm mb-2">
        by <span class="font-medium">{{ artistName }}</span>
      </p>

      <!-- Year and Dimensions -->
      <div class="flex items-center text-sm text-gray-500 space-x-4 mb-3">
        <span v-if="artwork.year" class="flex items-center">
          <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          {{ artwork.year }}
        </span>
        <span v-if="dimensions" class="flex items-center">
          <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
          </svg>
          {{ dimensions }}
        </span>
      </div>

      <!-- Price/Value -->
      <div class="flex items-center justify-between pt-3 border-t border-gray-100">
        <div>
          <span class="text-xs text-gray-500">Estimated Value</span>
          <p class="text-lg font-bold text-gray-900" v-currency="artwork.estimatedValue"></p>
        </div>
        
        <!-- Location Badge -->
        <div v-if="artwork.location" class="text-right">
          <span class="text-xs text-gray-500">Location</span>
          <p class="text-sm font-medium text-gray-700">{{ artwork.location }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ArtworkCard',

  /**
   * Props - Input properties with validation
   */
  props: {
    artwork: {
      type: Object,
      required: true,
      validator(value) {
        // Validate required artwork properties
        return value && typeof value.title === 'string';
      }
    }
  },

  /**
   * Emits - Custom event declarations
   */
  emits: ['view', 'edit', 'delete'],

  /**
   * data() - Local reactive state
   */
  data() {
    return {
      imageError: false
    };
  },

  /**
   * Computed properties
   * Derived values that update automatically
   */
  computed: {
    /**
     * Get artist display name
     * Handles both object and string artist data
     */
    artistName() {
      if (!this.artwork.artist) return 'Unknown Artist';
      
      if (typeof this.artwork.artist === 'object') {
        return this.artwork.artist.name || 'Unknown Artist';
      }
      
      return this.artwork.artist;
    },

    /**
     * Format dimensions string
     */
    dimensions() {
      const { width, height, depth } = this.artwork;
      
      if (!width && !height) return '';
      
      let dims = `${width || '?'} × ${height || '?'}`;
      if (depth) dims += ` × ${depth}`;
      dims += ' cm';
      
      return dims;
    },

    /**
     * Status badge CSS classes
     */
    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      const status = (this.artwork.status || 'available').toLowerCase();

      const statusStyles = {
        'available': 'bg-green-100 text-green-800',
        'on display': 'bg-blue-100 text-blue-800',
        'on loan': 'bg-yellow-100 text-yellow-800',
        'in restoration': 'bg-orange-100 text-orange-800',
        'in storage': 'bg-gray-100 text-gray-800',
        'sold': 'bg-purple-100 text-purple-800'
      };

      return `${base} ${statusStyles[status] || statusStyles['available']}`;
    }
  },

  /**
   * Methods - Event handlers
   */
  methods: {
    /**
     * Navigate to artwork detail view
     */
    viewArtwork() {
      this.$emit('view', this.artwork);
      this.$router.push({
        name: 'ArtworkDetail',
        params: { id: this.artwork.id }
      });
    },

    /**
     * Navigate to edit form
     */
    editArtwork() {
      this.$emit('edit', this.artwork);
      this.$router.push({
        name: 'EditArtwork',
        params: { id: this.artwork.id }
      });
    },

    /**
     * Confirm and emit delete action
     */
    confirmDelete() {
      if (confirm(`Are you sure you want to delete "${this.artwork.title}"?`)) {
        this.$emit('delete', this.artwork);
      }
    },

    /**
     * Handle image loading error
     */
    handleImageError(event) {
      this.imageError = true;
      event.target.style.display = 'none';
    }
  }
};
</script>

<style scoped>
/* Line clamp for title */
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Card hover effects */
.artwork-card:hover {
  transform: translateY(-2px);
}
</style>
