<template>
  <!--
    ListComponent.vue - Reusable List Component with Filtering/Sorting
    Art Gallery Management System
  -->
  <div class="list-component">
    <!-- List Header -->
    <div v-if="showHeader" class="list-header mb-6">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <!-- Title and Count -->
        <div>
          <h2 v-if="title" class="text-2xl font-display font-bold text-gray-900">
            {{ title }}
          </h2>
          <p class="text-gray-600 text-sm mt-1">
            Showing {{ filteredItems.length }} of {{ items.length }} items
            <span v-if="searchQuery"> matching "{{ searchQuery }}"</span>
          </p>
        </div>

        <!-- Header Actions Slot -->
        <slot name="header-actions">
          <router-link 
            v-if="addRoute"
            :to="addRoute"
            class="btn btn-primary inline-flex items-center space-x-2"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span>{{ addButtonText }}</span>
          </router-link>
        </slot>
      </div>
    </div>

    <!-- Search and Filters Bar -->
    <div v-if="showFilters" class="filters-bar bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-col lg:flex-row gap-4">
        
        <!-- Search Input -->
        <div v-if="showSearch" class="flex-1">
          <div class="relative">
            <svg 
              class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400"
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="searchQuery"
              type="text"
              :placeholder="searchPlaceholder"
              class="form-input pl-10 w-full"
              @input="handleSearchInput"
            />
            <button
              v-if="searchQuery"
              type="button"
              @click="clearSearch"
              class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        <!-- Filter Dropdown -->
        <div v-if="filterOptions.length > 0" class="lg:w-48">
          <select
            v-model="selectedFilter"
            class="form-input w-full"
            @change="handleFilterChange"
          >
            <option value="">{{ filterPlaceholder }}</option>
            <option 
              v-for="option in filterOptions" 
              :key="option.value"
              :value="option.value"
            >
              {{ option.label }}
            </option>
          </select>
        </div>

        <!-- Sort Dropdown -->
        <div v-if="sortOptions.length > 0" class="lg:w-48">
          <select
            v-model="selectedSort"
            class="form-input w-full"
            @change="handleSortChange"
          >
            <option 
              v-for="option in sortOptions" 
              :key="option.value"
              :value="option.value"
            >
              {{ option.label }}
            </option>
          </select>
        </div>

        <!-- View Toggle -->
        <div v-if="showViewToggle" class="flex items-center space-x-2">
          <button
            type="button"
            @click="viewMode = 'grid'"
            :class="viewButtonClasses('grid')"
            title="Grid View"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
            </svg>
          </button>
          <button
            type="button"
            @click="viewMode = 'list'"
            :class="viewButtonClasses('list')"
            title="List View"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M4 6h16M4 10h16M4 14h16M4 18h16" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Active Filters Tags -->
      <div v-if="hasActiveFilters" class="flex flex-wrap gap-2 mt-4 pt-4 border-t border-gray-100">
        <span class="text-sm text-gray-500">Active filters:</span>
        <span 
          v-if="searchQuery"
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800"
        >
          Search: {{ searchQuery }}
          <button @click="clearSearch" class="ml-1 hover:text-primary-900">
            <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </span>
        <span 
          v-if="selectedFilter"
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-secondary-100 text-secondary-800"
        >
          {{ getFilterLabel(selectedFilter) }}
          <button @click="clearFilter" class="ml-1 hover:text-secondary-900">
            <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </span>
        <button 
          @click="clearAllFilters"
          class="text-xs text-gray-500 hover:text-gray-700 underline"
        >
          Clear all
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-16">
      <div class="flex flex-col items-center space-y-4">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="text-gray-600">{{ loadingText }}</p>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else-if="filteredItems.length === 0" 
      class="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center"
    >
      <slot name="empty">
        <div class="flex flex-col items-center">
          <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
            <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
            </svg>
          </div>
          <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ emptyTitle }}</h3>
          <p class="text-gray-600 mb-4">{{ emptyMessage }}</p>
          <router-link 
            v-if="addRoute"
            :to="addRoute"
            class="btn btn-primary"
          >
            {{ addButtonText }}
          </router-link>
        </div>
      </slot>
    </div>

    <!-- Items Grid/List -->
    <div v-else :class="containerClasses">
      <!--
        v-for directive iterating over filteredItems
        :key ensures proper DOM updates
      -->
      <slot 
        name="items" 
        :items="filteredItems"
        :view-mode="viewMode"
      >
        <div 
          v-for="(item, index) in paginatedItems" 
          :key="getItemKey(item, index)"
          :class="itemClasses"
        >
          <slot name="item" :item="item" :index="index">
            <!-- Default item rendering -->
            <div class="card p-4">
              <pre class="text-xs text-gray-600 overflow-auto">{{ JSON.stringify(item, null, 2) }}</pre>
            </div>
          </slot>
        </div>
      </slot>
    </div>

    <!-- Pagination -->
    <div 
      v-if="showPagination && totalPages > 1" 
      class="pagination mt-6 flex items-center justify-between"
    >
      <div class="text-sm text-gray-600">
        Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredItems.length }} results
      </div>
      
      <div class="flex items-center space-x-2">
        <!-- Previous Button -->
        <button
          type="button"
          @click="goToPage(currentPage - 1)"
          :disabled="currentPage === 1"
          class="btn btn-secondary btn-sm"
          :class="{ 'opacity-50 cursor-not-allowed': currentPage === 1 }"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>

        <!-- Page Numbers -->
        <template v-for="page in visiblePages" :key="page">
          <button
            v-if="page !== '...'"
            type="button"
            @click="goToPage(page)"
            :class="pageButtonClasses(page)"
          >
            {{ page }}
          </button>
          <span v-else class="px-2 text-gray-400">...</span>
        </template>

        <!-- Next Button -->
        <button
          type="button"
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="btn btn-secondary btn-sm"
          :class="{ 'opacity-50 cursor-not-allowed': currentPage === totalPages }"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </button>
      </div>

      <!-- Items Per Page -->
      <div class="flex items-center space-x-2">
        <span class="text-sm text-gray-600">Per page:</span>
        <select
          v-model.number="itemsPerPage"
          class="form-input py-1 text-sm w-20"
          @change="handlePerPageChange"
        >
          <option v-for="option in perPageOptions" :key="option" :value="option">
            {{ option }}
          </option>
        </select>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ListComponent - Reusable List with Filtering/Sorting/Pagination
 */
