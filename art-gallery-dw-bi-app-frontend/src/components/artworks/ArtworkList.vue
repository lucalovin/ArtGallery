<template>
  <!--
    ArtworkList.vue - Artwork List Page Component
    Art Gallery Management System
  -->
  <div class="artwork-list">
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <h1 class="text-3xl font-display font-bold text-gray-900">Artworks Collection</h1>
          <p class="text-gray-600 mt-1">
            Manage your gallery's artwork inventory
          </p>
        </div>
        <router-link to="/artworks/new" class="btn btn-primary inline-flex items-center">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Add Artwork
        </router-link>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ totalArtworks }}</p>
            <p class="text-sm text-gray-500">Total Artworks</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ availableCount }}</p>
            <p class="text-sm text-gray-500">Available</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ onDisplayCount }}</p>
            <p class="text-sm text-gray-500">On Display</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900" v-currency="totalValue"></p>
            <p class="text-sm text-gray-500">Total Value</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters Bar -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-col lg:flex-row gap-4">
        <!-- Search -->
        <div class="flex-1">
          <div class="relative">
            <svg class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search artworks by title, artist, or medium..."
              class="form-input pl-10 w-full"
            />
          </div>
        </div>

        <!-- Medium Filter -->
        <div class="lg:w-48">
          <select v-model="filterMedium" class="form-input w-full">
            <option value="">All Mediums</option>
            <option v-for="medium in mediumOptions" :key="medium" :value="medium">
              {{ medium }}
            </option>
          </select>
        </div>

        <!-- Status Filter -->
        <div class="lg:w-48">
          <select v-model="filterStatus" class="form-input w-full">
            <option value="">All Statuses</option>
            <option v-for="status in statusOptions" :key="status" :value="status">
              {{ status }}
            </option>
          </select>
        </div>

        <!-- Sort -->
        <div class="lg:w-48">
          <select v-model="sortBy" class="form-input w-full">
            <option value="title-asc">Title (A-Z)</option>
            <option value="title-desc">Title (Z-A)</option>
            <option value="year-desc">Newest First</option>
            <option value="year-asc">Oldest First</option>
            <option value="value-desc">Highest Value</option>
            <option value="value-asc">Lowest Value</option>
          </select>
        </div>

        <!-- View Toggle -->
        <div class="flex items-center space-x-2">
          <button
            type="button"
            @click="viewMode = 'grid'"
            :class="viewMode === 'grid' ? 'bg-primary-100 text-primary-600' : 'bg-gray-100 text-gray-600'"
            class="p-2 rounded-lg transition-colors"
            title="Grid View"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
            </svg>
          </button>
          <button
            type="button"
            @click="viewMode = 'list'"
            :class="viewMode === 'list' ? 'bg-primary-100 text-primary-600' : 'bg-gray-100 text-gray-600'"
            class="p-2 rounded-lg transition-colors"
            title="List View"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-16">
      <div class="flex flex-col items-center space-y-4">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="text-gray-600">Loading artworks...</p>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredArtworks.length === 0" class="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
      <div class="flex flex-col items-center">
        <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
        <h3 class="text-lg font-semibold text-gray-900 mb-1">No artworks found</h3>
        <p class="text-gray-600 mb-4">
          {{ hasFilters ? 'Try adjusting your search or filters.' : 'Start by adding your first artwork.' }}
        </p>
        <router-link to="/artworks/new" class="btn btn-primary">
          Add Artwork
        </router-link>
      </div>
    </div>

    <!-- Artworks Grid -->
    <div v-else :class="gridClasses">
      <ArtworkCard
        v-for="artwork in paginatedArtworks"
        :key="artwork.id"
        :artwork="artwork"
        @view="handleView"
        @edit="handleEdit"
        @delete="handleDelete"
      />
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="mt-8 flex items-center justify-between">
      <p class="text-sm text-gray-600">
        Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredArtworks.length }} artworks
      </p>
      
      <div class="flex items-center space-x-2">
        <button
          type="button"
          @click="currentPage--"
          :disabled="currentPage === 1"
          class="btn btn-secondary btn-sm"
          :class="{ 'opacity-50 cursor-not-allowed': currentPage === 1 }"
        >
          Previous
        </button>
        
        <span class="text-sm text-gray-600">
          Page {{ currentPage }} of {{ totalPages }}
        </span>
        
        <button
          type="button"
          @click="currentPage++"
          :disabled="currentPage === totalPages"
          class="btn btn-secondary btn-sm"
          :class="{ 'opacity-50 cursor-not-allowed': currentPage === totalPages }"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapGetters, mapActions } from 'vuex';
import ArtworkCard from './ArtworkCard.vue';

/**
 * ArtworkList Component - OPTIONS API
 */
