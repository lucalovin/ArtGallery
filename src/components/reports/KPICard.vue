<template>
  <!--
    KPICard.vue - Key Performance Indicator Card Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="kpi-card bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition-shadow duration-300">
    <!-- Header with Icon -->
    <div class="flex items-center justify-between mb-4">
      <div 
        class="w-12 h-12 rounded-lg flex items-center justify-center"
        :class="iconBackgroundClass"
      >
        <span class="text-xl" :class="iconColorClass">{{ icon }}</span>
      </div>
      
      <!-- Trend Badge -->
      <div 
        v-if="showTrend && trend !== 0"
        class="flex items-center px-2 py-1 rounded-full text-xs font-medium"
        :class="trendClasses"
      >
        <span class="mr-1">{{ trendIcon }}</span>
        <span>{{ formattedTrend }}</span>
      </div>
    </div>

    <!-- Value -->
    <div class="mb-2">
      <span 
        v-if="isLoading"
        class="inline-block w-24 h-8 bg-gray-200 rounded animate-pulse"
      ></span>
      <span 
        v-else
        class="text-3xl font-bold text-gray-900"
      >
        {{ formattedValue }}
      </span>
    </div>

    <!-- Label -->
    <p class="text-sm text-gray-500 font-medium">{{ label }}</p>

    <!-- Subtitle / Comparison -->
    <p 
      v-if="subtitle"
      class="text-xs text-gray-400 mt-1"
    >
      {{ subtitle }}
    </p>

    <!-- Progress Bar (optional) -->
    <div 
      v-if="showProgress && progress !== null"
      class="mt-4"
    >
      <div class="flex items-center justify-between text-xs mb-1">
        <span class="text-gray-500">Progress</span>
        <span class="font-medium text-gray-700">{{ progress }}%</span>
      </div>
      <div class="w-full h-2 bg-gray-100 rounded-full overflow-hidden">
        <div 
          class="h-full rounded-full transition-all duration-500"
          :class="progressBarClass"
          :style="{ width: `${Math.min(progress, 100)}%` }"
        ></div>
      </div>
    </div>

    <!-- Sparkline placeholder -->
    <div 
      v-if="sparklineData && sparklineData.length > 0"
      class="mt-4 h-12"
    >
      <svg 
        class="w-full h-full" 
        :viewBox="`0 0 ${sparklineData.length * 10} 40`"
        preserveAspectRatio="none"
      >
        <path
          :d="sparklinePath"
          fill="none"
          :stroke="sparklineColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
      </svg>
    </div>
  </div>
</template>

<script>
import { kpiVariantColors } from '@/utils/colors';

/**
 * KPICard Component
 * Displays key performance indicator with trend, progress, and sparkline
 */
export default {
  name: 'KPICard',

  props: {
    value: {
      type: [Number, String],
      required: true
    },
    label: {
      type: String,
      required: true
    },
    icon: {
      type: String,
      default: '📊'
    },
    color: {
      type: String,
      default: 'primary',
      validator: (value) => ['primary', 'secondary', 'success', 'warning', 'danger', 'info'].includes(value)
    },
    trend: {
      type: Number,
      default: 0
    },
    showTrend: {
      type: Boolean,
      default: true
    },
    trendLabel: {
      type: String,
      default: 'vs last period'
    },
    subtitle: {
      type: String,
      default: ''
    },
    format: {
      type: String,
      default: 'number',
      validator: (value) => ['number', 'currency', 'percentage', 'decimal'].includes(value)
    },
    currencySymbol: {
      type: String,
      default: '$'
    },
    decimals: {
      type: Number,
      default: 0
    },
    progress: {
      type: Number,
      default: null
    },
    showProgress: {
      type: Boolean,
      default: false
    },
    sparklineData: {
      type: Array,
      default: () => []
    },
    isLoading: {
      type: Boolean,
      default: false
    }
  },

  computed: {
    formattedValue() {
      if (typeof this.value === 'string') {
        return this.value;
      }

      const num = Number(this.value);
      
      switch (this.format) {
        case 'currency':
          return `${this.currencySymbol}${this.formatNumber(num)}`;
        case 'percentage':
          return `${num.toFixed(this.decimals)}%`;
        case 'decimal':
          return num.toFixed(this.decimals);
        default:
          return this.formatNumber(num);
      }
    },

    formattedTrend() {
      const sign = this.trend > 0 ? '+' : '';
      return `${sign}${this.trend.toFixed(1)}%`;
    },

    trendIcon() {
      if (this.trend > 0) return '↑';
      if (this.trend < 0) return '↓';
      return '→';
    },

    trendClasses() {
      if (this.trend > 0) {
        return 'bg-green-100 text-green-700';
      } else if (this.trend < 0) {
        return 'bg-red-100 text-red-700';
      }
      return 'bg-gray-100 text-gray-600';
    },

    iconBackgroundClass() {
      const colorMap = {
        primary: 'bg-primary-100',
        secondary: 'bg-secondary-100',
        success: 'bg-green-100',
        warning: 'bg-yellow-100',
        danger: 'bg-red-100',
        info: 'bg-blue-100'
      };
      return colorMap[this.color] || colorMap.primary;
    },

    iconColorClass() {
      const colorMap = {
        primary: 'text-primary-600',
        secondary: 'text-secondary-600',
        success: 'text-green-600',
        warning: 'text-yellow-600',
        danger: 'text-red-600',
        info: 'text-blue-600'
      };
      return colorMap[this.color] || colorMap.primary;
    },

    progressBarClass() {
      const colorMap = {
        primary: 'bg-primary-500',
        secondary: 'bg-secondary-500',
        success: 'bg-green-500',
        warning: 'bg-yellow-500',
        danger: 'bg-red-500',
        info: 'bg-blue-500'
      };
      return colorMap[this.color] || colorMap.primary;
    },

    sparklineColor() {
      return kpiVariantColors[this.color] || kpiVariantColors.primary;
    },

    sparklinePath() {
      if (!this.sparklineData || this.sparklineData.length < 2) {
        return '';
      }

      const data = this.sparklineData;
      const max = Math.max(...data);
      const min = Math.min(...data);
      const range = max - min || 1;
      
      const points = data.map((value, index) => {
        const x = index * 10;
        const y = 40 - ((value - min) / range) * 35 - 2.5;
        return `${x},${y}`;
      });

      return `M ${points.join(' L ')}`;
    }
  },

  methods: {
    formatNumber(num) {
      if (num >= 1000000) {
        return (num / 1000000).toFixed(1) + 'M';
      } else if (num >= 1000) {
        return (num / 1000).toFixed(1) + 'K';
      }
      return num.toLocaleString('en-US', { maximumFractionDigits: this.decimals });
    }
  }
};
</script>

<style scoped>
.kpi-card {
  min-width: 200px;
}
</style>
