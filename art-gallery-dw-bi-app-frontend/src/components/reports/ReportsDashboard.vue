<template>
  <!--
    ReportsDashboard.vue - BI Reports Dashboard Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="reports-dashboard">
    <!-- Header -->
    <div class="mb-8 flex flex-col md:flex-row md:items-center md:justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Business Intelligence</h1>
        <p class="text-gray-600 mt-2">
          Analytics and insights from the Art Gallery Data Warehouse
        </p>
      </div>
      <div class="mt-4 md:mt-0 flex items-center space-x-3">
        <!-- Date Range Selector -->
        <div class="flex items-center space-x-2">
          <label class="text-sm text-gray-600">Period:</label>
          <select v-model="selectedPeriod" class="form-input text-sm">
            <option value="7d">Last 7 Days</option>
            <option value="30d">Last 30 Days</option>
            <option value="90d">Last 90 Days</option>
            <option value="ytd">Year to Date</option>
            <option value="custom">Custom Range</option>
          </select>
        </div>
        <!-- Refresh Button -->
        <button @click="refreshData" class="btn btn-secondary" :disabled="isLoading">
          <svg 
            class="w-4 h-4 mr-2" 
            :class="{ 'animate-spin': isLoading }"
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Refresh
        </button>
        <!-- Export Button -->
        <button @click="exportReport" class="btn btn-primary">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Export
        </button>
      </div>
    </div>

    <!-- KPI Cards Row -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <kpi-card
        v-for="kpi in kpiData"
        :key="kpi.id"
        :label="kpi.title"
        :value="kpi.value"
        :trend="kpi.change"
        :icon="kpi.icon"
        :color="kpi.color"
        :format="kpi.format"
      />
    </div>

    <!-- Charts Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
      <!-- Revenue Over Time Chart -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-gray-900">Insurance Coverage Value</h3>
          <select v-model="revenueChartType" class="form-input text-sm w-auto">
            <option value="line">Line Chart</option>
            <option value="bar">Bar Chart</option>
            <option value="area">Area Chart</option>
          </select>
        </div>
        <line-chart
          :chart-data="revenueChartData"
          :options="revenueChartOptions"
          :height="300"
        />
      </div>

      <!-- Visitor Statistics Chart -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-gray-900">New Visitor Registrations</h3>
          <select v-model="visitorMetric" class="form-input text-sm w-auto">
            <option value="monthly">Monthly</option>
          </select>
        </div>
        <bar-chart
          :chart-data="visitorChartData"
          :options="visitorChartOptions"
          :height="300"
        />
      </div>

      <!-- Artwork Distribution Chart -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-gray-900">Artwork by Category</h3>
        </div>
        <div class="flex items-center">
          <div class="w-2/3">
            <doughnut-chart
              :chart-data="artworkCategoryData"
              :options="doughnutOptions"
              :height="250"
            />
          </div>
          <div class="w-1/3 pl-4">
            <div 
              v-for="(category, index) in artworkCategories" 
              :key="category.name"
              class="flex items-center mb-2"
            >
              <span 
                class="w-3 h-3 rounded-full mr-2"
                :style="{ backgroundColor: categoryColors[index] }"
              ></span>
              <span class="text-sm text-gray-600">{{ category.name }}</span>
              <span class="text-sm font-medium text-gray-900 ml-auto">{{ category.count }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Top Performing Exhibitions -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-gray-900">Top Exhibitions</h3>
          <button @click="viewAllExhibitions" class="text-sm text-primary-600 hover:text-primary-700">
            View All
          </button>
        </div>
        <div class="space-y-4">
          <div 
            v-for="(exhibition, index) in topExhibitions" 
            :key="exhibition.id"
            class="flex items-center"
          >
            <span class="w-6 h-6 rounded-full bg-primary-100 text-primary-600 text-sm font-medium flex items-center justify-center mr-3">
              {{ index + 1 }}
            </span>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">{{ exhibition.name }}</p>
              <p class="text-xs text-gray-500">{{ exhibition.visitors }} visitors</p>
            </div>
            <div class="text-right">
              <p class="text-sm font-medium text-gray-900" v-currency="exhibition.revenue"></p>
              <p class="text-xs" :class="exhibition.trend > 0 ? 'text-green-600' : 'text-red-600'">
                {{ exhibition.trend > 0 ? '+' : '' }}{{ exhibition.trend }}%
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Additional Analytics Section -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
      <!-- Artworks by Collection -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">Artworks by Collection</h3>
        <horizontal-bar-chart
          :chart-data="salesByArtistData"
          :options="horizontalBarOptions"
          :height="250"
        />
      </div>

      <!-- Exhibition Visitors -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">Exhibition Distribution</h3>
        <pie-chart
          :chart-data="revenueSourceData"
          :options="pieOptions"
          :height="250"
        />
      </div>

      <!-- Monthly Loan Activity -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">Quarterly Coverage</h3>
        <grouped-bar-chart
          :chart-data="yoyComparisonData"
          :options="groupedBarOptions"
          :height="250"
        />
      </div>
    </div>

    <!-- Data Table Section -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900">Recent Loan Activity</h3>
        <div class="flex items-center space-x-3">
          <input
            type="text"
            v-model="tableSearch"
            placeholder="Search loans..."
            class="form-input text-sm w-64"
          />
          <select v-model="tableFilter" class="form-input text-sm">
            <option value="all">All Status</option>
            <option value="active">Active</option>
            <option value="completed">Completed</option>
          </select>
        </div>
      </div>
      <report-table
        :data="filteredTransactions"
        :columns="transactionColumns"
        :is-loading="isLoadingTable"
        @row-click="viewTransactionDetails"
      />
    </div>
  </div>
</template>

<script>
import { mapState, mapActions, mapGetters } from 'vuex';
import KPICard from './KPICard.vue';
import LineChart from './charts/LineChart.vue';
import BarChart from './charts/BarChart.vue';
import DoughnutChart from './charts/DoughnutChart.vue';
import PieChart from './charts/PieChart.vue';
import HorizontalBarChart from './charts/HorizontalBarChart.vue';
import GroupedBarChart from './charts/GroupedBarChart.vue';
import ReportTable from './ReportTable.vue';
import { chartColors, chartColorPalettes, withOpacity } from '@/utils/colors';

/**
 * ReportsDashboard Component
 * Main BI dashboard with charts and analytics
 */
export default {
  name: 'ReportsDashboard',

  components: {
    'kpi-card': KPICard,
    'line-chart': LineChart,
    'bar-chart': BarChart,
    'doughnut-chart': DoughnutChart,
    'pie-chart': PieChart,
    'horizontal-bar-chart': HorizontalBarChart,
    'grouped-bar-chart': GroupedBarChart,
    'report-table': ReportTable
  },

  data() {
    return {
      isLoading: false,
      isLoadingTable: false,
      selectedPeriod: '30d',
      revenueChartType: 'line',
      visitorMetric: 'monthly',
      tableSearch: '',
      tableFilter: 'all',

      // KPI Data - initialized with structure, values populated from API
      kpiData: [
        {
          id: 1,
          title: 'Insurance Coverage',
          value: 0,
          change: 0,
          changeType: 'neutral',
          icon: '🛡️',
          color: 'green',
          format: 'currency'
        },
        {
          id: 2,
          title: 'Registered Visitors',
          value: 0,
          change: 0,
          changeType: 'neutral',
          icon: '👥',
          color: 'blue',
          format: 'number'
        },
        {
          id: 3,
          title: 'Artworks in Collection',
          value: 0,
          change: 0,
          changeType: 'neutral',
          icon: '🖼️',
          color: 'purple',
          format: 'number'
        },
        {
          id: 4,
          title: 'Active Exhibitions',
          value: 0,
          change: 0,
          changeType: 'neutral',
          icon: '🎨',
          color: 'yellow',
          format: 'number'
        }
      ],

      // Chart Colors
      categoryColors: chartColorPalettes.standard,

      // Artwork Categories (populated from API)
      artworkCategories: [],

      // Top Exhibitions (populated from API)
      topExhibitions: [],

      // Revenue data from API
      revenueData: [],

      // Visitor trends data from API
      visitorTrendsData: [],

      // Transaction Columns
      transactionColumns: [
        { key: 'id', label: 'Loan ID', sortable: true },
        { key: 'date', label: 'Start Date', sortable: true },
        { key: 'type', label: 'Type', sortable: true },
        { key: 'description', label: 'Artwork → Exhibitor', sortable: false },
        { key: 'amount', label: 'Insured Value', sortable: true, format: 'currency' },
        { key: 'status', label: 'Status', sortable: true }
      ],

      // Transactions (populated from API - loans/activities)
      transactions: [],

      // Chart Options
      revenueChartOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => '$' + value.toLocaleString()
            }
          }
        }
      },

      visitorChartOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        }
      },

      doughnutOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        },
        cutout: '60%'
      },

      pieOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom' }
        }
      },

      horizontalBarOptions: {
        responsive: true,
        maintainAspectRatio: false,
        indexAxis: 'y',
        plugins: {
          legend: { display: false }
        }
      },

      groupedBarOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top' }
        }
      }
    };
  },

  computed: {
    revenueChartData() {
      // Uses data fetched from API stored in revenueData
      if (!this.revenueData || this.revenueData.length === 0) {
        return { labels: [], datasets: [] };
      }
      return {
        labels: this.revenueData.map(r => {
          const date = new Date(r.period);
          return date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
        }),
        datasets: [{
          label: 'Coverage Value ($)',
          data: this.revenueData.map(r => r.totalRevenue || 0),
          borderColor: chartColors.blue,
          backgroundColor: withOpacity(chartColors.blue, 0.1),
          fill: true,
          tension: 0.4
        }]
      };
    },

    visitorChartData() {
      // Uses data fetched from API stored in visitorTrendsData
      if (!this.visitorTrendsData || this.visitorTrendsData.length === 0) {
        return { labels: [], datasets: [] };
      }
      return {
        labels: this.visitorTrendsData.map(v => {
          const date = new Date(v.date);
          return date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
        }),
        datasets: [{
          label: 'New Registrations',
          data: this.visitorTrendsData.map(v => v.visitorCount || 0),
          backgroundColor: chartColors.emerald
        }]
      };
    },

    artworkCategoryData() {
      if (!this.artworkCategories || this.artworkCategories.length === 0) {
        return { labels: [], datasets: [] };
      }
      return {
        labels: this.artworkCategories.map(c => c.name),
        datasets: [{
          data: this.artworkCategories.map(c => c.count),
          backgroundColor: this.categoryColors
        }]
      };
    },

    // Artworks by Collection - uses artworkCategories data
    salesByArtistData() {
      if (!this.artworkCategories || this.artworkCategories.length === 0) {
        return { labels: [], datasets: [] };
      }
      return {
        labels: this.artworkCategories.slice(0, 5).map(c => c.name),
        datasets: [{
          label: 'Artworks',
          data: this.artworkCategories.slice(0, 5).map(c => c.count),
          backgroundColor: chartColors.violet
        }]
      };
    },

    // Exhibition distribution - shows visitor estimates from exhibitions
    revenueSourceData() {
      if (!this.topExhibitions || this.topExhibitions.length === 0) {
        return { labels: [], datasets: [] };
      }
      return {
        labels: this.topExhibitions.slice(0, 5).map(ex => ex.name),
        datasets: [{
          data: this.topExhibitions.slice(0, 5).map(ex => ex.visitors || 0),
          backgroundColor: chartColorPalettes.standard.slice(0, 5)
        }]
      };
    },

    // Quarterly coverage - derived from revenue data grouped by quarter
    yoyComparisonData() {
      if (!this.revenueData || this.revenueData.length === 0) {
        return { labels: [], datasets: [] };
      }
      // Group revenue data into quarterly summaries
      const quarters = ['Q1', 'Q2', 'Q3', 'Q4'];
      const currentYear = new Date().getFullYear();
      const quarterData = [0, 0, 0, 0];
      
      this.revenueData.forEach(r => {
        const date = new Date(r.period);
        const quarterIndex = Math.floor(date.getMonth() / 3);
        quarterData[quarterIndex] += r.totalRevenue || 0;
      });
      
      return {
        labels: quarters,
        datasets: [
          {
            label: `${currentYear} Coverage`,
            data: quarterData,
            backgroundColor: chartColors.blue
          }
        ]
      };
    },

    filteredTransactions() {
      let filtered = [...this.transactions];

      if (this.tableFilter !== 'all') {
        filtered = filtered.filter(t => t.type === this.tableFilter);
      }

      if (this.tableSearch) {
        const search = this.tableSearch.toLowerCase();
        filtered = filtered.filter(t => 
          t.id.toLowerCase().includes(search) ||
          t.description.toLowerCase().includes(search)
        );
      }

      return filtered;
    }
  },

  watch: {
    selectedPeriod() {
      this.refreshData();
    }
  },

  created() {
    this.loadDashboardData();
  },

  methods: {
    ...mapActions('reports', ['fetchReportData']),

    async loadDashboardData() {
      this.isLoading = true;
      this.isLoadingTable = true;
      try {
        // Fetch all dashboard data in parallel
        const [kpiResponse, exhibitionResponse, artworkDistResponse, loansResponse, revenueResponse, visitorResponse] = await Promise.all([
          this.$api.reports.getDashboardKPIs(),
          this.$api.reports.getExhibitionPerformance?.() || Promise.resolve(null),
          this.$api.reports.getArtworkDistribution?.() || Promise.resolve(null),
          this.$api.loans.getAll?.() || Promise.resolve(null),
          this.$api.reports.getRevenueByPeriod?.({ period: 'monthly' }) || Promise.resolve(null),
          this.$api.reports.getVisitorTrends?.() || Promise.resolve(null)
        ]);

        // Update KPI data
        if (kpiResponse.data?.success && kpiResponse.data?.data) {
          const kpis = kpiResponse.data.data;
          if (kpis.totalInsuranceCoverage !== undefined) this.kpiData[0].value = kpis.totalInsuranceCoverage;
          if (kpis.totalVisitors !== undefined) this.kpiData[1].value = kpis.totalVisitors;
          // Use total artworks in collection
          if (kpis.totalArtworks !== undefined) this.kpiData[2].value = kpis.totalArtworks;
          if (kpis.activeExhibitions !== undefined) this.kpiData[3].value = kpis.activeExhibitions;
        }

        // Update exhibition performance
        if (exhibitionResponse?.data?.success && exhibitionResponse.data?.data) {
          this.topExhibitions = exhibitionResponse.data.data.slice(0, 5).map((ex, i) => ({
            id: ex.exhibitionId || ex.id || i + 1,
            name: ex.title || ex.exhibitionName || ex.name || 'Exhibition',
            visitors: ex.actualVisitors || ex.expectedVisitors || ex.visitorCount || ex.visitors || 0,
            revenue: ex.budget || ex.revenue || 0,
            trend: ex.performanceRatio ? Math.round((ex.performanceRatio - 1) * 100) : 0
          }));
        }

        // Update artwork distribution (categories)
        if (artworkDistResponse?.data?.success && artworkDistResponse.data?.data) {
          this.artworkCategories = artworkDistResponse.data.data.map(cat => ({
            name: cat.category || cat.medium || cat.name || 'Other',
            count: cat.count || cat.artworkCount || 0
          }));
        }

        // Update revenue data for charts
        if (revenueResponse?.data?.success && revenueResponse.data?.data) {
          this.revenueData = revenueResponse.data.data;
        }

        // Update visitor trends data for charts
        if (visitorResponse?.data?.success && visitorResponse.data?.data) {
          this.visitorTrendsData = visitorResponse.data.data;
        }

        // Update transactions from loans (showing recent loan activity)
        if (loansResponse?.data?.success && loansResponse.data?.data) {
          const loansData = loansResponse.data.data.items || loansResponse.data.data;
          if (Array.isArray(loansData)) {
            this.transactions = loansData.slice(0, 10).map((loan, i) => ({
              id: `LOAN-${loan.id || i + 1}`,
              date: loan.startDate ? loan.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
              type: 'loan',
              description: `${loan.artworkTitle || 'Artwork'} → ${loan.exhibitorName || 'Exhibitor'}`,
              amount: loan.insuranceValue || 0,
              status: loan.endDate ? 'completed' : 'active'
            }));
          }
        }
      } catch (error) {
        console.error('Failed to load dashboard data:', error);
      } finally {
        this.isLoading = false;
        this.isLoadingTable = false;
      }
    },

    async refreshData() {
      this.isLoading = true;
      try {
        // Refresh from backend API
        await this.loadDashboardData();
      } catch (error) {
        console.error('Failed to refresh data:', error);
      } finally {
        this.isLoading = false;
      }
    },

    exportReport() {
      alert('Exporting report as PDF...');
    },

    viewAllExhibitions() {
      this.$router.push({ name: 'ExhibitionInventory' });
    },

    viewTransactionDetails(transaction) {
      console.log('View transaction:', transaction);
    }
  }
};
</script>
