<template>
  <!--
    TopArtistsChart.vue - Top Artists by Artwork Count
    Art Gallery Management System - DW/BI Module
    
    Query 1: "Show me the top 10 artists with the most artworks in the collection"
    Chart Type: Horizontal Bar Chart
  -->
  <div class="chart-container bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
        <p class="text-sm text-gray-500">{{ description }}</p>
      </div>
      <div class="flex items-center space-x-2">
        <select 
          v-model="selectedTopN" 
          @change="fetchData"
          class="text-sm border border-gray-300 rounded-md px-2 py-1 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
        >
          <option :value="5">Top 5</option>
          <option :value="10">Top 10</option>
          <option :value="15">Top 15</option>
          <option :value="20">Top 20</option>
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
        <p class="text-gray-500">Loading artist data...</p>
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

    <!-- Chart -->
    <div v-else>
      <horizontal-bar-chart
        :chart-data="chartData"
        :height="chartHeight"
        :options="chartOptions"
      />
      
      <!-- Summary Statistics -->
      <div class="mt-4 grid grid-cols-3 gap-4 pt-4 border-t border-gray-100">
        <div class="text-center">
          <p class="text-2xl font-bold text-indigo-600">{{ formatNumber(totalArtworks) }}</p>
          <p class="text-xs text-gray-500">Total Artworks</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-emerald-600">{{ formatCurrency(totalValue) }}</p>
          <p class="text-xs text-gray-500">Total Value</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-amber-600">{{ formatCurrency(averageValue) }}</p>
          <p class="text-xs text-gray-500">Avg Value/Artwork</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import HorizontalBarChart from '../reports/charts/HorizontalBarChart.vue';
import { analyticsAPI } from '@/api/analyticsAPI';

/**
 * TopArtistsChart Component
 * Displays top artists by artwork count with total estimated value
 */
export default {
  name: 'TopArtistsChart',

  components: {
    'horizontal-bar-chart': HorizontalBarChart
  },

  props: {
    title: {
      type: String,
      default: 'Top Artists by Artwork Count'
    },
    description: {
      type: String,
      default: 'Artists with the most artworks in the collection'
    },
    initialTopN: {
      type: Number,
      default: 10
    },
    autoRefresh: {
      type: Boolean,
      default: false
    },
    refreshInterval: {
      type: Number,
      default: 300000 // 5 minutes
    }
  },

  data() {
    return {
      isLoading: false,
      error: null,
      selectedTopN: this.initialTopN,
      artistData: [],
      refreshTimer: null
    };
  },

  computed: {
    chartData() {
      if (!this.artistData.length) {
        return { labels: [], datasets: [] };
      }

      const colors = this.generateColors(this.artistData.length);

      return {
        labels: this.artistData.map(a => a.artistName),
        datasets: [
          {
            label: 'Artwork Count',
            data: this.artistData.map(a => a.artworkCount),
            backgroundColor: colors.background,
            borderColor: colors.border,
            borderWidth: 1,
            borderRadius: 4
          }
        ]
      };
    },

    chartOptions() {
      return {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              afterLabel: (context) => {
                const artist = this.artistData[context.dataIndex];
                return [
                  `Total Value: ${this.formatCurrency(artist.totalValue)}`,
                  `Avg Value: ${this.formatCurrency(artist.averageValue)}`
                ];
              }
            }
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Number of Artworks'
            }
          },
          y: {
            ticks: {
              font: {
                size: 11
              }
            }
          }
        }
      };
    },

    chartHeight() {
      return Math.max(300, this.artistData.length * 35);
    },

    totalArtworks() {
      return this.artistData.reduce((sum, a) => sum + a.artworkCount, 0);
    },

    totalValue() {
      return this.artistData.reduce((sum, a) => sum + a.totalValue, 0);
    },

    averageValue() {
      return this.totalArtworks > 0 ? this.totalValue / this.totalArtworks : 0;
    }
  },

  created() {
    this.fetchData();
  },

  mounted() {
    if (this.autoRefresh) {
      this.startAutoRefresh();
    }
  },

  beforeUnmount() {
    this.stopAutoRefresh();
  },

  methods: {
    async fetchData() {
      this.isLoading = true;
      this.error = null;

      try {
        const response = await analyticsAPI.getTopArtists(this.selectedTopN);
        this.artistData = response.data?.data || response.data || [];
      } catch (err) {
        console.error('Error fetching top artists:', err);
        this.error = err.response?.data?.message || 'Failed to load artist data';
        this.artistData = [];
      } finally {
        this.isLoading = false;
      }
    },

    generateColors(count) {
      const baseColors = [
        { bg: 'rgba(99, 102, 241, 0.8)', border: 'rgb(99, 102, 241)' },   // Indigo
        { bg: 'rgba(16, 185, 129, 0.8)', border: 'rgb(16, 185, 129)' },   // Emerald
        { bg: 'rgba(245, 158, 11, 0.8)', border: 'rgb(245, 158, 11)' },   // Amber
        { bg: 'rgba(239, 68, 68, 0.8)', border: 'rgb(239, 68, 68)' },     // Red
        { bg: 'rgba(59, 130, 246, 0.8)', border: 'rgb(59, 130, 246)' },   // Blue
        { bg: 'rgba(168, 85, 247, 0.8)', border: 'rgb(168, 85, 247)' },   // Purple
        { bg: 'rgba(236, 72, 153, 0.8)', border: 'rgb(236, 72, 153)' },   // Pink
        { bg: 'rgba(20, 184, 166, 0.8)', border: 'rgb(20, 184, 166)' },   // Teal
        { bg: 'rgba(251, 146, 60, 0.8)', border: 'rgb(251, 146, 60)' },   // Orange
        { bg: 'rgba(34, 197, 94, 0.8)', border: 'rgb(34, 197, 94)' }      // Green
      ];

      const background = [];
      const border = [];

      for (let i = 0; i < count; i++) {
        const color = baseColors[i % baseColors.length];
        background.push(color.bg);
        border.push(color.border);
      }

      return { background, border };
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
    },

    startAutoRefresh() {
      this.refreshTimer = setInterval(() => {
        this.fetchData();
      }, this.refreshInterval);
    },

    stopAutoRefresh() {
      if (this.refreshTimer) {
        clearInterval(this.refreshTimer);
        this.refreshTimer = null;
      }
    }
  }
};
</script>

<style scoped>
.chart-container {
  min-height: 400px;
}
</style>
