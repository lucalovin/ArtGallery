<template>
  <!--
    ExhibitionList.vue - Exhibition List Page Component
    Art Gallery Management System
  -->
  <div class="exhibition-list">
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <h1 class="text-3xl font-display font-bold text-gray-900">Exhibitions</h1>
          <p class="text-gray-600 mt-1">
            Manage gallery exhibitions and events
          </p>
        </div>
        <router-link to="/exhibitions/new" class="btn btn-primary inline-flex items-center">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          New Exhibition
        </router-link>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ upcomingCount }}</p>
            <p class="text-sm text-gray-500">Upcoming</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ ongoingCount }}</p>
            <p class="text-sm text-gray-500">Ongoing</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ pastCount }}</p>
            <p class="text-sm text-gray-500">Past</p>
          </div>
        </div>
      </div>
      
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900">{{ totalVisitors }}</p>
            <p class="text-sm text-gray-500">Total Visitors</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Tabs and Filters -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-col lg:flex-row lg:items-center gap-4">
        <!-- Status Tabs -->
        <div class="flex border border-gray-200 rounded-lg overflow-hidden">
          <button
            v-for="tab in statusTabs"
            :key="tab.value"
            type="button"
            @click="activeTab = tab.value"
            :class="tabClasses(tab.value)"
          >
            {{ tab.label }}
            <span class="ml-2 text-xs px-2 py-0.5 rounded-full" :class="tabCountClasses(tab.value)">
              {{ getTabCount(tab.value) }}
            </span>
          </button>
        </div>

        <!-- Search -->
        <div class="flex-1">
          <div class="relative">
            <svg class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search exhibitions..."
              class="form-input pl-10 w-full"
            />
          </div>
        </div>

        <!-- Sort -->
        <div class="lg:w-48">
          <select v-model="sortBy" class="form-input w-full">
            <option value="startDate-desc">Start Date (Newest)</option>
            <option value="startDate-asc">Start Date (Oldest)</option>
            <option value="title-asc">Title (A-Z)</option>
            <option value="title-desc">Title (Z-A)</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-16">
      <div class="flex flex-col items-center space-y-4">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="text-gray-600">Loading exhibitions...</p>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredExhibitions.length === 0" class="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
      <div class="flex flex-col items-center">
        <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
        </div>
        <h3 class="text-lg font-semibold text-gray-900 mb-1">No exhibitions found</h3>
        <p class="text-gray-600 mb-4">
          {{ hasFilters ? 'Try adjusting your filters.' : 'Start by creating your first exhibition.' }}
        </p>
        <router-link to="/exhibitions/new" class="btn btn-primary">
          Create Exhibition
        </router-link>
      </div>
    </div>

    <!-- Exhibitions Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <ExhibitionCard
        v-for="exhibition in paginatedExhibitions"
        :key="exhibition.id"
        :exhibition="exhibition"
        @view="handleView"
        @edit="handleEdit"
        @delete="handleDelete"
      />
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="mt-8 flex items-center justify-between">
      <p class="text-sm text-gray-600">
        Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredExhibitions.length }} exhibitions
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
import ExhibitionCard from './ExhibitionCard.vue';

/**
 * ExhibitionList Component
 */
