<template>
  <!--
    Reports.vue - BI Reports Page
    Art Gallery Management System - DW/BI Module
  -->
  <div class="reports-page">
    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">BI Reports & Analytics</h1>
      <p class="text-gray-500 mt-1">Business Intelligence dashboards and data visualizations</p>
    </header>

    <!-- Reports Dashboard Component -->
    <reports-dashboard
      :kpi-data="kpiData"
      :visitor-trends="visitorTrends"
      :revenue-data="revenueData"
      :category-distribution="categoryDistribution"
      :exhibition-performance="exhibitionPerformance"
      :is-loading="isLoading"
      :selected-period="selectedPeriod"
      @period-change="handlePeriodChange"
      @export-report="handleExportReport"
    />
  </div>
</template>

<script>
import ReportsDashboard from '@/components/reports/ReportsDashboard.vue';
import { chartColors, chartColorPalettes, withOpacity } from '@/utils/colors';

/**
 * Reports Page
 */
export default {
  name: 'ReportsPage',

  components: {
    'reports-dashboard': ReportsDashboard
  },

  data() {
    return {
      isLoading: true,
      selectedPeriod: 'month',
      kpiData: [],
      visitorTrends: {},
      revenueData: {},
      categoryDistribution: {},
      exhibitionPerformance: []
    };
  },

  created() {
    this.loadReportData();
  },

  methods: {
    async loadReportData() {
      this.isLoading = true;
      await new Promise(resolve => setTimeout(resolve, 800));

      // KPI Data
      this.kpiData = [
        { id: 1, label: 'Total Visitors', value: 45230, trend: 12.5, icon: '👥', color: 'primary' },
        { id: 2, label: 'Revenue', value: 892500, trend: 8.3, icon: '💰', color: 'success', format: 'currency' },
        { id: 3, label: 'Avg. Visit Duration', value: 2.4, trend: -3.2, icon: '⏱️', color: 'info', format: 'decimal' },
        { id: 4, label: 'Member Conversion', value: 15.8, trend: 5.7, icon: '🎯', color: 'secondary', format: 'percentage' }
      ];

      // Visitor Trends (Line Chart)
      this.visitorTrends = {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [
          {
            label: 'Visitors',
            data: [3200, 3800, 4100, 4500, 5200, 6100, 5800, 5400, 4800, 4200, 3600, 4500],
            borderColor: chartColors.purple,
            backgroundColor: withOpacity(chartColors.purple, 0.1),
            fill: true
          }
        ]
      };

      // Revenue Data (Bar Chart)
      this.revenueData = {
        labels: ['Tickets', 'Memberships', 'Gift Shop', 'Events', 'Loans', 'Donations'],
        datasets: [
          {
            label: 'Revenue ($)',
            data: [345000, 125000, 89000, 156000, 98000, 79500],
            backgroundColor: chartColorPalettes.purple
          }
        ]
      };

      // Category Distribution (Doughnut Chart)
      this.categoryDistribution = {
        labels: ['Paintings', 'Sculptures', 'Photography', 'Drawings', 'Mixed Media', 'Digital Art'],
        datasets: [
          {
            data: [420, 185, 210, 165, 142, 125],
            backgroundColor: chartColorPalettes.purple
          }
        ]
      };

      // Exhibition Performance
      this.exhibitionPerformance = [
        { id: 1, name: 'Modern Masters', visitors: 12450, revenue: 156250, rating: 4.8 },
        { id: 2, name: 'Renaissance Revival', visitors: 8920, revenue: 111500, rating: 4.6 },
        { id: 3, name: 'Contemporary Visions', visitors: 7650, revenue: 95625, rating: 4.4 },
        { id: 4, name: 'Photography Now', visitors: 6200, revenue: 77500, rating: 4.5 }
      ];

      this.isLoading = false;
    },

    handlePeriodChange(period) {
      this.selectedPeriod = period;
      this.loadReportData();
    },

    handleExportReport(format) {
      console.log('Exporting report in format:', format);
    }
  }
};
</script>

<style scoped>
.reports-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
