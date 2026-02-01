<template>
  <!--
    ExhibitionInventory.vue - Exhibition List Page
    Art Gallery Management System
  -->
  <div class="exhibition-inventory-page">
    <!-- Page Header -->
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Exhibitions</h1>
        <p class="text-gray-500 mt-1">Manage gallery exhibitions and events</p>
      </div>
      <div class="mt-4 md:mt-0">
        <router-link
          to="/exhibitions/new"
          class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
        >
          <span class="mr-2">➕</span>
          New Exhibition
        </router-link>
      </div>
    </header>

    <!-- Status Tabs -->
    <div class="mb-6">
      <nav class="flex space-x-2 bg-gray-100 rounded-lg p-1">
        <button
          v-for="tab in statusTabs"
          :key="tab.value"
          @click="activeStatus = tab.value"
          class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
          :class="activeStatus === tab.value 
            ? 'bg-white text-primary-600 shadow-sm' 
            : 'text-gray-600 hover:text-gray-800'"
        >
          {{ tab.label }}
          <span 
            class="ml-1 px-2 py-0.5 text-xs rounded-full"
            :class="activeStatus === tab.value ? 'bg-primary-100 text-primary-600' : 'bg-gray-200 text-gray-500'"
          >
            {{ getStatusCount(tab.value) }}
          </span>
        </button>
      </nav>
    </div>

    <!-- Search -->
    <div class="mb-6">
      <div class="relative max-w-md">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search exhibitions..."
          class="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        />
        <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400">
          🔍
        </span>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="n in 6" 
        :key="n" 
        class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden animate-pulse"
      >
        <div class="h-40 bg-gray-200"></div>
        <div class="p-4 space-y-3">
          <div class="h-4 bg-gray-200 rounded w-3/4"></div>
          <div class="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else-if="filteredExhibitions.length === 0"
      class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-100"
    >
      <span class="text-6xl mb-4 block">🎨</span>
      <h3 class="text-xl font-semibold text-gray-800 mb-2">No Exhibitions Found</h3>
      <p class="text-gray-500 mb-6">
        {{ searchQuery ? 'Try a different search term' : 'Create your first exhibition' }}
      </p>
      <router-link
        v-if="!searchQuery"
        to="/exhibitions/new"
        class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
      >
        Create Exhibition
      </router-link>
    </div>

    <!-- Exhibition Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <exhibition-card
        v-for="exhibition in filteredExhibitions"
        :key="exhibition.id"
        :exhibition="exhibition"
        @view="viewExhibition"
        @edit="editExhibition"
        @delete="confirmDelete"
      />
    </div>

    <!-- Delete Modal -->
    <div 
      v-if="showDeleteModal"
      class="fixed inset-0 z-50 overflow-y-auto"
    >
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-black bg-opacity-50" @click="showDeleteModal = false"></div>
        <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6 z-10">
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Delete Exhibition</h3>
          <p class="text-gray-500 mb-6">
            Are you sure you want to delete "{{ exhibitionToDelete?.title }}"?
          </p>
          <div class="flex justify-end space-x-3">
            <button
              @click="showDeleteModal = false"
              class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 font-medium"
            >
              Cancel
            </button>
            <button
              @click="deleteExhibition"
              class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 font-medium"
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
import ExhibitionCard from '@/components/exhibitions/ExhibitionCard.vue';

/**
 * ExhibitionInventory Page
 */
export default {
  name: 'ExhibitionInventoryPage',

  components: {
    'exhibition-card': ExhibitionCard
  },

  data() {
    return {
      isLoading: true,
      searchQuery: '',
      activeStatus: 'all',
      showDeleteModal: false,
      exhibitionToDelete: null,
      statusTabs: [
        { label: 'All', value: 'all' },
        { label: 'Current', value: 'current' },
        { label: 'Upcoming', value: 'upcoming' },
        { label: 'Past', value: 'past' }
      ]
    };
  },

  computed: {
    ...mapState({
      exhibitions: state => state.exhibition?.exhibitions || []
    }),

    filteredExhibitions() {
      let result = [...this.exhibitions];

      // Status filter - calculate status based on dates
      if (this.activeStatus !== 'all') {
        result = result.filter(e => this.getExhibitionStatus(e) === this.activeStatus);
      }

      // Search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(e => 
          e.title.toLowerCase().includes(query) ||
          e.description?.toLowerCase().includes(query)
        );
      }

      return result;
    }
  },

  created() {
    this.loadExhibitions();
  },

  methods: {
    ...mapActions({
      fetchExhibitions: 'exhibition/fetchExhibitions',
      deleteExhibitionAction: 'exhibition/deleteExhibition'
    }),

    /**
     * Calculate exhibition status based on start and end dates
     * @param {Object} exhibition - Exhibition object with startDate and endDate
     * @returns {string} 'current', 'upcoming', or 'past'
     */
    getExhibitionStatus(exhibition) {
      const now = new Date();
      const startDate = new Date(exhibition.startDate);
      const endDate = new Date(exhibition.endDate);
      
      if (now < startDate) {
        return 'upcoming';
      } else if (now > endDate) {
        return 'past';
      } else {
        return 'current';
      }
    },

    async loadExhibitions() {
      this.isLoading = true;
      try {
        await this.fetchExhibitions();
      } catch (error) {
        console.error('Failed to load exhibitions:', error);
      } finally {
        this.isLoading = false;
      }
    },

    getStatusCount(status) {
      if (status === 'all') return this.exhibitions.length;
      return this.exhibitions.filter(e => this.getExhibitionStatus(e) === status).length;
    },

    viewExhibition(exhibition) {
      this.$router.push({ name: 'ExhibitionDetail', params: { id: exhibition.id } });
    },

    editExhibition(exhibition) {
      this.$router.push({ name: 'EditExhibition', params: { id: exhibition.id } });
    },

    confirmDelete(exhibition) {
      this.exhibitionToDelete = exhibition;
      this.showDeleteModal = true;
    },

    async deleteExhibition() {
      try {
        await this.deleteExhibitionAction(this.exhibitionToDelete.id);
        this.showDeleteModal = false;
        this.exhibitionToDelete = null;
      } catch (error) {
        console.error('Failed to delete exhibition:', error);
        alert('Failed to delete exhibition. Please try again.');
      }
    }
  }
};
</script>

<style scoped>
.exhibition-inventory-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
