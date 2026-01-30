<template>
  <!--
    MembershipDistributionChart.vue - Location/Gallery Distribution
    Art Gallery Management System - DW/BI Module
    
    Query 4: "What is the gallery occupancy rate and distribution of artworks across different locations?"
    Chart Type: Pie Chart with percentage labels
  -->
  <div class="chart-container bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
        <p class="text-sm text-gray-500">{{ description }}</p>
      </div>
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

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center h-80">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
        <p class="text-gray-500">Loading location data...</p>
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
      <!-- Pie Chart -->
      <div class="flex-1 flex items-center justify-center">
        <pie-chart
          :chart-data="chartData"
          :height="300"
          :options="chartOptions"
          :show-labels="false"
        />
      </div>
      
      <!-- Location Details Table -->
      <div class="lg:w-80">
        <div class="text-sm font-medium text-gray-500 mb-2">Location Breakdown</div>
        <div class="space-y-2 max-h-72 overflow-y-auto">
          <div 
            v-for="(item, index) in locationData" 
            :key="index"
            class="flex items-center p-2 rounded-lg hover:bg-gray-50 transition-colors border border-gray-100"
          >
            <span 
              class="w-3 h-3 rounded-full flex-shrink-0 mr-3" 
              :style="{ backgroundColor: getColor(index) }"
            ></span>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">{{ item.locationName }}</p>
              <p class="text-xs text-gray-500">
                {{ item.galleryRoom }} • {{ item.locationType }}
              </p>
            </div>
            <div class="text-right ml-2">
              <p class="text-sm font-semibold text-gray-900">{{ item.artworksCount }}</p>
              <p class="text-xs text-indigo-600">{{ item.percentage }}%</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Summary Statistics -->
    <div v-if="!isLoading && !error" class="mt-6 grid grid-cols-3 gap-4 pt-4 border-t border-gray-100">
      <div class="text-center">
        <p class="text-2xl font-bold text-indigo-600">{{ formatNumber(totalLocations) }}</p>
        <p class="text-xs text-gray-500">Locations</p>
      </div>
      <div class="text-center">
        <p class="text-2xl font-bold text-emerald-600">{{ formatNumber(totalArtworks) }}</p>
        <p class="text-xs text-gray-500">Artworks Distributed</p>
      </div>
      <div class="text-center">
        <p class="text-2xl font-bold text-amber-600">{{ formatCurrency(totalValue) }}</p>
        <p class="text-xs text-gray-500">Total Value</p>
      </div>
    </div>
  </div>
</template>

<script>
import PieChart from '../reports/charts/PieChart.vue';
import { analyticsAPI } from '@/api/analyticsAPI';

/**
 * MembershipDistributionChart Component
 * Displays location/gallery distribution of artworks
 */
export default {
  name: 'MembershipDistributionChart',

  components: {
    'pie-chart': PieChart
  },

  props: {
    title: {
      type: String,
      default: 'Gallery Location Distribution'
    },
    description: {
      type: String,
      default: 'Artwork distribution across gallery locations'
    }
  },

  data() {
    return {
      isLoading: false,
      error: null,
      locationData: [],
      colorPalette: [
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
      ]
    };
  },

  computed: {
    chartData() {
      if (!this.locationData.length) {
        return { labels: [], datasets: [] };
      }

      return {
        labels: this.locationData.map(d => d.locationName),
        datasets: [
          {
            data: this.locationData.map(d => d.artworksCount),
            backgroundColor: this.locationData.map((_, i) => this.getColor(i)),
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
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const item = this.locationData[context.dataIndex];
                return [
                  `Artworks: ${item.artworksCount}`,
                  `Value: ${this.formatCurrency(item.totalValue)}`,
                  `Share: ${item.percentage}%`
                ];
              }
            }
          }
        }
      };
    },

    totalLocations() {
      return this.locationData.length;
    },

    totalArtworks() {
      return this.locationData.reduce((sum, d) => sum + d.artworksCount, 0);
    },

    totalValue() {
      return this.locationData.reduce((sum, d) => sum + d.totalValue, 0);
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
        const response = await analyticsAPI.getMembershipDistribution();
        this.locationData = response.data?.data || response.data || [];
      } catch (err) {
        console.error('Error fetching location data:', err);
        this.error = err.response?.data?.message || 'Failed to load location data';
        this.locationData = [];
      } finally {
        this.isLoading = false;
      }
    },

    getColor(index) {
      return this.colorPalette[index % this.colorPalette.length];
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
  min-height: 420px;
}
</style>
