<template>
  <!--
    DoughnutChart.vue - Doughnut Chart Component
    Art Gallery Management System - DW/BI Module
  -->
  <base-chart
    type="doughnut"
    :chart-data="chartData"
    :options="mergedOptions"
    :height="height"
    :is-loading="isLoading"
  />
</template>

<script>
import BaseChart from './BaseChart.vue';

/**
 * DoughnutChart Component
 * Wrapper for doughnut charts
 */
export default {
  name: 'DoughnutChart',

  components: {
    'base-chart': BaseChart
  },

  props: {
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
    },
    cutout: {
      type: String,
      default: '60%'
    }
  },

  computed: {
    mergedOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        cutout: this.cutout,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const label = context.label || '';
                const value = context.parsed || 0;
                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                const percentage = ((value / total) * 100).toFixed(1);
                return `${label}: ${value} (${percentage}%)`;
              }
            }
          }
        },
        ...this.options
      };
    }
  }
};
</script>
