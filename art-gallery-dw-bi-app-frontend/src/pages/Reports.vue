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
            { id: 1, label: 'Total Visitors', value: kpis.totalVisitors || 0, trend: kpis.visitorTrend || 0, icon: '👥', color: 'primary' },
            { id: 2, label: 'Revenue', value: kpis.totalRevenue || 0, trend: kpis.revenueTrend || 0, icon: '💰', color: 'success', format: 'currency' },
            { id: 3, label: 'Avg. Visit Duration', value: kpis.avgVisitDuration || 0, trend: kpis.durationTrend || 0, icon: '⏱️', color: 'info', format: 'decimal' },
            { id: 4, label: 'Member Conversion', value: kpis.memberConversion || 0, trend: kpis.conversionTrend || 0, icon: '🎯', color: 'secondary', format: 'percentage' }
          ];
        }

        // Fetch visitor trends from API
        const trendsResponse = await this.$api.reports?.getVisitorTrends?.();
        if (trendsResponse?.data?.success && trendsResponse.data?.data) {
          const trends = trendsResponse.data.data;
          this.visitorTrends = {
            labels: trends.map(t => t.month || t.label),
            datasets: [
              {
                label: 'Visitors',
                data: trends.map(t => t.visitorCount || t.value),
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
            id: ex.id || i + 1,
            name: ex.exhibitionName || ex.name,
            visitors: ex.visitorCount || ex.visitors || 0,
            revenue: ex.revenue || 0,
            rating: ex.rating || 0
          }));
        }

        // Set default revenue data (could also be fetched from API)
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
