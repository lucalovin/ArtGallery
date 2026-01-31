<template>
  <!--
    ExhibitionCard.vue - Exhibition Display Card Component
    Art Gallery Management System
  -->
  <div 
    class="exhibition-card bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden transition-all duration-300 hover:shadow-lg hover:border-gray-200 cursor-pointer"
    @click="viewExhibition"
  >
    <!-- Exhibition Image/Banner -->
    <div class="relative h-48 overflow-hidden bg-gradient-to-br from-primary-400 to-secondary-500">
      <img
        v-if="exhibition.imageUrl"
        :src="exhibition.imageUrl"
        :alt="exhibition.title"
        class="w-full h-full object-cover transition-transform duration-300 hover:scale-105"
        loading="lazy"
        @error="handleImageError"
      />
      <div v-else class="w-full h-full flex items-center justify-center">
        <svg class="w-16 h-16 text-white opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
                d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
        </svg>
      </div>

      <!-- Status Badge -->
      <div class="absolute top-3 left-3">
        <span :class="statusBadgeClasses">
          {{ exhibitionStatus }}
        </span>
      </div>

      <!-- Days Counter -->
      <div v-if="daysInfo" class="absolute top-3 right-3">
        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-black bg-opacity-60 text-white">
          {{ daysInfo }}
        </span>
      </div>

      <!-- Quick Actions on Hover -->
      <div class="absolute inset-0 bg-black bg-opacity-0 hover:bg-opacity-40 transition-all duration-300 flex items-center justify-center opacity-0 hover:opacity-100">
        <div class="flex space-x-2">
          <button
            type="button"
            @click.stop="viewExhibition"
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
            @click.stop="editExhibition"
            class="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
            title="Edit Exhibition"
          >
            <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>
          <button
            type="button"
            @click.stop="confirmDelete"
            class="p-2 bg-white rounded-full shadow-lg hover:bg-red-100 transition-colors"
            title="Delete Exhibition"
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
      <!-- Title -->
      <h3 class="text-lg font-semibold text-gray-900 line-clamp-1 mb-2">
        {{ exhibition.title }}
      </h3>

      <!-- Dates -->
      <div class="flex items-center text-sm text-gray-500 mb-3">
        <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <span>{{ formattedDateRange }}</span>
      </div>

      <!-- Description -->
      <p v-if="exhibition.description" class="text-gray-600 text-sm line-clamp-2 mb-4">
        {{ exhibition.description }}
      </p>

      <!-- Stats Row -->
      <div class="flex items-center justify-between pt-3 border-t border-gray-100">
        <!-- Artworks Count -->
        <div class="flex items-center text-sm">
          <svg class="w-4 h-4 mr-1 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span class="text-gray-600">{{ artworkCount }} artworks</span>
        </div>

        <!-- Visitors Count (if ongoing/past) -->
        <div v-if="exhibition.visitorCount" class="flex items-center text-sm">
          <svg class="w-4 h-4 mr-1 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          <span class="text-gray-600">{{ exhibition.visitorCount }} visitors</span>
        </div>

        <!-- Location -->
        <div v-if="exhibition.location" class="flex items-center text-sm">
          <svg class="w-4 h-4 mr-1 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span class="text-gray-600">{{ exhibition.location }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ExhibitionCard Component
 */
export default {
  name: 'ExhibitionCard',

  /**
   * Props
   */
  props: {
    exhibition: {
      type: Object,
      required: true,
      validator(value) {
        return value && typeof value.title === 'string';
      }
    }
  },

  /**
   * Emits
   */
  emits: ['view', 'edit', 'delete'],

  /**
   * data()
   */
  data() {
    return {
      imageError: false
    };
  },

  /**
   * Computed
   */
  computed: {
    /**
     * Determine exhibition status based on dates
     */
    exhibitionStatus() {
      const now = new Date();
      const startDate = new Date(this.exhibition.startDate);
      const endDate = new Date(this.exhibition.endDate);

      if (now < startDate) {
        return 'Upcoming';
      } else if (now > endDate) {
        return 'Past';
      } else {
        return 'Ongoing';
      }
    },

    /**
     * Status badge CSS classes
     */
    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      
      const statusStyles = {
        'Upcoming': 'bg-yellow-100 text-yellow-800',
        'Ongoing': 'bg-green-100 text-green-800',
        'Past': 'bg-gray-100 text-gray-800'
      };

      return `${base} ${statusStyles[this.exhibitionStatus] || statusStyles['Upcoming']}`;
    },

    /**
     * Format date range for display
     */
    formattedDateRange() {
      const options = { month: 'short', day: 'numeric', year: 'numeric' };
      const start = new Date(this.exhibition.startDate).toLocaleDateString('en-US', options);
      const end = new Date(this.exhibition.endDate).toLocaleDateString('en-US', options);
      return `${start} - ${end}`;
    },

    /**
     * Calculate days info
     */
    daysInfo() {
      const now = new Date();
      const startDate = new Date(this.exhibition.startDate);
      const endDate = new Date(this.exhibition.endDate);
      
      const diffToStart = Math.ceil((startDate - now) / (1000 * 60 * 60 * 24));
      const diffToEnd = Math.ceil((endDate - now) / (1000 * 60 * 60 * 24));

      if (this.exhibitionStatus === 'Upcoming') {
        return `Opens in ${diffToStart} days`;
      } else if (this.exhibitionStatus === 'Ongoing') {
        return `${diffToEnd} days left`;
      }
      return null;
    },

    /**
     * Get artwork count
     */
    artworkCount() {
      if (Array.isArray(this.exhibition.artworks)) {
        return this.exhibition.artworks.length;
      }
      return this.exhibition.artworkCount || 0;
    }
  },

  /**
   * Methods
   */
  methods: {
    /**
     * Navigate to exhibition detail view
     */
    viewExhibition() {
      this.$emit('view', this.exhibition);
      this.$router.push({
        name: 'ExhibitionDetail',
        params: { id: this.exhibition.id }
      });
    },

    /**
     * Navigate to edit form
     */
    editExhibition() {
      this.$emit('edit', this.exhibition);
      this.$router.push({
        name: 'exhibition-edit',
        params: { id: this.exhibition.id }
      });
    },

    /**
     * Confirm and emit delete action
     */
    confirmDelete() {
      if (confirm(`Are you sure you want to delete "${this.exhibition.title}"?`)) {
        this.$emit('delete', this.exhibition);
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
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.exhibition-card:hover {
  transform: translateY(-2px);
}
</style>
