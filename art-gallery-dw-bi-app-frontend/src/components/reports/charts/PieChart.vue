<template>
  <!--
    PieChart.vue - Pie Chart Component
    Art Gallery Management System - DW/BI Module
  -->
  <base-chart
    type="pie"
    :chart-data="chartData"
    :options="mergedOptions"
    :height="height"
    :is-loading="isLoading"
  />
</template>

<script>
import BaseChart from './BaseChart.vue';

/**
 * PieChart Component
 * Wrapper for pie charts with customizable options
 */
export default {
  name: 'PieChart',

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
    showLabels: {
      type: Boolean,
      default: true
    }
  },

  computed: {
    mergedOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: this.showLabels,
            position: 'bottom',
            labels: {
              padding: 15,
              usePointStyle: true,
              font: {
                size: 12
              }
            }
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
