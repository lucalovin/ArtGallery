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
        <!-- Emoji icons -->
        <span v-if="isEmojiIcon" class="text-xl">{{ icon }}</span>
        <!-- SVG icons -->
        <svg 
          v-else
          class="w-6 h-6" 
          :class="iconColorClass"
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path 
            stroke-linecap="round" 
            stroke-linejoin="round" 
            stroke-width="2" 
            :d="iconPath"
          />
        </svg>
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
    isEmojiIcon() {
      // Check if the icon is an emoji (contains emoji unicode ranges) or not a known icon name
      const emojiRegex = /[\u{1F300}-\u{1F9FF}]|[\u{2600}-\u{26FF}]|[\u{2700}-\u{27BF}]/u;
      return emojiRegex.test(this.icon) || !this.iconPaths[this.icon];
    },

    iconPaths() {
      return {
        'currency-dollar': 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
        'users': 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
        'photograph': 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z',
        'calendar': 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
        'chart-bar': 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
        'trending-up': 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6',
        'clock': 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',
        'star': 'M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z',
        'eye': 'M15 12a3 3 0 11-6 0 3 3 0 016 0z M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z',
        'ticket': 'M15 5v2m0 4v2m0 4v2M5 5a2 2 0 00-2 2v3a2 2 0 110 4v3a2 2 0 002 2h14a2 2 0 002-2v-3a2 2 0 110-4V7a2 2 0 00-2-2H5z'
      };
    },

    iconPath() {
      return this.iconPaths[this.icon] || 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z';
    },

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
