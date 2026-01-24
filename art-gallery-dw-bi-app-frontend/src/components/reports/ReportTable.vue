<template>
  <!--
    ReportTable.vue - Data Table Component for BI Reports
    Art Gallery Management System - DW/BI Module
  -->
  <div class="report-table bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Table Header -->
    <div 
      v-if="title || showExport"
      class="px-6 py-4 border-b border-gray-100 flex items-center justify-between"
    >
      <div>
        <h3 v-if="title" class="text-lg font-semibold text-gray-800">{{ title }}</h3>
        <p v-if="subtitle" class="text-sm text-gray-500 mt-1">{{ subtitle }}</p>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Search -->
        <div v-if="searchable" class="relative">
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="searchPlaceholder"
            class="pl-10 pr-4 py-2 text-sm border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 w-64"
          />
          <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400">
            🔍
          </span>
        </div>

        <!-- Export Button -->
        <button
          v-if="showExport"
          @click="exportData"
          class="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <span class="mr-2">📥</span>
          Export
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="p-8">
      <div class="animate-pulse space-y-4">
        <div class="h-10 bg-gray-200 rounded"></div>
        <div class="h-8 bg-gray-100 rounded"></div>
        <div class="h-8 bg-gray-100 rounded"></div>
        <div class="h-8 bg-gray-100 rounded"></div>
        <div class="h-8 bg-gray-100 rounded"></div>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else-if="!filteredData || filteredData.length === 0"
      class="p-12 text-center"
    >
      <span class="text-4xl mb-4 block">📋</span>
      <p class="text-gray-500">{{ emptyMessage }}</p>
    </div>

    <!-- Table Content -->
    <div v-else class="overflow-x-auto">
      <table class="w-full">
        <thead class="bg-gray-50">
          <tr>
            <!-- Checkbox Column -->
            <th 
              v-if="selectable"
              class="px-6 py-3 text-left"
            >
              <input
                type="checkbox"
                :checked="isAllSelected"
                :indeterminate="isIndeterminate"
                @change="toggleSelectAll"
                class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
              />
            </th>

            <!-- Data Columns -->
            <th
              v-for="column in columns"
              :key="column.key"
              class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100 transition-colors"
              :class="{ 'text-right': column.align === 'right', 'text-center': column.align === 'center' }"
              :style="column.width ? { width: column.width } : {}"
              @click="sortable && column.sortable !== false ? handleSort(column.key) : null"
            >
              <div class="flex items-center" :class="{ 'justify-end': column.align === 'right', 'justify-center': column.align === 'center' }">
                <span>{{ column.label }}</span>
                <span 
                  v-if="sortable && column.sortable !== false" 
                  class="ml-1 text-gray-400"
                >
                  <span v-if="sortKey === column.key">
                    {{ sortOrder === 'asc' ? '↑' : '↓' }}
                  </span>
                  <span v-else class="opacity-50">↕</span>
                </span>
              </div>
            </th>

            <!-- Actions Column -->
            <th 
              v-if="showActions"
              class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider"
            >
              Actions
            </th>
          </tr>
        </thead>

        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="(row, rowIndex) in paginatedData"
            :key="getRowKey(row, rowIndex)"
            class="hover:bg-gray-50 transition-colors"
            :class="{ 'bg-primary-50': isRowSelected(row) }"
          >
            <!-- Checkbox -->
            <td v-if="selectable" class="px-6 py-4">
              <input
                type="checkbox"
                :checked="isRowSelected(row)"
                @change="toggleRowSelection(row)"
                class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
              />
            </td>

            <!-- Data Cells -->
            <td
              v-for="column in columns"
              :key="column.key"
              class="px-6 py-4 text-sm"
              :class="[
                column.align === 'right' ? 'text-right' : column.align === 'center' ? 'text-center' : 'text-left',
                column.class || ''
              ]"
            >
              <!-- Custom slot -->
              <slot 
                v-if="$slots[`cell-${column.key}`]"
                :name="`cell-${column.key}`"
                :row="row"
                :value="getCellValue(row, column.key)"
                :column="column"
              ></slot>

              <!-- Badge formatter -->
              <span 
                v-else-if="column.type === 'badge'"
                class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                :class="getBadgeClass(getCellValue(row, column.key), column.badgeColors)"
              >
                {{ getCellValue(row, column.key) }}
              </span>

              <!-- Currency formatter -->
              <span v-else-if="column.type === 'currency'" class="font-medium text-gray-900">
                {{ formatCurrency(getCellValue(row, column.key)) }}
              </span>

              <!-- Number formatter -->
              <span v-else-if="column.type === 'number'" class="font-medium">
                {{ formatNumber(getCellValue(row, column.key)) }}
              </span>

              <!-- Date formatter -->
              <span v-else-if="column.type === 'date'" class="text-gray-600">
                {{ formatDate(getCellValue(row, column.key)) }}
              </span>

              <!-- Progress bar -->
              <div v-else-if="column.type === 'progress'" class="w-full">
                <div class="flex items-center">
                  <div class="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                    <div 
                      class="h-full bg-primary-500 rounded-full"
                      :style="{ width: `${Math.min(getCellValue(row, column.key), 100)}%` }"
                    ></div>
                  </div>
                  <span class="ml-2 text-xs text-gray-500">{{ getCellValue(row, column.key) }}%</span>
                </div>
              </div>

              <!-- Default text -->
              <span v-else class="text-gray-700">
                {{ getCellValue(row, column.key) }}
              </span>
            </td>

            <!-- Actions -->
            <td v-if="showActions" class="px-6 py-4 text-right">
              <slot name="actions" :row="row" :index="rowIndex">
                <div class="flex items-center justify-end space-x-2">
                  <button
                    v-if="actions.includes('view')"
                    @click="$emit('view', row)"
                    class="p-1.5 text-gray-400 hover:text-blue-600 transition-colors"
                    title="View"
                  >
                    👁️
                  </button>
                  <button
                    v-if="actions.includes('edit')"
                    @click="$emit('edit', row)"
                    class="p-1.5 text-gray-400 hover:text-primary-600 transition-colors"
                    title="Edit"
                  >
                    ✏️
                  </button>
                  <button
                    v-if="actions.includes('delete')"
                    @click="$emit('delete', row)"
                    class="p-1.5 text-gray-400 hover:text-red-600 transition-colors"
                    title="Delete"
                  >
                    🗑️
                  </button>
                </div>
              </slot>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div 
      v-if="paginated && totalPages > 1"
      class="px-6 py-4 border-t border-gray-100 flex items-center justify-between"
    >
      <div class="text-sm text-gray-500">
        Showing {{ startIndex + 1 }} to {{ endIndex }} of {{ filteredData.length }} results
      </div>

      <div class="flex items-center space-x-2">
        <button
          :disabled="currentPage === 1"
          @click="goToPage(currentPage - 1)"
          class="px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Previous
        </button>

        <div class="flex items-center space-x-1">
          <button
            v-for="page in visiblePages"
            :key="page"
            @click="goToPage(page)"
            class="w-8 h-8 text-sm font-medium rounded-lg transition-colors"
            :class="currentPage === page 
              ? 'bg-primary-600 text-white' 
              : 'text-gray-600 hover:bg-gray-100'"
          >
            {{ page }}
          </button>
        </div>

        <button
          :disabled="currentPage === totalPages"
          @click="goToPage(currentPage + 1)"
          class="px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ReportTable Component
 * Feature-rich data table for BI reports with sorting, filtering, pagination
 */
