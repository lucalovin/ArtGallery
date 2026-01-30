<template>
  <!--
    ValueByCategoryChart.vue - Collection Value by Medium and Category
    Art Gallery Management System - DW/BI Module
    
    Query 2: "What is the total estimated value of the collection broken down by art medium and collection type?"
    Chart Type: Doughnut Chart with legend
  -->
  <div class="chart-container bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
        <p class="text-sm text-gray-500">{{ description }}</p>
      </div>
      <div class="flex items-center space-x-2">
        <select 
          v-model="viewMode" 
          class="text-sm border border-gray-300 rounded-md px-2 py-1 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
        >
          <option value="medium">By Medium</option>
          <option value="collection">By Collection</option>
        </select>
        <button 
          @click="fetchData" 
          :disabled="isLoading"
          class="p-2 text-gray-500 hover:text-indigo-600 transition-colors"
          title="Refresh data"
        >
          <svg class="w-5 h-5" :class="{ 'animate-spin': isLoading }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center h-80">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
        <p class="text-gray-500">Loading category data...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center h-80">
      <div class="text-center">
        <svg class="w-12 h-12 text-red-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-red-600 mb-2">{{ error }}</p>
        <button @click="fetchData" class="text-indigo-600 hover:text-indigo-800 text-sm font-medium">
          Try Again
        </button>
      </div>
    </div>

    <!-- Chart with Legend -->
    <div v-else class="flex flex-col lg:flex-row gap-6">
      <!-- Doughnut Chart -->
      <div class="flex-1">
        <doughnut-chart
          :chart-data="chartData"
          :height="300"
          :options="chartOptions"
        />
      </div>
      
      <!-- Custom Legend -->
      <div class="lg:w-64 space-y-2 max-h-80 overflow-y-auto">
        <div 
          v-for="(item, index) in legendItems" 
          :key="index"
          class="flex items-center justify-between p-2 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <div class="flex items-center space-x-2">
            <span 
              class="w-3 h-3 rounded-full flex-shrink-0" 
              :style="{ backgroundColor: item.color }"
            ></span>
            <span class="text-sm text-gray-700 truncate" :title="item.label">
              {{ item.label }}
            </span>
          </div>
          <div class="text-right">
            <p class="text-sm font-semibold text-gray-900">{{ formatCurrency(item.value) }}</p>
            <p class="text-xs text-gray-500">{{ item.percentage }}%</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Summary Statistics -->
    <div v-if="!isLoading && !error" class="mt-6 grid grid-cols-3 gap-4 pt-4 border-t border-gray-100">
      <div class="text-center">
        <p class="text-2xl font-bold text-indigo-600">{{ formatNumber(totalCategories) }}</p>
        <p class="text-xs text-gray-500">Categories</p>
      </div>
      <div class="text-center">
        <p class="text-2xl font-bold text-emerald-600">{{ formatCurrency(totalValue) }}</p>
        <p class="text-xs text-gray-500">Total Collection Value</p>
      </div>
      <div class="text-center">
        <p class="text-2xl font-bold text-amber-600">{{ formatNumber(totalArtworks) }}</p>
        <p class="text-xs text-gray-500">Total Artworks</p>
      </div>
    </div>
  </div>
</template>

<script>
import DoughnutChart from '../reports/charts/DoughnutChart.vue';
import { analyticsAPI } from '@/api/analyticsAPI';

/**
 * ValueByCategoryChart Component
 * Displays collection value breakdown by medium and collection type
 */
export default {
  name: 'ValueByCategoryChart',

  components: {
    'doughnut-chart': DoughnutChart
  },

  props: {
    title: {
      type: String,
      default: 'Collection Value by Category'
    },
    description: {
      type: String,
      default: 'Breakdown of estimated value by art medium and collection'
    }
  },

  data() {
    return {
      isLoading: false,
      error: null,
      viewMode: 'medium',
      categoryData: []
    };
  },

  computed: {
    aggregatedData() {
      if (!this.categoryData.length) return [];

      const grouped = {};
      
      this.categoryData.forEach(item => {
        const key = this.viewMode === 'medium' ? item.mediumType : item.collectionName;
        if (!grouped[key]) {
          grouped[key] = { label: key, value: 0, count: 0 };
        }
        grouped[key].value += item.totalValue;
        grouped[key].count += item.artworkCount;
      });

      const sorted = Object.values(grouped).sort((a, b) => b.value - a.value);
      
      // Calculate percentages
      const total = sorted.reduce((sum, item) => sum + item.value, 0);
      sorted.forEach(item => {
        item.percentage = total > 0 ? ((item.value / total) * 100).toFixed(1) : 0;
      });

      return sorted;
    },

    chartData() {
      if (!this.aggregatedData.length) {
        return { labels: [], datasets: [] };
      }

      const colors = this.generateColors(this.aggregatedData.length);

      return {
        labels: this.aggregatedData.map(d => d.label),
        datasets: [
          {
            data: this.aggregatedData.map(d => d.value),
            backgroundColor: colors,
            borderColor: '#ffffff',
            borderWidth: 2
          }
        ]
      };
    },

    chartOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '60%',
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const item = this.aggregatedData[context.dataIndex];
                return [
                  `Value: ${this.formatCurrency(item.value)}`,
                  `Artworks: ${item.count}`,
                  `Share: ${item.percentage}%`
                ];
              }
            }
          }
        }
      };
    },

    legendItems() {
      const colors = this.generateColors(this.aggregatedData.length);
      return this.aggregatedData.map((item, index) => ({
        label: item.label,
        value: item.value,
        percentage: item.percentage,
        color: colors[index]
      }));
    },

    totalCategories() {
      return this.aggregatedData.length;
    },

    totalValue() {
      return this.aggregatedData.reduce((sum, d) => sum + d.value, 0);
    },

    totalArtworks() {
      return this.aggregatedData.reduce((sum, d) => sum + d.count, 0);
    }
  },

  created() {
    this.fetchData();
  },

  methods: {
    async fetchData() {
      this.isLoading = true;
      this.error = null;

      try {
        const response = await analyticsAPI.getValueByCategory();
        this.categoryData = response.data?.data || response.data || [];
      } catch (err) {
        console.error('Error fetching category data:', err);
        this.error = err.response?.data?.message || 'Failed to load category data';
        this.categoryData = [];
      } finally {
        this.isLoading = false;
      }
    },

    generateColors(count) {
      const palette = [
        'rgba(99, 102, 241, 0.85)',   // Indigo
        'rgba(16, 185, 129, 0.85)',   // Emerald
        'rgba(245, 158, 11, 0.85)',   // Amber
        'rgba(239, 68, 68, 0.85)',    // Red
        'rgba(59, 130, 246, 0.85)',   // Blue
        'rgba(168, 85, 247, 0.85)',   // Purple
        'rgba(236, 72, 153, 0.85)',   // Pink
        'rgba(20, 184, 166, 0.85)',   // Teal
        'rgba(251, 146, 60, 0.85)',   // Orange
        'rgba(34, 197, 94, 0.85)',    // Green
        'rgba(139, 92, 246, 0.85)',   // Violet
        'rgba(14, 165, 233, 0.85)'    // Sky
      ];

      const colors = [];
      for (let i = 0; i < count; i++) {
        colors.push(palette[i % palette.length]);
      }
      return colors;
    },

    formatNumber(value) {
      return new Intl.NumberFormat().format(value);
    },

    formatCurrency(value) {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
      }).format(value);
    }
  }
};
</script>

<style scoped>
.chart-container {
  min-height: 400px;
}
</style>
