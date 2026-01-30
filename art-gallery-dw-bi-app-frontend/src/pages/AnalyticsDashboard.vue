<template>
  <!--
    AnalyticsDashboard.vue - Analytics Dashboard Page
    Art Gallery Management System - DW/BI Module
    
    Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
    
    This dashboard displays 5 interactive charts mapped to the DW analytical queries:
    1. Top Artists by Artwork Count (Horizontal Bar Chart)
    2. Collection Value by Category (Doughnut Chart)
    3. Monthly Exhibition Activity (Line Chart)
    4. Location Distribution (Pie Chart)
    5. Annual Exhibition Trends (Line Chart with YoY growth)
  -->
  <div class="analytics-dashboard">
    <!-- Header -->
    <header class="mb-8">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Analytics Dashboard</h1>
          <p class="text-gray-500 mt-1">
            Data Warehouse insights and business intelligence reports
          </p>
        </div>
        <div class="flex items-center space-x-3">
          <span class="text-sm text-gray-500">
            Last updated: {{ formatDateTime(lastRefresh) }}
          </span>
          <button 
            @click="refreshAll" 
            :disabled="isRefreshing"
            class="inline-flex items-center px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors disabled:opacity-50"
          >
            <svg 
              class="w-4 h-4 mr-2" 
              :class="{ 'animate-spin': isRefreshing }"
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            {{ isRefreshing ? 'Refreshing...' : 'Refresh All' }}
          </button>
        </div>
      </div>

      <!-- Query Reference Cards -->
      <div class="mt-6 grid grid-cols-1 md:grid-cols-5 gap-3">
        <div 
          v-for="(query, index) in queries" 
          :key="index"
          class="bg-gradient-to-r from-indigo-50 to-purple-50 border border-indigo-100 rounded-lg p-3"
        >
          <div class="flex items-center space-x-2">
            <span class="flex-shrink-0 w-6 h-6 flex items-center justify-center rounded-full bg-indigo-600 text-white text-xs font-bold">
              {{ index + 1 }}
            </span>
            <p class="text-xs text-gray-700 line-clamp-2">{{ query }}</p>
          </div>
        </div>
      </div>
    </header>

    <!-- Charts Grid -->
    <div class="space-y-8">
      <!-- Row 1: Top Artists and Value by Category -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <top-artists-chart 
          ref="topArtistsChart"
          title="Query 1: Top Artists by Artwork Count"
          description="Show me the top 10 artists with the most artworks in the collection"
        />
        <value-by-category-chart 
          ref="valueByCategoryChart"
          title="Query 2: Collection Value by Category"
          description="Total estimated value broken down by art medium and collection type"
        />
      </div>

      <!-- Row 2: Monthly Activity (Full Width) -->
      <visitor-trends-chart 
        ref="visitorTrendsChart"
        title="Query 3: Monthly Exhibition Activity"
        description="Analyze exhibition performance: show monthly activity metrics for the past year"
      />

      <!-- Row 3: Location Distribution and Annual Trends -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <membership-distribution-chart 
          ref="membershipDistributionChart"
          title="Query 4: Gallery Location Distribution"
          description="Gallery occupancy rate and distribution of artworks across different locations"
        />
        <acquisition-trends-chart 
          ref="acquisitionTrendsChart"
          title="Query 5: Annual Exhibition Value Trends"
          description="How has the annual total artwork value evolved over the last 5 years?"
        />
      </div>
    </div>

    <!-- Technical Info Footer -->
    <footer class="mt-12 pt-6 border-t border-gray-200">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 text-sm text-gray-500">
        <div>
          <p class="font-medium text-gray-700">Data Warehouse Analytics</p>
          <p>Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries</p>
        </div>
        <div class="space-y-1 md:text-right">
          <p><strong>Backend:</strong> .NET 10 Web API with Clean Architecture</p>
          <p><strong>Database:</strong> Oracle Data Warehouse (DW Schema)</p>
          <p><strong>Frontend:</strong> Vue.js 3 with Chart.js</p>
        </div>
      </div>

      <!-- API Endpoints Reference -->
      <div class="mt-6 bg-gray-50 rounded-lg p-4">
        <p class="text-sm font-medium text-gray-700 mb-3">API Endpoints:</p>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2 text-xs font-mono">
          <code class="bg-white px-2 py-1 rounded border">GET /api/reports/analytics/top-artists</code>
          <code class="bg-white px-2 py-1 rounded border">GET /api/reports/analytics/value-by-category</code>
          <code class="bg-white px-2 py-1 rounded border">GET /api/reports/analytics/visitor-trends</code>
          <code class="bg-white px-2 py-1 rounded border">GET /api/reports/analytics/membership-distribution</code>
          <code class="bg-white px-2 py-1 rounded border">GET /api/reports/analytics/acquisition-trends</code>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
import TopArtistsChart from '@/components/analytics/TopArtistsChart.vue';
import ValueByCategoryChart from '@/components/analytics/ValueByCategoryChart.vue';
import VisitorTrendsChart from '@/components/analytics/VisitorTrendsChart.vue';
import MembershipDistributionChart from '@/components/analytics/MembershipDistributionChart.vue';
import AcquisitionTrendsChart from '@/components/analytics/AcquisitionTrendsChart.vue';

/**
 * AnalyticsDashboard Page Component
 * 
 * Displays 5 interactive charts for DW analytical queries
 * as required by Module 1 & 2, Requirement 10.
 */
export default {
  name: 'AnalyticsDashboard',

  components: {
    'top-artists-chart': TopArtistsChart,
    'value-by-category-chart': ValueByCategoryChart,
    'visitor-trends-chart': VisitorTrendsChart,
    'membership-distribution-chart': MembershipDistributionChart,
    'acquisition-trends-chart': AcquisitionTrendsChart
  },

  data() {
    return {
      isRefreshing: false,
      lastRefresh: new Date(),
      queries: [
        'Show me the top 10 artists with the most artworks in the collection',
        'What is the total estimated value broken down by art medium and collection type?',
        'Analyze exhibition performance: show monthly activity metrics for the past year',
        'What is the gallery occupancy rate and distribution of artworks across locations?',
        'Show the trend of exhibition activity: how has the annual total artwork value evolved?'
      ]
    };
  },

  methods: {
    async refreshAll() {
      this.isRefreshing = true;

      try {
        // Refresh all chart components
        const refreshPromises = [
          this.$refs.topArtistsChart?.fetchData?.(),
          this.$refs.valueByCategoryChart?.fetchData?.(),
          this.$refs.visitorTrendsChart?.fetchData?.(),
          this.$refs.membershipDistributionChart?.fetchData?.(),
          this.$refs.acquisitionTrendsChart?.fetchData?.()
        ].filter(Boolean);

        await Promise.all(refreshPromises);
        this.lastRefresh = new Date();
      } catch (error) {
        console.error('Error refreshing charts:', error);
      } finally {
        this.isRefreshing = false;
      }
    },

    formatDateTime(date) {
      return new Intl.DateTimeFormat('en-US', {
        dateStyle: 'medium',
        timeStyle: 'short'
      }).format(date);
    }
  }
};
</script>

<style scoped>
.analytics-dashboard {
  padding: 1.5rem;
  max-width: 1800px;
  margin: 0 auto;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

@media (max-width: 640px) {
  .analytics-dashboard {
    padding: 1rem;
  }
}
</style>
