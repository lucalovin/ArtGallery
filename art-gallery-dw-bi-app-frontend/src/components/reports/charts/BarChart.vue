<template>
  <!--
    BarChart.vue - Bar Chart Component
    Art Gallery Management System - DW/BI Module
  -->
  <base-chart
    type="bar"
    :chart-data="chartData"
    :options="mergedOptions"
    :height="height"
    :is-loading="isLoading"
  />
</template>

<script>
import BaseChart from './BaseChart.vue';

/**
 * BarChart Component
 * Wrapper for vertical bar charts
 */
export default {
  name: 'BarChart',

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
    }
  },

  computed: {
    mergedOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            mode: 'index',
            intersect: false
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            }
          },
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(0, 0, 0, 0.05)'
            }
          }
        },
        ...this.options
      };
    }
  }
};
</script>
