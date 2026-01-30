<template>
  <!--
    AcquisitionTrendsChart.vue - Annual Exhibition Value Trends
    Art Gallery Management System - DW/BI Module
    
    Query 5: "Show the trend of exhibition activity: how has the annual total artwork value evolved over the last 5 years?"
    Chart Type: Line Chart with data points showing YoY growth rate
  -->
  <div class="chart-container bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
        <p class="text-sm text-gray-500">{{ description }}</p>
      </div>
      <div class="flex items-center space-x-2">
        <select 
          v-model="selectedYears" 
          @change="fetchData"
          class="text-sm border border-gray-300 rounded-md px-2 py-1 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
        >
          <option :value="3">Last 3 years</option>
          <option :value="5">Last 5 years</option>
          <option :value="10">Last 10 years</option>
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
        <p class="text-gray-500">Loading trend data...</p>
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
        :height="320"
        :options="chartOptions"
      />
      
      <!-- YoY Growth Indicators -->
      <div class="mt-4 flex flex-wrap gap-3 justify-center">
        <div 
          v-for="(item, index) in trendsData" 
          :key="index"
          class="flex items-center px-3 py-2 rounded-lg bg-gray-50 border border-gray-200"
        >
          <span class="text-sm font-medium text-gray-700 mr-2">{{ item.year }}</span>
          <span 
            v-if="item.yoyGrowthRate != null"
            :class="[
              'text-sm font-semibold',
              item.yoyGrowthRate >= 0 ? 'text-emerald-600' : 'text-red-600'
            ]"
          >
            <span v-if="item.yoyGrowthRate >= 0">↑</span>
            <span v-else>↓</span>
            {{ Math.abs(item.yoyGrowthRate).toFixed(1) }}%
          </span>
          <span v-else class="text-sm text-gray-400">—</span>
        </div>
      </div>

      <!-- Summary Statistics -->
      <div class="mt-6 grid grid-cols-4 gap-4 pt-4 border-t border-gray-100">
        <div class="text-center">
          <p class="text-2xl font-bold text-indigo-600">{{ formatNumber(totalExhibitions) }}</p>
          <p class="text-xs text-gray-500">Total Exhibitions</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-emerald-600">{{ formatNumber(totalArtworks) }}</p>
          <p class="text-xs text-gray-500">Total Artworks</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-amber-600">{{ formatCurrency(totalValue) }}</p>
          <p class="text-xs text-gray-500">Cumulative Value</p>
        </div>
        <div class="text-center">
          <p 
            class="text-2xl font-bold" 
            :class="avgGrowth >= 0 ? 'text-emerald-600' : 'text-red-600'"
          >
            {{ avgGrowth >= 0 ? '+' : '' }}{{ avgGrowth.toFixed(1) }}%
          </p>
          <p class="text-xs text-gray-500">Avg. YoY Growth</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import LineChart from '../reports/charts/LineChart.vue';
import { analyticsAPI } from '@/api/analyticsAPI';

/**
 * AcquisitionTrendsChart Component
 * Displays annual exhibition value trends with YoY growth
 */
export default {
  name: 'AcquisitionTrendsChart',

  components: {
    'line-chart': LineChart
  },

  props: {
    title: {
      type: String,
      default: 'Annual Exhibition Value Trends'
    },
    description: {
      type: String,
      default: 'Year-over-year evolution of exhibition activity and artwork value'
    },
    initialYears: {
      type: Number,
      default: 5
    }
  },

  data() {
    return {
      isLoading: false,
      error: null,
      selectedYears: this.initialYears,
      trendsData: []
    };
  },

  computed: {
    chartData() {
      if (!this.trendsData.length) {
        return { labels: [], datasets: [] };
      }

      return {
        labels: this.trendsData.map(d => d.year.toString()),
        datasets: [
          {
            label: 'Total Artwork Value',
            data: this.trendsData.map(d => d.totalArtworkValue),
            borderColor: 'rgb(99, 102, 241)',
            backgroundColor: 'rgba(99, 102, 241, 0.1)',
            fill: true,
            tension: 0.3,
            yAxisID: 'y',
            pointRadius: 6,
            pointHoverRadius: 8,
            pointBackgroundColor: 'rgb(99, 102, 241)',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2
          },
          {
            label: 'Exhibitions Count',
            data: this.trendsData.map(d => d.exhibitionsCount),
            borderColor: 'rgb(16, 185, 129)',
            backgroundColor: 'transparent',
            borderDash: [5, 5],
            tension: 0.3,
            yAxisID: 'y1',
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: 'rgb(16, 185, 129)'
          },
          {
            label: 'Artworks Count',
            data: this.trendsData.map(d => d.artworksCount),
            borderColor: 'rgb(245, 158, 11)',
            backgroundColor: 'transparent',
            borderDash: [3, 3],
            tension: 0.3,
            yAxisID: 'y1',
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: 'rgb(245, 158, 11)'
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
                const data = this.trendsData[index];
                const lines = [
                  `Avg Value: ${this.formatCurrency(data.averageArtworkValue)}`
                ];
                if (data.yoyGrowthRate != null) {
                  const sign = data.yoyGrowthRate >= 0 ? '+' : '';
                  lines.push(`YoY Growth: ${sign}${data.yoyGrowthRate.toFixed(1)}%`);
                }
                return lines;
              }
            }
          }
        },
        scales: {
          x: {
            title: {
              display: true,
              text: 'Year'
            },
            grid: {
              display: false
            }
          },
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            title: {
              display: true,
              text: 'Total Value ($)'
            },
            ticks: {
              callback: (value) => this.formatCompactCurrency(value)
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.05)'
            }
          },
          y1: {
            type: 'linear',
            display: true,
            position: 'right',
            title: {
              display: true,
              text: 'Count'
            },
            grid: {
              drawOnChartArea: false
            }
          }
        }
      };
    },

    totalExhibitions() {
      return this.trendsData.reduce((sum, d) => sum + d.exhibitionsCount, 0);
    },

    totalArtworks() {
      return this.trendsData.reduce((sum, d) => sum + d.artworksCount, 0);
    },

    totalValue() {
      return this.trendsData.reduce((sum, d) => sum + d.totalArtworkValue, 0);
    },

    avgGrowth() {
      const growthRates = this.trendsData
        .filter(d => d.yoyGrowthRate != null)
        .map(d => d.yoyGrowthRate);
      
      if (!growthRates.length) return 0;
      return growthRates.reduce((sum, r) => sum + r, 0) / growthRates.length;
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
        const response = await analyticsAPI.getAcquisitionTrends(this.selectedYears);
        this.trendsData = response.data?.data || response.data || [];
      } catch (err) {
        console.error('Error fetching trend data:', err);
        this.error = err.response?.data?.message || 'Failed to load trend data';
        this.trendsData = [];
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
    },

    formatCompactCurrency(value) {
      if (value >= 1000000) {
        return `$${(value / 1000000).toFixed(1)}M`;
      } else if (value >= 1000) {
        return `$${(value / 1000).toFixed(0)}K`;
      }
      return `$${value}`;
    }
  }
};
</script>

<style scoped>
.chart-container {
  min-height: 480px;
}
</style>