export default {
  name: 'ArtworkList',

  /**
   * Components - Local registration
   */
  components: {
    ArtworkCard
  },

  /**
   * data() - Local state
   */
  data() {
    return {
      // Search and filters
      searchQuery: '',
      filterMedium: '',
      filterStatus: '',
      sortBy: 'title-asc',
      
      // View options
      viewMode: 'grid',
      
      // Pagination
      currentPage: 1,
      itemsPerPage: 12,
      
      // Filter options
      mediumOptions: [
        'Oil on Canvas',
        'Acrylic on Canvas',
        'Watercolor',
        'Mixed Media',
        'Photography',
        'Sculpture - Bronze',
        'Sculpture - Marble',
        'Digital Art'
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
   * Computed
   */
  computed: {
    // Map Vuex state and getters
    ...mapState('artwork', ['artworks', 'loading', 'error']),
    ...mapGetters('artwork', ['totalArtworks', 'totalValue']),

    /**
     * Filter artworks based on search and filters
     */
    filteredArtworks() {
      let result = [...this.artworks];

      // Search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(artwork => {
          const title = (artwork.title || '').toLowerCase();
          const artist = typeof artwork.artist === 'object' 
            ? (artwork.artist.name || '').toLowerCase()
            : (artwork.artist || '').toLowerCase();
          const medium = (artwork.medium || '').toLowerCase();
          
          return title.includes(query) || artist.includes(query) || medium.includes(query);
        });
      }

      // Medium filter
      if (this.filterMedium) {
        result = result.filter(artwork => artwork.medium === this.filterMedium);
      }

      // Status filter
      if (this.filterStatus) {
        result = result.filter(artwork => artwork.status === this.filterStatus);
      }

      // Sorting
      result = this.sortArtworks(result);

      return result;
    },

    /**
     * Get paginated artworks
     */
    paginatedArtworks() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      return this.filteredArtworks.slice(start, end);
    },

    /**
     * Calculate total pages
     */
    totalPages() {
      return Math.ceil(this.filteredArtworks.length / this.itemsPerPage);
    },

    /**
     * Pagination display values
     */
    paginationStart() {
      if (this.filteredArtworks.length === 0) return 0;
      return (this.currentPage - 1) * this.itemsPerPage + 1;
    },

    paginationEnd() {
      const end = this.currentPage * this.itemsPerPage;
      return Math.min(end, this.filteredArtworks.length);
    },

    /**
     * Count artworks by status
     */
    availableCount() {
      return this.artworks.filter(a => a.status === 'Available').length;
    },

    onDisplayCount() {
      return this.artworks.filter(a => a.status === 'On Display').length;
    },

    /**
     * Check if any filters are active
     */
    hasFilters() {
      return this.searchQuery || this.filterMedium || this.filterStatus;
    },

    /**
     * Grid CSS classes based on view mode
     */
    gridClasses() {
      if (this.viewMode === 'list') {
        return 'space-y-4';
      }
      return 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6';
    }
  },

  /**
   * Watch - Monitor filter changes
   */
  watch: {
    /**
     * Reset to first page when filters change
     */
    searchQuery() {
      this.currentPage = 1;
    },
    filterMedium() {
      this.currentPage = 1;
    },
    filterStatus() {
      this.currentPage = 1;
    }
  },

  /**
   * created - Lifecycle hook
   */
  created() {
    console.log('[ArtworkList] Created');
    this.loadArtworks();
  },

  /**
   * Methods
   */
  methods: {
    // Map Vuex actions
    ...mapActions('artwork', ['fetchArtworks', 'deleteArtwork']),

    /**
     * Load artworks from store
     */
    async loadArtworks() {
      try {
        await this.fetchArtworks();
      } catch (err) {
        console.error('[ArtworkList] Load error:', err);
      }
    },

    /**
     * Sort artworks based on selected option
     */
    sortArtworks(artworks) {
      const [field, direction] = this.sortBy.split('-');
      
      return artworks.sort((a, b) => {
        let aValue, bValue;
        
        switch (field) {
          case 'title':
            aValue = (a.title || '').toLowerCase();
            bValue = (b.title || '').toLowerCase();
            break;
          case 'year':
            aValue = a.year || 0;
            bValue = b.year || 0;
            break;
          case 'value':
            aValue = a.estimatedValue || 0;
            bValue = b.estimatedValue || 0;
            break;
          default:
            aValue = a[field];
            bValue = b[field];
        }

        if (direction === 'asc') {
          return aValue > bValue ? 1 : -1;
        } else {
          return aValue < bValue ? 1 : -1;
        }
      });
    },

    /**
     * Handle view artwork action
     */
    handleView(artwork) {
      this.$router.push({
        name: 'artwork-detail',
        params: { id: artwork.id }
      });
    },

    /**
     * Handle edit artwork action
     */
    handleEdit(artwork) {
      this.$router.push({
        name: 'EditArtwork',
        params: { id: artwork.id }
      });
    },

    /**
     * Handle delete artwork action
     */
    async handleDelete(artwork) {
      try {
        await this.deleteArtwork(artwork.id);
      } catch (err) {
        console.error('[ArtworkList] Delete error:', err);
      }
    }
  }
};
</script>

<style scoped>
/* List view specific styles */
.space-y-4 > * {
  width: 100%;
}
</style>
