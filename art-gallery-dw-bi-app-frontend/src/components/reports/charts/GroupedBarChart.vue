<template>
  <!--
    GroupedBarChart.vue - Grouped/Stacked Bar Chart Component
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
 * GroupedBarChart Component
 * Wrapper for grouped or stacked bar charts with multiple datasets
 */
export default {
  name: 'GroupedBarChart',

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
    stacked: {
      type: Boolean,
      default: false
    },
    horizontal: {
      type: Boolean,
      default: false
    }
  },

  computed: {
    mergedOptions() {
      return {
        indexAxis: this.horizontal ? 'y' : 'x',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'top',
            labels: {
              usePointStyle: true,
              padding: 15,
              font: {
                size: 12
              }
            }
          },
          tooltip: {
            mode: 'index',
            intersect: false
          }
        },
        scales: {
          x: {
            stacked: this.stacked,
            grid: {
              display: this.horizontal,
              color: 'rgba(0, 0, 0, 0.05)'
            },
            ticks: {
              font: {
                size: 11
              }
            }
          },
          y: {
            stacked: this.stacked,
            beginAtZero: true,
            grid: {
              display: !this.horizontal,
              color: 'rgba(0, 0, 0, 0.05)'
            },
            ticks: {
              font: {
                size: 11
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