export default {
  name: 'ExhibitionList',

  components: {
    ExhibitionCard
  },

  data() {
    return {
      searchQuery: '',
      activeTab: 'all',
      sortBy: 'startDate-desc',
      currentPage: 1,
      itemsPerPage: 9,
      
      statusTabs: [
        { value: 'all', label: 'All' },
        { value: 'upcoming', label: 'Upcoming' },
        { value: 'ongoing', label: 'Ongoing' },
        { value: 'past', label: 'Past' }
      ]
    };
  },

  computed: {
    ...mapState('exhibition', ['exhibitions', 'loading', 'error']),
    ...mapGetters('exhibition', ['upcomingExhibitions', 'ongoingExhibitions', 'pastExhibitions']),

    upcomingCount() {
      return this.upcomingExhibitions?.length || 0;
    },

    ongoingCount() {
      return this.ongoingExhibitions?.length || 0;
    },

    pastCount() {
      return this.pastExhibitions?.length || 0;
    },

    totalVisitors() {
      return this.exhibitions.reduce((sum, e) => sum + (e.visitorCount || 0), 0);
    },

    /**
     * Filter exhibitions based on tab and search
     */
    filteredExhibitions() {
      let result = [...this.exhibitions];

      // Tab filter
      if (this.activeTab !== 'all') {
        const now = new Date();
        result = result.filter(exhibition => {
          const startDate = new Date(exhibition.startDate);
          const endDate = new Date(exhibition.endDate);
          
          switch (this.activeTab) {
            case 'upcoming':
              return now < startDate;
            case 'ongoing':
              return now >= startDate && now <= endDate;
            case 'past':
              return now > endDate;
            default:
              return true;
          }
        });
      }

      // Search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(exhibition => {
          const title = (exhibition.title || '').toLowerCase();
          const location = (exhibition.location || '').toLowerCase();
          return title.includes(query) || location.includes(query);
        });
      }

      // Sort
      result = this.sortExhibitions(result);

      return result;
    },

    paginatedExhibitions() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      return this.filteredExhibitions.slice(start, end);
    },

    totalPages() {
      return Math.ceil(this.filteredExhibitions.length / this.itemsPerPage);
    },

    paginationStart() {
      if (this.filteredExhibitions.length === 0) return 0;
      return (this.currentPage - 1) * this.itemsPerPage + 1;
    },

    paginationEnd() {
      return Math.min(this.currentPage * this.itemsPerPage, this.filteredExhibitions.length);
    },

    hasFilters() {
      return this.searchQuery || this.activeTab !== 'all';
    }
  },

  watch: {
    searchQuery() {
      this.currentPage = 1;
    },
    activeTab() {
      this.currentPage = 1;
    }
  },

  created() {
    this.loadExhibitions();
  },

  methods: {
    ...mapActions('exhibition', ['fetchExhibitions', 'deleteExhibition']),

    async loadExhibitions() {
      try {
        await this.fetchExhibitions();
      } catch (err) {
        console.error('[ExhibitionList] Load error:', err);
      }
    },

    sortExhibitions(exhibitions) {
      const [field, direction] = this.sortBy.split('-');
      
      return exhibitions.sort((a, b) => {
        let aValue, bValue;
        
        if (field === 'startDate') {
          aValue = new Date(a.startDate);
          bValue = new Date(b.startDate);
        } else {
          aValue = (a[field] || '').toLowerCase();
          bValue = (b[field] || '').toLowerCase();
        }

        if (direction === 'asc') {
          return aValue > bValue ? 1 : -1;
        } else {
          return aValue < bValue ? 1 : -1;
        }
      });
    },

    getTabCount(tab) {
      switch (tab) {
        case 'all':
          return this.exhibitions.length;
        case 'upcoming':
          return this.upcomingCount;
        case 'ongoing':
          return this.ongoingCount;
        case 'past':
          return this.pastCount;
        default:
          return 0;
      }
    },

    tabClasses(tab) {
      const base = 'px-4 py-2 text-sm font-medium transition-colors';
      if (this.activeTab === tab) {
        return `${base} bg-primary-600 text-white`;
      }
      return `${base} bg-white text-gray-600 hover:bg-gray-50`;
    },

    tabCountClasses(tab) {
      if (this.activeTab === tab) {
        return 'bg-primary-500 text-white';
      }
      return 'bg-gray-100 text-gray-600';
    },

    handleView(exhibition) {
      this.$router.push({
        name: 'ExhibitionDetail',
        params: { id: exhibition.id }
      });
    },

    handleEdit(exhibition) {
      this.$router.push({
        name: 'EditExhibition',
        params: { id: exhibition.id }
      });
    },

    async handleDelete(exhibition) {
      try {
        await this.deleteExhibition(exhibition.id);
      } catch (err) {
        console.error('[ExhibitionList] Delete error:', err);
      }
    }
  }
};
</script>
