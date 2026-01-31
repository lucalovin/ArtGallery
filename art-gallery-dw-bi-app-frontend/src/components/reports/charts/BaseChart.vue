<template>
  <!--
    BaseChart.vue - Base Chart Component Wrapper
    Art Gallery Management System - DW/BI Module
  -->
  <div class="base-chart relative" :style="{ height: height + 'px' }">
    <canvas ref="chartCanvas"></canvas>
    
    <!-- Loading Overlay -->
    <div 
      v-if="isLoading" 
      class="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center"
    >
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
    </div>

    <!-- No Data State -->
    <div 
      v-if="!isLoading && !hasData" 
      class="absolute inset-0 flex items-center justify-center"
    >
      <div class="text-center">
        <svg class="w-12 h-12 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
        <p class="mt-2 text-sm text-gray-500">No data available</p>
      </div>
    </div>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js';

// Register all Chart.js components
Chart.register(...registerables);

/**
 * BaseChart Component
 * Base wrapper for Chart.js charts
 */
export default {
  name: 'BaseChart',

  props: {
    type: {
      type: String,
      required: true,
      validator: (value) => ['line', 'bar', 'pie', 'doughnut', 'radar', 'polarArea'].includes(value)
    },
    chartData: {
      type: Object,
      required: true
    },
    options: {
      type: Object,
      default: () => ({})
    },
    height: {
      type: Number,
      default: 300
    },
    isLoading: {
      type: Boolean,
      default: false
    }
  },

  data() {
    return {
      chart: null
    };
  },

  computed: {
    hasData() {
      if (!this.chartData || !this.chartData.datasets) return false;
      return this.chartData.datasets.some(ds => ds.data && ds.data.length > 0);
    },

    /**
     * Safe chart data that ensures valid structure for Chart.js
     * This prevents the "Cannot set properties of undefined (setting 'fullSize')" error
     */
    safeChartData() {
      // Return a valid chart structure even if data is empty/missing
      if (!this.chartData || !this.chartData.datasets || !this.hasData) {
        return {
          labels: [],
          datasets: [{
            data: [],
            backgroundColor: [],
            borderColor: []
          }]
        };
      }
      return this.chartData;
    },

    mergedOptions() {
      const baseOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: this.hasData,
            position: 'top',
            labels: {}
          },
          title: {
            display: false
          }
        }
      };
      
      // Deep merge options to ensure all plugin configurations are properly set
      return this.deepMerge(baseOptions, this.options || {});
    }
  },

  watch: {
    chartData: {
      handler(newVal, oldVal) {
        // Use nextTick to ensure DOM is ready and avoid race conditions
        this.$nextTick(() => {
          this.updateChart();
        });
      },
      deep: true
    },
    options: {
      handler() {
        // When options change significantly, recreate the chart to avoid state issues
        this.$nextTick(() => {
          this.recreateChart();
        });
      },
      deep: true
    },
    type() {
      this.recreateChart();
    }
  },

  mounted() {
    this.createChart();
  },

  beforeUnmount() {
    this.destroyChart();
  },

  methods: {
    /**
     * Deep merge helper to properly merge nested options objects
     */
    deepMerge(target, source) {
      const result = { ...target };
      
      for (const key in source) {
        if (source[key] && typeof source[key] === 'object' && !Array.isArray(source[key])) {
          result[key] = this.deepMerge(target[key] || {}, source[key]);
        } else {
          result[key] = source[key];
        }
      }
      
      return result;
    },

    createChart() {
      if (!this.$refs.chartCanvas) return;
      // Don't create chart if there's no valid data
      if (!this.hasData) return;

      const ctx = this.$refs.chartCanvas.getContext('2d');
      
      this.chart = new Chart(ctx, {
        type: this.type,
        data: this.safeChartData,
        options: this.mergedOptions
      });
    },

    updateChart() {
      if (!this.hasData) {
        // Destroy chart if data becomes empty
        this.destroyChart();
        return;
      }
      
      if (!this.chart) {
        this.createChart();
        return;
      }

      try {
        // Update data
        this.chart.data.labels = this.safeChartData.labels;
        this.chart.data.datasets = this.safeChartData.datasets;
        
        // Update with 'none' mode to skip animations and reduce state issues
        this.chart.update('none');
      } catch (error) {
        console.warn('Chart update failed, recreating chart:', error);
        this.recreateChart();
      }
    },

    recreateChart() {
      this.destroyChart();
      this.$nextTick(() => {
        this.createChart();
      });
    },

    destroyChart() {
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
      }
    }
  }
};
</script>