export default {
  // Component name
  name: 'ListComponent',

  /**
   * props - Configuration options
   */
  props: {
    // Data items to display
    items: {
      type: Array,
      required: true,
      default: () => []
    },

    // Unique key field for items
    itemKey: {
      type: String,
      default: 'id'
    },

    // Title for the list
    title: {
      type: String,
      default: ''
    },

    // Loading state
    loading: {
      type: Boolean,
      default: false
    },
    loadingText: {
      type: String,
      default: 'Loading items...'
    },

    // Empty state messages
    emptyTitle: {
      type: String,
      default: 'No items found'
    },
    emptyMessage: {
      type: String,
      default: 'Try adjusting your search or filters.'
    },

    // Add button configuration
    addRoute: {
      type: [String, Object],
      default: null
    },
    addButtonText: {
      type: String,
      default: 'Add New'
    },

    // Search configuration
    showSearch: {
      type: Boolean,
      default: true
    },
    searchPlaceholder: {
      type: String,
      default: 'Search...'
    },
    searchFields: {
      type: Array,
      default: () => ['name', 'title', 'description']
    },
    searchDebounce: {
      type: Number,
      default: 300
    },

    // Filter configuration
    filterOptions: {
      type: Array,
      default: () => []
    },
    filterPlaceholder: {
      type: String,
      default: 'All Categories'
    },
    filterField: {
      type: String,
      default: 'category'
    },

    // Sort configuration
    sortOptions: {
      type: Array,
      default: () => [
        { value: 'name-asc', label: 'Name (A-Z)' },
        { value: 'name-desc', label: 'Name (Z-A)' },
        { value: 'date-desc', label: 'Newest First' },
        { value: 'date-asc', label: 'Oldest First' }
      ]
    },
    defaultSort: {
      type: String,
      default: 'name-asc'
    },

    // View options
    showViewToggle: {
      type: Boolean,
      default: true
    },
    defaultViewMode: {
      type: String,
      default: 'grid',
      validator: value => ['grid', 'list'].includes(value)
    },

    // Grid configuration
    gridCols: {
      type: Object,
      default: () => ({
        sm: 1,
        md: 2,
        lg: 3,
        xl: 4
      })
    },

    // Pagination
    showPagination: {
      type: Boolean,
      default: true
    },
    defaultPerPage: {
      type: Number,
      default: 12
    },
    perPageOptions: {
      type: Array,
      default: () => [6, 12, 24, 48]
    },

    // UI options
    showHeader: {
      type: Boolean,
      default: true
    },
    showFilters: {
      type: Boolean,
      default: true
    }
  },

  /**
   * emits - Events for parent communication
   */
  emits: [
    'search',
    'filter',
    'sort',
    'page-change',
    'view-change',
    'item-click'
  ],

  /**
   * data() - Internal reactive state
   */
  data() {
    return {
      // Search state
      searchQuery: '',
      searchTimeout: null,

      // Filter state
      selectedFilter: '',

      // Sort state
      selectedSort: this.defaultSort,

      // View mode
      viewMode: this.defaultViewMode,

      // Pagination state
      currentPage: 1,
      itemsPerPage: this.defaultPerPage
    };
  },

  /**
   * computed - Derived properties
   */
  computed: {
    /**
     * Filter items based on search query and selected filter
     * Demonstrates chained array methods
     */
    filteredItems() {
      let result = [...this.items];

      // Apply search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(item => {
          return this.searchFields.some(field => {
            const value = this.getNestedValue(item, field);
            return value && String(value).toLowerCase().includes(query);
          });
        });
      }

      // Apply category/type filter
      if (this.selectedFilter) {
        result = result.filter(item => {
          const value = this.getNestedValue(item, this.filterField);
          return value === this.selectedFilter;
        });
      }

      // Apply sorting
      if (this.selectedSort) {
        result = this.sortItems(result);
      }

      return result;
    },

    /**
     * Get paginated items subset
     */
    paginatedItems() {
      if (!this.showPagination) {
        return this.filteredItems;
      }

      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      return this.filteredItems.slice(start, end);
    },

    /**
     * Calculate total pages
     */
    totalPages() {
      return Math.ceil(this.filteredItems.length / this.itemsPerPage);
    },

    /**
     * Pagination display values
     */
    paginationStart() {
      if (this.filteredItems.length === 0) return 0;
      return (this.currentPage - 1) * this.itemsPerPage + 1;
    },

    paginationEnd() {
      const end = this.currentPage * this.itemsPerPage;
      return Math.min(end, this.filteredItems.length);
    },

    /**
     * Generate visible page numbers with ellipsis
     */
    visiblePages() {
      const pages = [];
      const total = this.totalPages;
      const current = this.currentPage;

      if (total <= 7) {
        for (let i = 1; i <= total; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        
        if (current > 3) {
          pages.push('...');
        }
        
        for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
          pages.push(i);
        }
        
        if (current < total - 2) {
          pages.push('...');
        }
        
        pages.push(total);
      }

      return pages;
    },

    /**
     * Check if any filters are active
     */
    hasActiveFilters() {
      return this.searchQuery || this.selectedFilter;
    },

    /**
     * Container classes based on view mode
     */
    containerClasses() {
      if (this.viewMode === 'list') {
        return 'space-y-4';
      }

      const { sm, md, lg, xl } = this.gridCols;
      return `grid grid-cols-${sm} md:grid-cols-${md} lg:grid-cols-${lg} xl:grid-cols-${xl} gap-6`;
    },

    /**
     * Individual item classes
     */
    itemClasses() {
      return this.viewMode === 'list' ? 'w-full' : '';
    }
  },

  /**
   * watch - Monitor changes
   */
  watch: {
    /**
     * Reset to first page when items change
     */
    items: {
      handler() {
        this.currentPage = 1;
      },
      deep: true
    },

    /**
     * Reset to first page when filter changes
     */
    selectedFilter() {
      this.currentPage = 1;
    },

    /**
     * Emit view mode change
     */
    viewMode(newValue) {
      this.$emit('view-change', newValue);
    }
  },

  /**
   * created - Lifecycle hook
   */
  created() {
    console.log('[ListComponent] Created with', this.items.length, 'items');
  },

  /**
   * mounted - Lifecycle hook
   */
  mounted() {
    // Check for URL query params and apply filters
    this.initializeFromUrl();
  },

  /**
   * methods - Component methods
   */
  methods: {
    /**
     * Get item key for v-for iteration
     * Uses configured itemKey or falls back to index
     */
    getItemKey(item, index) {
      return item[this.itemKey] || index;
    },

    /**
     * Get nested object value using dot notation
     * Example: getNestedValue(item, 'artist.name')
     */
    getNestedValue(obj, path) {
      return path.split('.').reduce((acc, part) => acc && acc[part], obj);
    },

    /**
     * Handle search input with debounce
     * Prevents excessive filtering during typing
     */
    handleSearchInput() {
      // Clear existing timeout
      if (this.searchTimeout) {
        clearTimeout(this.searchTimeout);
      }

      // Set new debounced timeout
      this.searchTimeout = setTimeout(() => {
        this.currentPage = 1;
        this.$emit('search', this.searchQuery);
      }, this.searchDebounce);
    },

    /**
     * Clear search query
     */
    clearSearch() {
      this.searchQuery = '';
      this.currentPage = 1;
      this.$emit('search', '');
    },

    /**
     * Handle filter change
     */
    handleFilterChange() {
      this.$emit('filter', this.selectedFilter);
    },

    /**
     * Clear selected filter
     */
    clearFilter() {
      this.selectedFilter = '';
      this.$emit('filter', '');
    },

    /**
     * Clear all filters
     */
    clearAllFilters() {
      this.searchQuery = '';
      this.selectedFilter = '';
      this.currentPage = 1;
      this.$emit('search', '');
      this.$emit('filter', '');
    },

    /**
     * Handle sort change
     */
    handleSortChange() {
      this.$emit('sort', this.selectedSort);
    },

    /**
     * Sort items based on selected sort option
     */
    sortItems(items) {
      const [field, direction] = this.selectedSort.split('-');
      
      return items.sort((a, b) => {
        let aValue = this.getNestedValue(a, field);
        let bValue = this.getNestedValue(b, field);

        // Handle dates
        if (field === 'date' || field === 'createdAt' || field === 'updatedAt') {
          aValue = new Date(aValue);
          bValue = new Date(bValue);
        }

        // Handle strings
        if (typeof aValue === 'string') {
          aValue = aValue.toLowerCase();
          bValue = bValue.toLowerCase();
        }

        if (direction === 'asc') {
          return aValue > bValue ? 1 : -1;
        } else {
          return aValue < bValue ? 1 : -1;
        }
      });
    },

    /**
     * Get filter label by value
     */
    getFilterLabel(value) {
      const option = this.filterOptions.find(opt => opt.value === value);
      return option ? option.label : value;
    },

    /**
     * Navigate to specific page
     */
    goToPage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        this.$emit('page-change', page);
        
        // Scroll to top of list
        this.$el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    },

    /**
     * Handle items per page change
     */
    handlePerPageChange() {
      this.currentPage = 1;
    },

    /**
     * View toggle button classes
     */
    viewButtonClasses(mode) {
      const base = 'p-2 rounded-lg transition-colors';
      if (this.viewMode === mode) {
        return `${base} bg-primary-100 text-primary-600`;
      }
      return `${base} bg-gray-100 text-gray-600 hover:bg-gray-200`;
    },

    /**
     * Page button classes
     */
    pageButtonClasses(page) {
      const base = 'w-8 h-8 rounded-lg text-sm font-medium transition-colors';
      if (page === this.currentPage) {
        return `${base} bg-primary-600 text-white`;
      }
      return `${base} bg-gray-100 text-gray-600 hover:bg-gray-200`;
    },

    /**
     * Initialize filters from URL query params
     */
    initializeFromUrl() {
      const query = this.$route?.query || {};
      
      if (query.search) {
        this.searchQuery = query.search;
      }
      if (query.filter) {
        this.selectedFilter = query.filter;
      }
      if (query.sort) {
        this.selectedSort = query.sort;
      }
      if (query.page) {
        this.currentPage = parseInt(query.page, 10) || 1;
      }
    }
  }
};
</script>

<style scoped>
/* List component animations */
.list-component {
  @apply min-h-0;
}

/* Grid responsive overrides */
@media (min-width: 640px) {
  .grid-cols-1 { grid-template-columns: repeat(1, minmax(0, 1fr)); }
}

@media (min-width: 768px) {
  .md\:grid-cols-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (min-width: 1024px) {
  .lg\:grid-cols-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}

@media (min-width: 1280px) {
  .xl\:grid-cols-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
}
</style>
