<template>
  <!--
    ArtworkInventory.vue - Artwork List Page
    Art Gallery Management System
  -->
  <div class="artwork-inventory-page">
    <!-- Page Header -->
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Artwork Inventory</h1>
        <p class="text-gray-500 mt-1">Manage your gallery's artwork collection</p>
      </div>
      <div class="mt-4 md:mt-0 flex items-center space-x-3">
        <button
          @click="toggleViewMode"
          class="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          :title="viewMode === 'grid' ? 'Switch to list view' : 'Switch to grid view'"
        >
          {{ viewMode === 'grid' ? '📋' : '🔲' }}
        </button>
        <router-link
          to="/artworks/new"
          class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
        >
          <span class="mr-2">➕</span>
          Add Artwork
        </router-link>
      </div>
    </header>

    <!-- Filters Section -->
    <section class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Search -->
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search artworks..."
            class="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
          <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400">
            🔍
          </span>
        </div>

        <!-- Category Filter -->
        <select
          v-model="selectedCategory"
          class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="">All Categories</option>
          <option v-for="category in categories" :key="category" :value="category">
            {{ category }}
          </option>
        </select>

        <!-- Status Filter -->
        <select
          v-model="selectedStatus"
          class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="">All Statuses</option>
          <option v-for="status in statuses" :key="status" :value="status">
            {{ status }}
          </option>
        </select>

        <!-- Sort -->
        <select
          v-model="sortBy"
          class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="title">Sort by Title</option>
          <option value="artist">Sort by Artist</option>
          <option value="year">Sort by Year</option>
          <option value="dateAdded">Sort by Date Added</option>
        </select>
      </div>

      <!-- Active Filters -->
      <div v-if="hasActiveFilters" class="mt-4 flex items-center flex-wrap gap-2">
        <span class="text-sm text-gray-500">Active filters:</span>
        <span 
          v-if="searchQuery"
          class="inline-flex items-center px-3 py-1 bg-primary-100 text-primary-700 rounded-full text-sm"
        >
          Search: {{ searchQuery }}
          <button @click="searchQuery = ''" class="ml-2 hover:text-primary-900">×</button>
        </span>
        <span 
          v-if="selectedCategory"
          class="inline-flex items-center px-3 py-1 bg-primary-100 text-primary-700 rounded-full text-sm"
        >
          {{ selectedCategory }}
          <button @click="selectedCategory = ''" class="ml-2 hover:text-primary-900">×</button>
        </span>
        <span 
          v-if="selectedStatus"
          class="inline-flex items-center px-3 py-1 bg-primary-100 text-primary-700 rounded-full text-sm"
        >
          {{ selectedStatus }}
          <button @click="selectedStatus = ''" class="ml-2 hover:text-primary-900">×</button>
        </span>
        <button 
          @click="clearFilters"
          class="text-sm text-gray-500 hover:text-gray-700 underline"
        >
          Clear all
        </button>
      </div>
    </section>

    <!-- Results Summary -->
    <div class="flex items-center justify-between mb-4">
      <p class="text-sm text-gray-500">
        Showing {{ filteredArtworks.length }} of {{ artworks.length }} artworks
      </p>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div 
        v-for="n in 8" 
        :key="n" 
        class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden animate-pulse"
      >
        <div class="h-48 bg-gray-200"></div>
        <div class="p-4 space-y-3">
          <div class="h-4 bg-gray-200 rounded w-3/4"></div>
          <div class="h-3 bg-gray-200 rounded w-1/2"></div>
          <div class="h-3 bg-gray-200 rounded w-2/3"></div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else-if="filteredArtworks.length === 0"
      class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-100"
    >
      <span class="text-6xl mb-4 block">🖼️</span>
      <h3 class="text-xl font-semibold text-gray-800 mb-2">No Artworks Found</h3>
      <p class="text-gray-500 mb-6">
        {{ hasActiveFilters ? 'Try adjusting your filters' : 'Start by adding your first artwork' }}
      </p>
      <router-link
        v-if="!hasActiveFilters"
        to="/artworks/new"
        class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
      >
        Add First Artwork
      </router-link>
      <button
        v-else
        @click="clearFilters"
        class="inline-flex items-center px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
      >
        Clear Filters
      </button>
    </div>

    <!-- Grid View -->
    <div 
      v-else-if="viewMode === 'grid'"
      class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
    >
      <artwork-card
        v-for="artwork in paginatedArtworks"
        :key="artwork.id"
        :artwork="artwork"
        @view="viewArtwork"
        @edit="editArtwork"
        @delete="confirmDelete"
      />
    </div>

    <!-- List View -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Artwork</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Artist</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Year</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Category</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
            <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr 
            v-for="artwork in paginatedArtworks" 
            :key="artwork.id"
            class="hover:bg-gray-50 transition-colors"
          >
            <td class="px-6 py-4">
              <div class="flex items-center space-x-3">
                <div class="w-12 h-12 bg-gray-200 rounded-lg overflow-hidden flex-shrink-0">
                  <img 
                    v-if="artwork.imageUrl" 
                    :src="artwork.imageUrl" 
                    :alt="artwork.title"
                    class="w-full h-full object-cover"
                  />
                  <div v-else class="w-full h-full flex items-center justify-center">🖼️</div>
                </div>
                <span class="font-medium text-gray-900">{{ artwork.title }}</span>
              </div>
            </td>
            <td class="px-6 py-4 text-gray-600">{{ artwork.artist }}</td>
            <td class="px-6 py-4 text-gray-600">{{ artwork.year }}</td>
            <td class="px-6 py-4 text-gray-600">{{ artwork.category }}</td>
            <td class="px-6 py-4">
              <span 
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="getStatusClass(artwork.status)"
              >
                {{ artwork.status }}
              </span>
            </td>
            <td class="px-6 py-4 text-right">
              <div class="flex items-center justify-end space-x-2">
                <button
                  @click="viewArtwork(artwork)"
                  class="p-1.5 text-gray-400 hover:text-blue-600 transition-colors"
                  title="View"
                >
                  👁️
                </button>
                <button
                  @click="editArtwork(artwork)"
                  class="p-1.5 text-gray-400 hover:text-primary-600 transition-colors"
                  title="Edit"
                >
                  ✏️
                </button>
                <button
                  @click="confirmDelete(artwork)"
                  class="p-1.5 text-gray-400 hover:text-red-600 transition-colors"
                  title="Delete"
                >
                  🗑️
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div 
      v-if="totalPages > 1"
      class="mt-6 flex items-center justify-center space-x-2"
    >
      <button
        :disabled="currentPage === 1"
        @click="currentPage--"
        class="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        Previous
      </button>

      <div class="flex items-center space-x-1">
        <button
          v-for="page in visiblePages"
          :key="page"
          @click="currentPage = page"
          class="w-10 h-10 text-sm font-medium rounded-lg transition-colors"
          :class="currentPage === page 
            ? 'bg-primary-600 text-white' 
            : 'text-gray-600 hover:bg-gray-100'"
        >
          {{ page }}
        </button>
      </div>

      <button
        :disabled="currentPage === totalPages"
        @click="currentPage++"
        class="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        Next
      </button>
    </div>

    <!-- Delete Confirmation Modal -->
    <div 
      v-if="showDeleteModal"
      class="fixed inset-0 z-50 overflow-y-auto"
    >
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="showDeleteModal = false"></div>
        
        <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6 z-10">
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Delete Artwork</h3>
          <p class="text-gray-500 mb-6">
            Are you sure you want to delete "{{ artworkToDelete?.title }}"? This action cannot be undone.
          </p>
          <div class="flex justify-end space-x-3">
            <button
              @click="showDeleteModal = false"
              class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors font-medium"
            >
              Cancel
            </button>
            <button
              @click="deleteArtwork"
              class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import ArtworkCard from '@/components/artworks/ArtworkCard.vue';

