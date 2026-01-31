<template>
  <!--
    Home.vue - Dashboard Home Page
    Art Gallery Management System
  -->
  <div class="home-page">
    <!-- Page Header -->
    <header class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">Art Gallery Dashboard</h1>
      <p class="text-gray-500 mt-2">Welcome to the Art Gallery Management System</p>
    </header>

    <!-- KPI Cards Row -->
    <section class="mb-8">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <kpi-card
          v-for="kpi in kpiData"
          :key="kpi.id"
          :value="kpi.value"
          :label="kpi.label"
          :icon="kpi.icon"
          :color="kpi.color"
          :trend="kpi.trend"
          :format="kpi.format"
          :is-loading="isLoading"
        />
      </div>
    </section>

    <!-- Quick Actions -->
    <section class="mb-8">
      <h2 class="text-xl font-semibold text-gray-800 mb-4">Quick Actions</h2>
      <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
        <router-link
          v-for="action in quickActions"
          :key="action.route"
          :to="action.route"
          class="flex flex-col items-center p-4 bg-white rounded-xl shadow-sm border border-gray-100 hover:shadow-md hover:border-primary-200 transition-all group"
        >
          <span class="text-2xl mb-2 group-hover:scale-110 transition-transform">{{ action.icon }}</span>
          <span class="text-sm font-medium text-gray-700 text-center">{{ action.label }}</span>
        </router-link>
      </div>
    </section>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Recent Artworks -->
      <section class="lg:col-span-2">
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-gray-800">Recent Artworks</h2>
            <router-link 
              to="/artworks" 
              class="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              View All →
            </router-link>
          </div>

          <div v-if="isLoading" class="space-y-4">
            <div v-for="n in 3" :key="n" class="animate-pulse flex space-x-4">
              <div class="w-16 h-16 bg-gray-200 rounded-lg"></div>
              <div class="flex-1 space-y-2 py-1">
                <div class="h-4 bg-gray-200 rounded w-3/4"></div>
                <div class="h-3 bg-gray-200 rounded w-1/2"></div>
              </div>
            </div>
          </div>

          <div v-else class="space-y-4">
            <div 
              v-for="artwork in recentArtworks" 
              :key="artwork.id"
              class="flex items-center space-x-4 p-3 rounded-lg hover:bg-gray-50 transition-colors cursor-pointer"
              @click="viewArtwork(artwork.id)"
            >
              <div class="w-16 h-16 bg-gray-200 rounded-lg overflow-hidden flex-shrink-0">
                <img 
                  v-if="artwork.imageUrl" 
                  :src="artwork.imageUrl" 
                  :alt="artwork.title"
                  class="w-full h-full object-cover"
                />
                <div v-else class="w-full h-full flex items-center justify-center text-2xl">
                  🖼️
                </div>
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="font-medium text-gray-900 truncate">{{ artwork.title }}</h3>
                <p class="text-sm text-gray-500">{{ artwork.artist }} • {{ artwork.year }}</p>
              </div>
              <span 
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="getStatusClass(artwork.status)"
              >
                {{ artwork.status }}
              </span>
            </div>
          </div>
        </div>
      </section>

      <!-- Sidebar -->
      <aside class="space-y-6">
        <!-- Upcoming Exhibitions -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-gray-800">Upcoming Exhibitions</h2>
            <router-link 
              to="/exhibitions" 
              class="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              View All
            </router-link>
          </div>

          <div v-if="isLoading" class="space-y-3">
            <div v-for="n in 3" :key="n" class="animate-pulse">
              <div class="h-4 bg-gray-200 rounded w-full mb-2"></div>
              <div class="h-3 bg-gray-200 rounded w-2/3"></div>
            </div>
          </div>

          <div v-else class="space-y-4">
            <div 
              v-for="exhibition in upcomingExhibitions" 
              :key="exhibition.id"
              class="border-l-4 border-primary-500 pl-4 py-2"
            >
              <h3 class="font-medium text-gray-900">{{ exhibition.title }}</h3>
              <p class="text-sm text-gray-500">
                {{ formatDate(exhibition.startDate) }} - {{ formatDate(exhibition.endDate) }}
              </p>
            </div>
          </div>
        </div>

        <!-- ETL Status -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-gray-800">ETL Status</h2>
            <router-link 
              to="/etl" 
              class="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              Manage
            </router-link>
          </div>

          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-600">Last Sync</span>
              <span class="text-sm font-medium text-gray-900">{{ etlStatus.lastSync }}</span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-600">Status</span>
              <span 
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="etlStatus.isHealthy ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'"
              >
                {{ etlStatus.isHealthy ? 'Healthy' : 'Error' }}
              </span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-600">Records Synced</span>
              <span class="text-sm font-medium text-gray-900">{{ etlStatus.recordsSynced.toLocaleString() }}</span>
            </div>
          </div>
        </div>

        <!-- System Notifications -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <h2 class="text-lg font-semibold text-gray-800 mb-4">Notifications</h2>
          
          <div class="space-y-3">
            <div 
              v-for="notification in notifications" 
              :key="notification.id"
              class="flex items-start space-x-3 p-3 rounded-lg"
              :class="notification.bgClass"
            >
              <span class="text-lg">{{ notification.icon }}</span>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium" :class="notification.textClass">
                  {{ notification.message }}
                </p>
                <p class="text-xs text-gray-400 mt-1">{{ notification.time }}</p>
              </div>
            </div>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import KPICard from '@/components/reports/KPICard.vue';

