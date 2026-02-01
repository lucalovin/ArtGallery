<template>
  <!--
    Reports.vue - BI Reports Page
    Art Gallery Management System - DW/BI Module
  -->
  <div class="reports-page">
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
      
      try {
        // Fetch KPI data from API
        const kpiResponse = await this.$api.reports?.getDashboardKPIs?.();
        if (kpiResponse?.data?.success && kpiResponse.data?.data) {
          const kpis = kpiResponse.data.data;
          this.kpiData = [
            { id: 1, label: 'Registered Visitors', value: kpis.totalVisitors || 0, trend: 0, icon: '👥', color: 'primary' },
            { id: 2, label: 'Insurance Coverage', value: kpis.totalInsuranceCoverage || 0, trend: 0, icon: '🛡️', color: 'success', format: 'currency' },
            { id: 3, label: 'Active Loans', value: kpis.activeLoans || 0, trend: 0, icon: '📋', color: 'info', format: 'number' },
            { id: 4, label: 'Total Artworks', value: kpis.totalArtworks || 0, trend: 0, icon: '🖼️', color: 'secondary', format: 'number' }
          ];
        }

        // Fetch visitor trends from API
        const trendsResponse = await this.$api.reports?.getVisitorTrends?.();
        if (trendsResponse?.data?.success && trendsResponse.data?.data) {
          const trends = trendsResponse.data.data;
          this.visitorTrends = {
            labels: trends.map(t => {
              const date = new Date(t.date);
              return date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
            }),
            datasets: [
              {
                label: 'New Registrations',
                data: trends.map(t => t.visitorCount || 0),
                borderColor: chartColors.purple,
                backgroundColor: withOpacity(chartColors.purple, 0.1),
                fill: true
              }
            ]
          };
        }

        // Fetch artwork distribution from API
        const distributionResponse = await this.$api.reports?.getArtworkDistribution?.();
        if (distributionResponse?.data?.success && distributionResponse.data?.data) {
          const dist = distributionResponse.data.data;
          this.categoryDistribution = {
            labels: dist.map(d => d.category || d.label),
            datasets: [
              {
                data: dist.map(d => d.count || d.value),
                backgroundColor: chartColorPalettes.purple
              }
            ]
          };
        }

        // Fetch exhibition performance from API
        const exhibitionResponse = await this.$api.reports?.getExhibitionPerformance?.();
        if (exhibitionResponse?.data?.success && exhibitionResponse.data?.data) {
          this.exhibitionPerformance = exhibitionResponse.data.data.slice(0, 4).map((ex, i) => ({
            id: ex.exhibitionId || i + 1,
            name: ex.title || ex.name,
            visitors: ex.actualVisitors || ex.expectedVisitors || 0,
            revenue: ex.budget || 0,
            rating: ex.performanceRatio ? (ex.performanceRatio * 5).toFixed(1) : 0
          }));
        }

        // Fetch revenue data from API (insurance coverage over time)
        const revenueResponse = await this.$api.reports?.getRevenueByPeriod?.({ period: 'monthly' });
        if (revenueResponse?.data?.success && revenueResponse.data?.data) {
          const revenueItems = revenueResponse.data.data;
          this.revenueData = {
            labels: revenueItems.map(r => {
              const date = new Date(r.period);
              return date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
            }),
            datasets: [
              {
                label: 'Coverage Value ($)',
                data: revenueItems.map(r => r.totalRevenue || 0),
                backgroundColor: chartColorPalettes.purple
              }
            ]
          };
        } else {
          // Empty state if no data
          this.revenueData = {
            labels: [],
            datasets: []
          };
        }

      } catch (error) {
        console.error('Failed to load report data:', error);
      } finally {
        this.isLoading = false;
      }
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
