<template>
  <!--
    ExhibitionDetail.vue - Exhibition Detail Page
    Art Gallery Management System
  -->
  <div class="exhibition-detail-page">
    <!-- Back Button -->
    <button
      @click="goBack"
      class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6"
    >
      <span class="mr-2">←</span>
      Back to Exhibitions
    </button>

    <!-- Loading -->
    <div v-if="isLoading" class="animate-pulse bg-white rounded-xl p-8">
      <div class="h-8 bg-gray-200 rounded w-1/2 mb-4"></div>
      <div class="h-4 bg-gray-200 rounded w-1/3"></div>
    </div>

    <!-- Content -->
    <div v-else class="space-y-6">
      <!-- Header Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div class="h-48 bg-gradient-to-r from-primary-500 to-secondary-500 flex items-center justify-center">
          <span class="text-6xl">🎨</span>
        </div>
        <div class="p-6">
          <div class="flex items-start justify-between">
            <div>
              <span 
                class="inline-block px-3 py-1 text-sm font-medium rounded-full mb-2"
                :class="getStatusClass(exhibition.status)"
              >
                {{ exhibition.status }}
              </span>
              <h1 class="text-3xl font-bold text-gray-900">{{ exhibition.title }}</h1>
              <p class="text-gray-500 mt-1">{{ formatDateRange(exhibition.startDate, exhibition.endDate) }}</p>
            </div>
            <div class="flex space-x-2">
              <button @click="editExhibition" class="p-2 hover:bg-gray-100 rounded-lg">✏️</button>
              <button @click="showDeleteModal = true" class="p-2 hover:bg-red-50 rounded-lg">🗑️</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Details Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Main Info -->
        <div class="lg:col-span-2 space-y-6">
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-800 mb-4">About</h2>
            <p class="text-gray-600 leading-relaxed">{{ exhibition.description }}</p>
          </div>

          <!-- Artworks -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-lg font-semibold text-gray-800">Featured Artworks</h2>
              <span class="text-sm text-gray-500">{{ exhibition.artworkCount }} pieces</span>
            </div>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div 
                v-for="n in 4" 
                :key="n"
                class="aspect-square bg-gray-100 rounded-lg flex items-center justify-center text-3xl"
              >
                🖼️
              </div>
            </div>
          </div>
        </div>

        <!-- Sidebar -->
        <div class="space-y-6">
          <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-lg font-semibold text-gray-800 mb-4">Details</h2>
            <dl class="space-y-4">
              <div>
                <dt class="text-sm text-gray-500">Curator</dt>
                <dd class="font-medium text-gray-900">{{ exhibition.curator || 'TBD' }}</dd>
              </div>
              <div>
                <dt class="text-sm text-gray-500">Location</dt>
                <dd class="font-medium text-gray-900">{{ exhibition.location || 'Main Gallery' }}</dd>
              </div>
              <div>
                <dt class="text-sm text-gray-500">Ticket Price</dt>
                <dd class="font-medium text-gray-900">{{ formatCurrency(exhibition.ticketPrice) }}</dd>
              </div>
              <div>
                <dt class="text-sm text-gray-500">Visitors</dt>
                <dd class="font-medium text-gray-900">{{ (exhibition.visitorCount || 0).toLocaleString() }}</dd>
              </div>
            </dl>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="fixed inset-0 bg-black bg-opacity-50" @click="showDeleteModal = false"></div>
      <div class="relative bg-white rounded-xl p-6 max-w-md w-full z-10">
        <h3 class="text-lg font-semibold mb-2">Delete Exhibition</h3>
        <p class="text-gray-500 mb-6">Are you sure you want to delete this exhibition?</p>
        <div class="flex justify-end space-x-3">
          <button @click="showDeleteModal = false" class="px-4 py-2 bg-gray-100 rounded-lg">Cancel</button>
          <button @click="deleteExhibition" class="px-4 py-2 bg-red-600 text-white rounded-lg">Delete</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ExhibitionDetail Page
 */
export default {
  name: 'ExhibitionDetailPage',

  props: {
    id: {
      type: [String, Number],
      required: true
    }
  },

  data() {
    return {
      isLoading: true,
      showDeleteModal: false,
      exhibition: {}
    };
  },

  created() {
    this.loadExhibition();
  },

  methods: {
    async loadExhibition() {
      this.isLoading = true;
      try {
        await new Promise(resolve => setTimeout(resolve, 500));
        this.exhibition = {
          id: this.id,
          title: 'Modern Masters',
          description: 'Exploring the works of 20th century masters including Picasso, Kandinsky, and Mondrian. This exhibition showcases the evolution of modern art through groundbreaking pieces.',
          startDate: '2024-01-15',
          endDate: '2024-04-30',
          status: 'current',
          artworkCount: 45,
          curator: 'Dr. Sarah Mitchell',
          location: 'Gallery Wing A',
          ticketPrice: 25,
          visitorCount: 12450
        };
      } finally {
        this.isLoading = false;
      }
    },

    goBack() {
      this.$router.push('/exhibitions');
    },

    editExhibition() {
      this.$router.push({ name: 'EditExhibition', params: { id: this.id } });
    },

    deleteExhibition() {
      this.$router.push('/exhibitions');
    },

    formatDateRange(start, end) {
      const opts = { month: 'short', day: 'numeric', year: 'numeric' };
      return `${new Date(start).toLocaleDateString('en-US', opts)} - ${new Date(end).toLocaleDateString('en-US', opts)}`;
    },

    formatCurrency(value) {
      if (!value) return 'Free';
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    },

    getStatusClass(status) {
      const classes = {
        current: 'bg-green-100 text-green-700',
        upcoming: 'bg-blue-100 text-blue-700',
        past: 'bg-gray-100 text-gray-700'
      };
      return classes[status] || classes.past;
    }
  }
};
</script>

<style scoped>
.exhibition-detail-page {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}
</style>