/**
 * Home Page Component
 * Dashboard with KPIs, recent items, and quick actions
 */
export default {
  name: 'HomePage',

  components: {
    'kpi-card': KPICard
  },

  data() {
    return {
      isLoading: true,
      kpiData: [
        { id: 1, label: 'Total Artworks', value: 0, icon: '🖼️', color: 'primary', trend: 5.2, format: 'number' },
        { id: 2, label: 'Active Exhibitions', value: 0, icon: '🎨', color: 'secondary', trend: 12.5, format: 'number' },
        { id: 3, label: 'Monthly Visitors', value: 0, icon: '👥', color: 'info', trend: -2.3, format: 'number' },
        { id: 4, label: 'Total Revenue', value: 0, icon: '💰', color: 'success', trend: 8.7, format: 'currency' }
      ],
      quickActions: [
        { route: '/artworks/new', icon: '➕', label: 'Add Artwork' },
        { route: '/exhibitions/new', icon: '📅', label: 'New Exhibition' },
        { route: '/visitors/new', icon: '👤', label: 'Add Visitor' },
        { route: '/loans/new', icon: '📋', label: 'New Loan' },
        { route: '/etl', icon: '🔄', label: 'Sync Data' },
        { route: '/reports', icon: '📊', label: 'View Reports' }
      ],
      recentArtworks: [],
      upcomingExhibitions: [],
      etlStatus: {
        lastSync: 'Loading...',
        isHealthy: true,
        recordsSynced: 0
      },
      notifications: [
        { 
          id: 1, 
          icon: '✅', 
          message: 'ETL sync completed successfully', 
          time: '5 minutes ago',
          bgClass: 'bg-green-50',
          textClass: 'text-green-800'
        },
        { 
          id: 2, 
          icon: '⚠️', 
          message: 'Artwork insurance expires in 30 days', 
          time: '1 hour ago',
          bgClass: 'bg-yellow-50',
          textClass: 'text-yellow-800'
        },
        { 
          id: 3, 
          icon: 'ℹ️', 
          message: 'New exhibition proposal submitted', 
          time: '3 hours ago',
          bgClass: 'bg-blue-50',
          textClass: 'text-blue-800'
        }
      ]
    };
  },

  computed: {
    ...mapState({
      artworks: state => state.artwork.artworks,
      exhibitions: state => state.exhibition.exhibitions
    })
  },

  created() {
    this.loadDashboardData();
  },

  methods: {
    ...mapActions({
      fetchArtworks: 'artwork/fetchArtworks',
      fetchExhibitions: 'exhibition/fetchExhibitions'
    }),

    async loadDashboardData() {
      this.isLoading = true;
      
      try {
        // Fetch real data from APIs
        const [artworksRes, exhibitionsRes, kpiRes] = await Promise.allSettled([
          this.$api.artworks?.getArtworks?.({ limit: 4 }) || Promise.resolve(null),
          this.$api.exhibitions?.getExhibitions?.({ limit: 3 }) || Promise.resolve(null),
          this.$api.reports?.getDashboardKPIs?.() || Promise.resolve(null)
        ]);

        // Update KPIs from API
        if (kpiRes.status === 'fulfilled' && kpiRes.value?.data?.success) {
          const kpis = kpiRes.value.data.data;
          if (kpis.totalArtworks !== undefined) this.kpiData[0].value = kpis.totalArtworks;
          if (kpis.activeExhibitions !== undefined) this.kpiData[1].value = kpis.activeExhibitions;
          if (kpis.totalVisitors !== undefined) this.kpiData[2].value = kpis.totalVisitors;
          if (kpis.totalRevenue !== undefined) this.kpiData[3].value = kpis.totalRevenue;
        }

        // Set recent artworks from API
        if (artworksRes.status === 'fulfilled' && artworksRes.value?.data?.success) {
          const artworks = artworksRes.value.data.data || [];
          this.recentArtworks = artworks.slice(0, 4).map(a => ({
            id: a.id,
            title: a.title,
            artist: a.artistName || a.artist,
            year: a.year || a.creationYear,
            status: a.status || 'On Display',
            imageUrl: a.imageUrl || ''
          }));
        }

        // Set upcoming exhibitions from API
        if (exhibitionsRes.status === 'fulfilled' && exhibitionsRes.value?.data?.success) {
          const exhibitions = exhibitionsRes.value.data.data || [];
          this.upcomingExhibitions = exhibitions.slice(0, 3).map(e => ({
            id: e.id,
            title: e.title || e.exhibitionName,
            startDate: e.startDate,
            endDate: e.endDate
          }));
        }

        // Fetch ETL status
        try {
          const etlRes = await this.$api.etl?.getStatus?.();
          if (etlRes?.data?.success) {
            const status = etlRes.data.data;
            this.etlStatus = {
              lastSync: status.lastSync?.timestamp ? this.formatTimeAgo(new Date(status.lastSync.timestamp)) : 'Never',
              isHealthy: status.status === 'idle' || status.status === 'completed',
              recordsSynced: status.lastSync?.recordsProcessed || 0
            };
          }
        } catch (etlError) {
          console.error('ETL status fetch failed:', etlError);
        }

      } catch (error) {
        console.error('Error loading dashboard data:', error);
      } finally {
        this.isLoading = false;
      }
    },

    formatTimeAgo(date) {
      const now = new Date();
      const diff = now - date;
      const minutes = Math.floor(diff / (1000 * 60));
      const hours = Math.floor(diff / (1000 * 60 * 60));
      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      
      if (minutes < 1) return 'Just now';
      if (minutes < 60) return `${minutes} minutes ago`;
      if (hours < 24) return `${hours} hours ago`;
      return `${days} days ago`;
    },

    viewArtwork(id) {
      this.$router.push({ name: 'ArtworkDetail', params: { id } });
    },

    formatDate(dateString) {
      if (!dateString) return '';
      return new Date(dateString).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      });
    },

    getStatusClass(status) {
      const statusClasses = {
        'On Display': 'bg-green-100 text-green-700',
        'In Storage': 'bg-gray-100 text-gray-700',
        'On Loan': 'bg-blue-100 text-blue-700',
        'Under Restoration': 'bg-yellow-100 text-yellow-700'
      };
      return statusClasses[status] || 'bg-gray-100 text-gray-700';
    }
  }
};
</script>

<style scoped>
.home-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
