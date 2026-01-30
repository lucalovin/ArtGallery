<template>
  <!--
    VisitorTrendsChart.vue - Monthly Exhibition Activity
    Art Gallery Management System - DW/BI Module
    
    Query 3: "Analyze exhibition performance: show monthly activity metrics for the past year"
    Chart Type: Line Chart with gradient area fill
  -->
  <div class="chart-container bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
        <p class="text-sm text-gray-500">{{ description }}</p>
      </div>
      <div class="flex items-center space-x-2">
        <select 
          v-model="selectedMonths" 
          @change="fetchData"
          class="text-sm border border-gray-300 rounded-md px-2 py-1 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
        >
          <option :value="6">Last 6 months</option>
          <option :value="12">Last 12 months</option>
          <option :value="24">Last 24 months</option>
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
        <p class="text-gray-500">Loading activity data...</p>
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
      <line-chart
        :chart-data="chartData"
        :height="300"
        :options="chartOptions"
      />
      
      <!-- Summary Statistics -->
      <div class="mt-6 grid grid-cols-4 gap-4 pt-4 border-t border-gray-100">
        <div class="text-center">
          <p class="text-2xl font-bold text-indigo-600">{{ formatNumber(totalExhibitions) }}</p>
          <p class="text-xs text-gray-500">Exhibitions</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-emerald-600">{{ formatNumber(totalArtworks) }}</p>
          <p class="text-xs text-gray-500">Artworks Exhibited</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-amber-600">{{ formatCurrency(totalValue) }}</p>
          <p class="text-xs text-gray-500">Total Value</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-purple-600">{{ avgRating }}</p>
          <p class="text-xs text-gray-500">Avg Rating</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import LineChart from '../reports/charts/LineChart.vue';
import { analyticsAPI } from '@/api/analyticsAPI';

/**
 * VisitorTrendsChart Component
 * Displays monthly exhibition activity with trends
 */
export default {
  name: 'VisitorTrendsChart',

  components: {
    'line-chart': LineChart
  },

  props: {
    title: {
      type: String,
      default: 'Monthly Exhibition Activity'
    },
    description: {
      type: String,
      default: 'Exhibition performance metrics over time'
    },
    initialMonths: {
      type: Number,
      default: 12
    }
  },

  data() {
    return {
      isLoading: false,
      error: null,
      selectedMonths: this.initialMonths,
      activityData: []
    };
  },

  computed: {
    chartData() {
      if (!this.activityData.length) {
        return { labels: [], datasets: [] };
      }

      const labels = this.activityData.map(d => `${d.monthName} ${d.year}`);

      return {
        labels,
        datasets: [
          {
            label: 'Exhibitions',
            data: this.activityData.map(d => d.exhibitionCount),
            borderColor: 'rgb(99, 102, 241)',
            backgroundColor: 'rgba(99, 102, 241, 0.1)',
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6
          },
          {
            label: 'Artworks Exhibited',
            data: this.activityData.map(d => d.artworksExhibited),
            borderColor: 'rgb(16, 185, 129)',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6
          }
        ]
      };
    },

    chartOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          mode: 'index',
          intersect: false
        },
        plugins: {
          legend: {
            display: true,
            position: 'top'
          },
          tooltip: {
            callbacks: {
              afterBody: (context) => {
                const index = context[0].dataIndex;
                const data = this.activityData[index];
                return [
                  `Value: ${this.formatCurrency(data.totalArtworkValue)}`,
                  `Rating: ${data.averageRating?.toFixed(1) || 'N/A'}`
                ];
              }
            }
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            },
            ticks: {
              maxRotation: 45,
              minRotation: 45
            }
          },
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(0, 0, 0, 0.05)'
            },
            title: {
              display: true,
              text: 'Count'
            }
          }
        }
      };
    },

    totalExhibitions() {
      return this.activityData.reduce((sum, d) => sum + d.exhibitionCount, 0);
    },

    totalArtworks() {
      return this.activityData.reduce((sum, d) => sum + d.artworksExhibited, 0);
    },

    totalValue() {
      return this.activityData.reduce((sum, d) => sum + d.totalArtworkValue, 0);
    },

    avgRating() {
      const ratings = this.activityData.filter(d => d.averageRating != null);
      if (!ratings.length) return 'N/A';
      const avg = ratings.reduce((sum, d) => sum + d.averageRating, 0) / ratings.length;
      return avg.toFixed(1);
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
        const response = await analyticsAPI.getVisitorTrends(this.selectedMonths);
        this.activityData = response.data?.data || response.data || [];
      } catch (err) {
        console.error('Error fetching activity data:', err);
        this.error = err.response?.data?.message || 'Failed to load activity data';
        this.activityData = [];
      } finally {
        this.isLoading = false;
      }
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