export default {
  name: 'ReportTable',

  props: {
    data: {
      type: Array,
      required: true
    },
    columns: {
      type: Array,
      required: true,
      // Each column: { key, label, type?, align?, width?, sortable?, class?, badgeColors? }
    },
    title: {
      type: String,
      default: ''
    },
    subtitle: {
      type: String,
      default: ''
    },
    rowKey: {
      type: String,
      default: 'id'
    },
    isLoading: {
      type: Boolean,
      default: false
    },
    emptyMessage: {
      type: String,
      default: 'No data available'
    },
    sortable: {
      type: Boolean,
      default: true
    },
    searchable: {
      type: Boolean,
      default: false
    },
    searchPlaceholder: {
      type: String,
      default: 'Search...'
    },
    searchKeys: {
      type: Array,
      default: () => []
    },
    selectable: {
      type: Boolean,
      default: false
    },
    paginated: {
      type: Boolean,
      default: true
    },
    pageSize: {
      type: Number,
      default: 10
    },
    showExport: {
      type: Boolean,
      default: false
    },
    showActions: {
      type: Boolean,
      default: false
    },
    actions: {
      type: Array,
      default: () => ['view', 'edit', 'delete']
    }
  },

  emits: ['view', 'edit', 'delete', 'selection-change', 'sort-change', 'export'],

  data() {
    return {
      searchQuery: '',
      sortKey: '',
      sortOrder: 'asc',
      currentPage: 1,
      selectedRows: []
    };
  },

  computed: {
    filteredData() {
      let result = [...this.data];

      // Apply search filter
      if (this.searchQuery && this.searchable) {
        const query = this.searchQuery.toLowerCase();
        const keys = this.searchKeys.length > 0 
          ? this.searchKeys 
          : this.columns.map(c => c.key);
        
        result = result.filter(row => {
          return keys.some(key => {
            const value = this.getCellValue(row, key);
            return String(value).toLowerCase().includes(query);
          });
        });
      }

      // Apply sorting
      if (this.sortKey) {
        result.sort((a, b) => {
          const aVal = this.getCellValue(a, this.sortKey);
          const bVal = this.getCellValue(b, this.sortKey);

          let comparison = 0;
          if (aVal < bVal) comparison = -1;
          if (aVal > bVal) comparison = 1;

          return this.sortOrder === 'asc' ? comparison : -comparison;
        });
      }

      return result;
    },

    paginatedData() {
      if (!this.paginated) {
        return this.filteredData;
      }

      return this.filteredData.slice(this.startIndex, this.endIndex);
    },

    totalPages() {
      return Math.ceil(this.filteredData.length / this.pageSize);
    },

    startIndex() {
      return (this.currentPage - 1) * this.pageSize;
    },

    endIndex() {
      return Math.min(this.startIndex + this.pageSize, this.filteredData.length);
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

    isAllSelected() {
      return this.filteredData.length > 0 && 
             this.selectedRows.length === this.filteredData.length;
    },

    isIndeterminate() {
      return this.selectedRows.length > 0 && 
             this.selectedRows.length < this.filteredData.length;
    }
  },

  watch: {
    searchQuery() {
      this.currentPage = 1;
    },

    selectedRows(newVal) {
      this.$emit('selection-change', newVal);
    },

    data() {
      // Reset page when data changes
      this.currentPage = 1;
      this.selectedRows = [];
    }
  },

  methods: {
    getCellValue(row, key) {
      // Support nested keys like 'user.name'
      return key.split('.').reduce((obj, k) => obj && obj[k], row);
    },

    getRowKey(row, index) {
      return row[this.rowKey] || index;
    },

    handleSort(key) {
      if (this.sortKey === key) {
        this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      } else {
        this.sortKey = key;
        this.sortOrder = 'asc';
      }

      this.$emit('sort-change', { key: this.sortKey, order: this.sortOrder });
    },

    goToPage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
      }
    },

    toggleSelectAll() {
      if (this.isAllSelected) {
        this.selectedRows = [];
      } else {
        this.selectedRows = [...this.filteredData];
      }
    },

    toggleRowSelection(row) {
      const index = this.selectedRows.findIndex(r => 
        this.getRowKey(r, 0) === this.getRowKey(row, 0)
      );

      if (index > -1) {
        this.selectedRows.splice(index, 1);
      } else {
        this.selectedRows.push(row);
      }
    },

    isRowSelected(row) {
      return this.selectedRows.some(r => 
        this.getRowKey(r, 0) === this.getRowKey(row, 0)
      );
    },

    formatCurrency(value) {
      if (value == null) return '-';
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(value);
    },

    formatNumber(value) {
      if (value == null) return '-';
      return new Intl.NumberFormat('en-US').format(value);
    },

    formatDate(value) {
      if (!value) return '-';
      return new Date(value).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    },

    getBadgeClass(value, colorMap = {}) {
      const defaultColors = {
        success: 'bg-green-100 text-green-800',
        warning: 'bg-yellow-100 text-yellow-800',
        error: 'bg-red-100 text-red-800',
        info: 'bg-blue-100 text-blue-800',
        default: 'bg-gray-100 text-gray-800'
      };

      const colors = { ...defaultColors, ...colorMap };
      const key = String(value).toLowerCase();
      
      return colors[key] || colors.default;
    },

    exportData() {
      this.$emit('export', {
        data: this.filteredData,
        columns: this.columns,
        format: 'csv'
      });
    }
  }
};
</script>

<style scoped>
.report-table {
  min-width: 100%;
}

/* Custom checkbox indeterminate state */
input[type="checkbox"]:indeterminate {
  background-color: #3d4eed;
  border-color: #3d4eed;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 16 16'%3e%3cpath stroke='white' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M4 8h8'/%3e%3c/svg%3e");
}
</style>
