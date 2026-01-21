<template>
  <!--
    HorizontalBarChart.vue - Horizontal Bar Chart Component
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
 * HorizontalBarChart Component
 * Wrapper for horizontal bar charts (bar with indexAxis: 'y')
 */
export default {
  name: 'HorizontalBarChart',

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
    showDataLabels: {
      type: Boolean,
      default: false
    }
  },

  computed: {
    mergedOptions() {
      return {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            enabled: true
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            grid: {
              display: true,
              color: 'rgba(0, 0, 0, 0.05)'
            },
            ticks: {
              font: {
                size: 11
              }
            }
          },
          y: {
            grid: {
              display: false
            },
            ticks: {
              font: {
                size: 12
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