/**
 * ArtworkInventory Page Component
 * Displays artwork collection with filtering, sorting, and pagination
 */
export default {
  name: 'ArtworkInventoryPage',

  components: {
    'artwork-card': ArtworkCard
  },

  data() {
    return {
      isLoading: true,
      viewMode: 'grid',
      searchQuery: '',
      selectedCategory: '',
      selectedStatus: '',
      sortBy: 'title',
      currentPage: 1,
      pageSize: 12,
      showDeleteModal: false,
      artworkToDelete: null,
      categories: ['Painting', 'Sculpture', 'Photography', 'Drawing', 'Print', 'Mixed Media', 'Digital Art'],
      statuses: ['On Display', 'In Storage', 'On Loan', 'Under Restoration'],
      // Mock data - replace with Vuex store
      artworks: []
    };
  },

  computed: {
    ...mapState({
      storeArtworks: state => state.artwork?.artworks || []
    }),

    filteredArtworks() {
      let result = [...this.artworks];

      // Search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(artwork => 
          artwork.title.toLowerCase().includes(query) ||
          artwork.artist.toLowerCase().includes(query) ||
          artwork.description?.toLowerCase().includes(query)
        );
      }

      // Category filter
      if (this.selectedCategory) {
        result = result.filter(artwork => artwork.category === this.selectedCategory);
      }

      // Status filter
      if (this.selectedStatus) {
        result = result.filter(artwork => artwork.status === this.selectedStatus);
      }

      // Sort
      result.sort((a, b) => {
        const aVal = a[this.sortBy];
        const bVal = b[this.sortBy];
        if (aVal < bVal) return -1;
        if (aVal > bVal) return 1;
        return 0;
      });

      return result;
    },

    paginatedArtworks() {
      const start = (this.currentPage - 1) * this.pageSize;
      return this.filteredArtworks.slice(start, start + this.pageSize);
    },

    totalPages() {
      return Math.ceil(this.filteredArtworks.length / this.pageSize);
    },

    visiblePages() {
      const pages = [];
      const maxVisible = 5;
      let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
      let end = Math.min(this.totalPages, start + maxVisible - 1);

      if (end - start + 1 < maxVisible) {
        start = Math.max(1, end - maxVisible + 1);
      }

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      return pages;
    },

    hasActiveFilters() {
      return this.searchQuery || this.selectedCategory || this.selectedStatus;
    }
  },

  watch: {
    searchQuery() {
      this.currentPage = 1;
    },
    selectedCategory() {
      this.currentPage = 1;
    },
    selectedStatus() {
      this.currentPage = 1;
    }
  },

  created() {
    this.loadArtworks();
  },

  methods: {
    ...mapActions({
      fetchArtworks: 'artwork/fetchArtworks',
      removeArtwork: 'artwork/deleteArtwork'
    }),

    async loadArtworks() {
      this.isLoading = true;
      
      try {
        // Simulate API call - replace with actual Vuex action
        await new Promise(resolve => setTimeout(resolve, 600));
        
        // Mock data
        this.artworks = [
          { id: 1, title: 'Starry Night', artist: 'Vincent van Gogh', year: 1889, category: 'Painting', status: 'On Display', imageUrl: '' },
          { id: 2, title: 'The Persistence of Memory', artist: 'Salvador Dalí', year: 1931, category: 'Painting', status: 'In Storage', imageUrl: '' },
          { id: 3, title: 'Girl with a Pearl Earring', artist: 'Johannes Vermeer', year: 1665, category: 'Painting', status: 'On Loan', imageUrl: '' },
          { id: 4, title: 'The Birth of Venus', artist: 'Sandro Botticelli', year: 1485, category: 'Painting', status: 'On Display', imageUrl: '' },
          { id: 5, title: 'The Thinker', artist: 'Auguste Rodin', year: 1904, category: 'Sculpture', status: 'On Display', imageUrl: '' },
          { id: 6, title: 'David', artist: 'Michelangelo', year: 1504, category: 'Sculpture', status: 'On Loan', imageUrl: '' },
          { id: 7, title: 'Migrant Mother', artist: 'Dorothea Lange', year: 1936, category: 'Photography', status: 'In Storage', imageUrl: '' },
          { id: 8, title: 'Water Lilies', artist: 'Claude Monet', year: 1906, category: 'Painting', status: 'Under Restoration', imageUrl: '' }
        ];
      } catch (error) {
        console.error('Error loading artworks:', error);
      } finally {
        this.isLoading = false;
      }
    },

    toggleViewMode() {
      this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
    },

    clearFilters() {
      this.searchQuery = '';
      this.selectedCategory = '';
      this.selectedStatus = '';
    },

    viewArtwork(artwork) {
      this.$router.push({ name: 'ArtworkDetail', params: { id: artwork.id } });
    },

    editArtwork(artwork) {
      this.$router.push({ name: 'EditArtwork', params: { id: artwork.id } });
    },

    confirmDelete(artwork) {
      this.artworkToDelete = artwork;
      this.showDeleteModal = true;
    },

    async deleteArtwork() {
      try {
        // Call Vuex action to delete
        // await this.removeArtwork(this.artworkToDelete.id);
        
        // For now, remove from local array
        this.artworks = this.artworks.filter(a => a.id !== this.artworkToDelete.id);
        this.showDeleteModal = false;
        this.artworkToDelete = null;
      } catch (error) {
        console.error('Error deleting artwork:', error);
      }
    },

    getStatusClass(status) {
      const statusClasses = {
        'On Display': 'bg-green-100 text-green-700',
        'In Storage': 'bg-gray-100 text-gray-700',
        'On Loan': 'bg-blue-100 text-blue-700',
        'Under Restoration': 'bg-yellow-100 text-yellow-700'
      };
      return statusClasses[status] || 'bg-gray-100 text-gray-700';
    }
  }
};
</script>

<style scoped>
.artwork-inventory-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
