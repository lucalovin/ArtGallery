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
        :title="kpi.title"
        :value="kpi.value"
        :change="kpi.change"
        :change-type="kpi.changeType"
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
          <h3 class="text-lg font-semibold text-gray-900">Revenue Trends</h3>
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
          <h3 class="text-lg font-semibold text-gray-900">Visitor Statistics</h3>
          <select v-model="visitorMetric" class="form-input text-sm w-auto">
            <option value="daily">Daily</option>
            <option value="weekly">Weekly</option>
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
      <!-- Sales by Artist -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">Sales by Artist</h3>
        <horizontal-bar-chart
          :chart-data="salesByArtistData"
          :options="horizontalBarOptions"
          :height="250"
        />
      </div>

      <!-- Revenue by Source -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">Revenue Sources</h3>
        <pie-chart
          :chart-data="revenueSourceData"
          :options="pieOptions"
          :height="250"
        />
      </div>

      <!-- Monthly Comparison -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-6">YoY Comparison</h3>
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
        <h3 class="text-lg font-semibold text-gray-900">Recent Transactions</h3>
        <div class="flex items-center space-x-3">
          <input
            type="text"
            v-model="tableSearch"
            placeholder="Search transactions..."
            class="form-input text-sm w-64"
          />
          <select v-model="tableFilter" class="form-input text-sm">
            <option value="all">All Types</option>
            <option value="sale">Sales</option>
            <option value="ticket">Tickets</option>
            <option value="membership">Memberships</option>
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
      visitorMetric: 'daily',
      tableSearch: '',
      tableFilter: 'all',

      // KPI Data
      kpiData: [
        {
          id: 1,
          title: 'Total Revenue',
          value: 1250000,
          change: 12.5,
          changeType: 'increase',
          icon: 'currency-dollar',
          color: 'green',
          format: 'currency'
        },
        {
          id: 2,
          title: 'Total Visitors',
          value: 45230,
          change: 8.3,
          changeType: 'increase',
          icon: 'users',
          color: 'blue',
          format: 'number'
        },
        {
          id: 3,
          title: 'Artworks Sold',
          value: 127,
          change: -2.1,
          changeType: 'decrease',
          icon: 'photograph',
          color: 'purple',
          format: 'number'
        },
        {
          id: 4,
          title: 'Active Exhibitions',
          value: 8,
          change: 0,
          changeType: 'neutral',
          icon: 'calendar',
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

      // Transaction Columns
      transactionColumns: [
        { key: 'id', label: 'ID', sortable: true },
        { key: 'date', label: 'Date', sortable: true },
        { key: 'type', label: 'Type', sortable: true },
        { key: 'description', label: 'Description', sortable: false },
        { key: 'amount', label: 'Amount', sortable: true, format: 'currency' },
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
      return {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [{
          label: 'Revenue',
          data: [85000, 92000, 78000, 105000, 112000, 98000, 125000, 134000, 118000, 142000, 156000, 165000],
          borderColor: chartColors.blue,
          backgroundColor: withOpacity(chartColors.blue, 0.1),
          fill: true,
          tension: 0.4
        }]
      };
    },

    visitorChartData() {
      const labels = this.visitorMetric === 'daily' 
        ? ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
        : this.visitorMetric === 'weekly'
          ? ['Week 1', 'Week 2', 'Week 3', 'Week 4']
          : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];

      const data = this.visitorMetric === 'daily'
        ? [1200, 1450, 1320, 1580, 1890, 2340, 2120]
        : this.visitorMetric === 'weekly'
          ? [8500, 9200, 8900, 10100]
          : [32000, 35000, 38000, 41000, 45000, 48000];

      return {
        labels,
        datasets: [{
          label: 'Visitors',
          data,
          backgroundColor: chartColors.emerald
        }]
      };
    },

    artworkCategoryData() {
      return {
        labels: this.artworkCategories.map(c => c.name),
        datasets: [{
          data: this.artworkCategories.map(c => c.count),
          backgroundColor: this.categoryColors
        }]
      };
    },

    salesByArtistData() {
      return {
        labels: ['Van Gogh', 'Monet', 'Picasso', 'Warhol', 'Banksy'],
        datasets: [{
          label: 'Sales',
          data: [245000, 198000, 175000, 156000, 134000],
          backgroundColor: chartColors.violet
        }]
      };
    },

    revenueSourceData() {
      return {
        labels: ['Artwork Sales', 'Tickets', 'Memberships', 'Gift Shop', 'Events'],
        datasets: [{
          data: [65, 15, 10, 5, 5],
          backgroundColor: chartColorPalettes.standard.slice(0, 5)
        }]
      };
    },

    yoyComparisonData() {
      return {
        labels: ['Q1', 'Q2', 'Q3', 'Q4'],
        datasets: [
          {
            label: '2025',
            data: [320000, 380000, 420000, 480000],
            backgroundColor: chartColors.slate
          },
          {
            label: '2026',
            data: [380000, 450000, 0, 0],
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
        const [kpiResponse, exhibitionResponse, artworkDistResponse, loansResponse] = await Promise.all([
          this.$api.reports.getDashboardKPIs(),
          this.$api.reports.getExhibitionPerformance?.() || Promise.resolve(null),
          this.$api.reports.getArtworkDistribution?.() || Promise.resolve(null),
          this.$api.loans.getAll?.() || Promise.resolve(null)
        ]);

        // Update KPI data
        if (kpiResponse.data?.success && kpiResponse.data?.data) {
          const kpis = kpiResponse.data.data;
          if (kpis.totalRevenue !== undefined) this.kpiData[0].value = kpis.totalRevenue;
          if (kpis.totalVisitors !== undefined) this.kpiData[1].value = kpis.totalVisitors;
          if (kpis.artworksSold !== undefined) this.kpiData[2].value = kpis.artworksSold;
          if (kpis.activeExhibitions !== undefined) this.kpiData[3].value = kpis.activeExhibitions;
          if (kpis.revenueChange !== undefined) {
            this.kpiData[0].change = Math.abs(kpis.revenueChange);
            this.kpiData[0].changeType = kpis.revenueChange >= 0 ? 'increase' : 'decrease';
          }
        }

        // Update exhibition performance
        if (exhibitionResponse?.data?.success && exhibitionResponse.data?.data) {
          this.topExhibitions = exhibitionResponse.data.data.slice(0, 5).map((ex, i) => ({
            id: ex.id || i + 1,
            name: ex.exhibitionName || ex.name || 'Exhibition',
            visitors: ex.visitorCount || ex.visitors || 0,
            revenue: ex.revenue || 0,
            trend: ex.trend || 0
          }));
        }

        // Update artwork distribution (categories)
        if (artworkDistResponse?.data?.success && artworkDistResponse.data?.data) {
          this.artworkCategories = artworkDistResponse.data.data.map(cat => ({
            name: cat.category || cat.medium || cat.name || 'Other',
            count: cat.count || cat.artworkCount || 0
          }));
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
