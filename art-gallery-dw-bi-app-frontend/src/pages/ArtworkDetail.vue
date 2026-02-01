<template>
  <!--
    ArtworkDetail.vue - Artwork Detail Page
    Art Gallery Management System
  -->
  <div class="artwork-detail-page">
    <!-- Back Button -->
    <button
      @click="goBack"
      class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6 transition-colors"
    >
      <span class="mr-2">←</span>
      Back to Inventory
    </button>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
      <div class="animate-pulse">
        <div class="flex flex-col lg:flex-row gap-8">
          <div class="lg:w-1/2">
            <div class="h-96 bg-gray-200 rounded-xl"></div>
          </div>
          <div class="lg:w-1/2 space-y-4">
            <div class="h-8 bg-gray-200 rounded w-3/4"></div>
            <div class="h-4 bg-gray-200 rounded w-1/2"></div>
            <div class="h-4 bg-gray-200 rounded w-full"></div>
            <div class="h-4 bg-gray-200 rounded w-full"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div 
      v-else-if="error"
      class="bg-white rounded-xl shadow-sm border border-gray-100 p-8 text-center"
    >
      <span class="text-6xl mb-4 block">❌</span>
      <h2 class="text-xl font-semibold text-gray-800 mb-2">Artwork Not Found</h2>
      <p class="text-gray-500 mb-6">The artwork you're looking for doesn't exist or has been removed.</p>
      <router-link
        to="/artworks"
        class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
      >
        Return to Inventory
      </router-link>
    </div>

    <!-- Artwork Content -->
    <div v-else class="space-y-6">
      <!-- Main Content Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div class="flex flex-col lg:flex-row">
          <!-- Image Section -->
          <div class="lg:w-1/2 bg-gray-100">
            <div class="relative h-96 lg:h-full min-h-[400px]">
              <img 
                v-if="artwork.imageUrl" 
                :src="artwork.imageUrl" 
                :alt="artwork.title"
                class="w-full h-full object-contain"
              />
              <div 
                v-else 
                class="w-full h-full flex items-center justify-center text-8xl text-gray-300"
              >
                🖼️
              </div>
              
              <!-- Status Badge -->
              <span 
                class="absolute top-4 left-4 px-3 py-1 text-sm font-medium rounded-full"
                :class="getStatusClass(artwork.status)"
              >
                {{ artwork.status }}
              </span>
            </div>
          </div>

          <!-- Details Section -->
          <div class="lg:w-1/2 p-8">
            <div class="flex items-start justify-between mb-4">
              <div>
                <h1 class="text-3xl font-bold text-gray-900">{{ artwork.title }}</h1>
                <p class="text-xl text-gray-500 mt-1">{{ artistDisplayName }}</p>
              </div>
              
              <div class="flex items-center space-x-2">
                <button
                  @click="editArtwork"
                  class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                  title="Edit"
                >
                  ✏️
                </button>
                <button
                  @click="showDeleteModal = true"
                  class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Delete"
                >
                  🗑️
                </button>
              </div>
            </div>

            <!-- Metadata Grid -->
            <div class="grid grid-cols-2 gap-4 mb-6">
              <div>
                <p class="text-sm text-gray-500">Year Created</p>
                <p class="font-medium text-gray-900">{{ artwork.yearCreated || artwork.year || 'N/A' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Category</p>
                <p class="font-medium text-gray-900">{{ artwork.medium || artwork.category || 'N/A' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Medium</p>
                <p class="font-medium text-gray-900">{{ artwork.medium || 'N/A' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Collection</p>
                <p class="font-medium text-gray-900">{{ artwork.collectionName || artwork.dimensions || 'N/A' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Location</p>
                <p class="font-medium text-gray-900">{{ artwork.locationName || artwork.location || 'N/A' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Estimated Value</p>
                <p class="font-medium text-gray-900">{{ formatCurrency(artwork.estimatedValue) }}</p>
              </div>
            </div>

            <!-- Description -->
            <div class="mb-6">
              <h3 class="text-sm text-gray-500 mb-2">Description</h3>
              <p class="text-gray-700 leading-relaxed">
                {{ artwork.description || 'No description available.' }}
              </p>
            </div>

            <!-- Location -->
            <div v-if="artwork.location" class="mb-6">
              <h3 class="text-sm text-gray-500 mb-2">Current Location</h3>
              <div class="flex items-center text-gray-700">
                <span class="mr-2">📍</span>
                {{ artwork.location }}
              </div>
            </div>

            <!-- Tags -->
            <div v-if="artwork.tags && artwork.tags.length > 0">
              <h3 class="text-sm text-gray-500 mb-2">Tags</h3>
              <div class="flex flex-wrap gap-2">
                <span 
                  v-for="tag in artwork.tags" 
                  :key="tag"
                  class="px-3 py-1 bg-gray-100 text-gray-600 text-sm rounded-full"
                >
                  {{ tag }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Additional Information Tabs -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <!-- Tab Navigation -->
        <div class="border-b border-gray-100">
          <nav class="flex">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              @click="activeTab = tab.id"
              class="px-6 py-4 text-sm font-medium transition-colors relative"
              :class="activeTab === tab.id 
                ? 'text-primary-600' 
                : 'text-gray-500 hover:text-gray-700'"
            >
              <span class="mr-2">{{ tab.icon }}</span>
              {{ tab.label }}
              <div 
                v-if="activeTab === tab.id"
                class="absolute bottom-0 left-0 right-0 h-0.5 bg-primary-600"
              ></div>
            </button>
          </nav>
        </div>

        <!-- Tab Content -->
        <div class="p-6">
          <!-- Provenance Tab -->
          <div v-if="activeTab === 'provenance'">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Provenance History</h3>
            <div v-if="artwork.provenance && artwork.provenance.length > 0" class="space-y-4">
              <div 
                v-for="(entry, index) in artwork.provenance" 
                :key="index"
                class="flex items-start border-l-2 border-primary-200 pl-4"
              >
                <div>
                  <p class="font-medium text-gray-900">{{ entry.owner }}</p>
                  <p class="text-sm text-gray-500">{{ entry.period }}</p>
                  <p v-if="entry.notes" class="text-sm text-gray-600 mt-1">{{ entry.notes }}</p>
                </div>
              </div>
            </div>
            <p v-else class="text-gray-500">No provenance information available.</p>
          </div>

          <!-- Exhibition History Tab -->
          <div v-if="activeTab === 'exhibitions'">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Exhibition History</h3>
            <div v-if="artwork.exhibitions && artwork.exhibitions.length > 0" class="space-y-4">
              <div 
                v-for="exhibition in artwork.exhibitions" 
                :key="exhibition.id"
                class="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
              >
                <div>
                  <p class="font-medium text-gray-900">{{ exhibition.title }}</p>
                  <p class="text-sm text-gray-500">{{ exhibition.venue }}</p>
                </div>
                <p class="text-sm text-gray-500">{{ exhibition.dates }}</p>
              </div>
            </div>
            <p v-else class="text-gray-500">This artwork has not been exhibited yet.</p>
          </div>

          <!-- Insurance Tab -->
          <div v-if="activeTab === 'insurance'">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Insurance Information</h3>
            <div v-if="artwork.insurance" class="grid grid-cols-2 gap-4">
              <div>
                <p class="text-sm text-gray-500">Policy Number</p>
                <p class="font-medium text-gray-900">{{ artwork.insurance.policyNumber }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Insurance Company</p>
                <p class="font-medium text-gray-900">{{ artwork.insurance.company }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Coverage Amount</p>
                <p class="font-medium text-gray-900">{{ formatCurrency(artwork.insurance.coverage) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Expiry Date</p>
                <p class="font-medium text-gray-900">{{ formatDate(artwork.insurance.expiryDate) }}</p>
              </div>
            </div>
            <p v-else class="text-gray-500">No insurance information available.</p>
          </div>

          <!-- Restoration Tab -->
          <div v-if="activeTab === 'restoration'">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Restoration History</h3>
            <div v-if="artwork.restorations && artwork.restorations.length > 0" class="space-y-4">
              <div 
                v-for="restoration in artwork.restorations" 
                :key="restoration.id"
                class="p-4 bg-gray-50 rounded-lg"
              >
                <div class="flex items-center justify-between mb-2">
                  <p class="font-medium text-gray-900">{{ restoration.type }}</p>
                  <span 
                    class="px-2 py-1 text-xs font-medium rounded-full"
                    :class="restoration.status === 'Completed' ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'"
                  >
                    {{ restoration.status }}
                  </span>
                </div>
                <p class="text-sm text-gray-600">{{ restoration.description }}</p>
                <p class="text-sm text-gray-500 mt-2">
                  Performed by {{ restoration.specialist }} • {{ formatDate(restoration.date) }}
                </p>
              </div>
            </div>
            <p v-else class="text-gray-500">No restoration history available.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div 
      v-if="showDeleteModal"
      class="fixed inset-0 z-50 overflow-y-auto"
    >
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="showDeleteModal = false"></div>
        
        <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6 z-10">
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Delete Artwork</h3>
          <p class="text-gray-500 mb-6">
            Are you sure you want to delete "{{ artwork.title }}"? This action cannot be undone.
          </p>
          <div class="flex justify-end space-x-3">
            <button
              @click="showDeleteModal = false"
              class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors font-medium"
            >
              Cancel
            </button>
            <button
              @click="deleteArtwork"
              class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import { insuranceAPI } from '@/api/insuranceAPI';
import { exhibitionAPI } from '@/api/exhibitionAPI';
import apiClient from '@/api/client';

/**
 * ArtworkDetail Page Component
 * Displays detailed artwork information with tabs
 */
export default {
  name: 'ArtworkDetailPage',

  props: {
    id: {
      type: [String, Number],
      required: true
    }
  },

  data() {
    return {
      isLoading: true,
      error: false,
      showDeleteModal: false,
      activeTab: 'provenance',
      tabs: [
        { id: 'provenance', label: 'Provenance', icon: '📜' },
        { id: 'exhibitions', label: 'Exhibitions', icon: '🎨' },
        { id: 'insurance', label: 'Insurance', icon: '🛡️' },
        { id: 'restoration', label: 'Restoration', icon: '🔧' }
      ],
      // Tab data loaded from APIs
      artworkExhibitions: [],
      artworkInsurance: null,
      artworkRestorations: [],
      artworkProvenance: []
    };
  },

  computed: {
    ...mapState({
      artworks: state => state.artwork?.artworks || []
    }),

    artwork() {
      const found = this.artworks.find(a => a.id === parseInt(this.id));
      const base = found || {
        id: null,
        title: '',
        artist: '',
        artistName: '',
        year: null,
        category: '',
        medium: '',
        dimensions: '',
        description: '',
        status: '',
        location: '',
        imageUrl: '',
        acquisitionDate: null,
        estimatedValue: null,
        tags: []
      };
      
      // Merge with loaded tab data
      return {
        ...base,
        provenance: this.artworkProvenance,
        exhibitions: this.artworkExhibitions,
        insurance: this.artworkInsurance,
        restorations: this.artworkRestorations
      };
    },

    artistDisplayName() {
      return this.artwork.artistName || this.artwork.artist || 'Unknown Artist';
    }
  },

  watch: {
    id: {
      immediate: true,
      handler: 'loadArtwork'
    }
  },

  methods: {
    ...mapActions({
      fetchArtworks: 'artwork/fetchArtworks',
      removeArtwork: 'artwork/deleteArtwork'
    }),

    async loadArtwork() {
      this.isLoading = true;
      this.error = false;

      try {
        // Fetch artworks if not already loaded
        if (this.artworks.length === 0) {
          await this.fetchArtworks();
        }
        
        // Check if artwork exists
        const artworkId = parseInt(this.id);
        const artworkExists = this.artworks.find(a => a.id === artworkId);
        
        if (!artworkExists) {
          this.error = true;
          return;
        }
        
        // Load tab data in parallel
        await this.loadTabData(artworkId);
      } catch (err) {
        console.error('Error loading artwork:', err);
        this.error = true;
      } finally {
        this.isLoading = false;
      }
    },

    async loadTabData(artworkId) {
      try {
        // Load all tab data in parallel
        const [insuranceData, restorationData, exhibitionData] = await Promise.allSettled([
          this.loadInsuranceData(artworkId),
          this.loadRestorationData(artworkId),
          this.loadExhibitionData(artworkId)
        ]);

        // Process results (ignore errors for individual tabs)
        if (insuranceData.status === 'fulfilled') {
          this.artworkInsurance = insuranceData.value;
        }
        if (restorationData.status === 'fulfilled') {
          this.artworkRestorations = restorationData.value;
        }
        if (exhibitionData.status === 'fulfilled') {
          this.artworkExhibitions = exhibitionData.value;
        }
      } catch (err) {
        console.warn('Error loading tab data:', err);
        // Don't fail the page if tab data can't be loaded
      }
    },

    async loadInsuranceData(artworkId) {
      try {
        const response = await insuranceAPI.getAll({ PageSize: 100 });
        const data = response.data?.data?.items || response.data?.data || [];
        
        // Filter insurance records for this artwork
        const artworkInsurance = data.filter(ins => ins.artworkId === artworkId);
        
        if (artworkInsurance.length > 0) {
          // Return the most recent insurance as the primary one
          const insurance = artworkInsurance[0];
          return {
            policyNumber: insurance.policyId?.toString() || 'N/A',
            company: insurance.policyProvider || 'N/A',
            coverage: insurance.insuredAmount || 0,
            expiryDate: insurance.expiryDate || null
          };
        }
        return null;
      } catch (err) {
        console.warn('Error loading insurance data:', err);
        return null;
      }
    },

    async loadRestorationData(artworkId) {
      try {
        const response = await apiClient.get('/restoration', { params: { PageSize: 100 } });
        const data = response.data?.data?.items || response.data?.data || [];
        
        // Filter restoration records for this artwork
        const artworkRestorations = data.filter(rest => rest.artworkId === artworkId);
        
        return artworkRestorations.map(rest => ({
          id: rest.id,
          type: 'Restoration',
          description: rest.description || 'General restoration work',
          specialist: rest.staffName || 'Unknown',
          date: rest.endDate || rest.startDate,
          status: rest.endDate ? 'Completed' : 'In Progress'
        }));
      } catch (err) {
        console.warn('Error loading restoration data:', err);
        return [];
      }
    },

    async loadExhibitionData(artworkId) {
      try {
        // Get all exhibitions and filter those that might include this artwork
        const response = await exhibitionAPI.getAll({ PageSize: 100 });
        const data = response.data?.data?.items || response.data?.data || [];
        
        // For each exhibition, check if the artwork is included
        const artworkExhibitions = [];
        
        for (const exhibition of data) {
          try {
            const artworksResponse = await apiClient.get(`/exhibitions/${exhibition.id}/artworks`);
            const exhibitionArtworks = artworksResponse.data?.data || [];
            
            if (exhibitionArtworks.some(a => a.artworkId === artworkId)) {
              artworkExhibitions.push({
                id: exhibition.id,
                title: exhibition.title,
                venue: exhibition.exhibitorName || 'Unknown Venue',
                dates: this.formatDateRange(exhibition.startDate, exhibition.endDate)
              });
            }
          } catch (e) {
            // Skip exhibitions we can't get artworks for
          }
        }
        
        return artworkExhibitions;
      } catch (err) {
        console.warn('Error loading exhibition data:', err);
        return [];
      }
    },

    formatDateRange(startDate, endDate) {
      const start = startDate ? new Date(startDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }) : '';
      const end = endDate ? new Date(endDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }) : '';
      return start && end ? `${start} - ${end}` : start || end || 'N/A';
    },

    goBack() {
      this.$router.push('/artworks');
    },

    editArtwork() {
      this.$router.push({ name: 'EditArtwork', params: { id: this.id } });
    },

    async deleteArtwork() {
      try {
        await this.removeArtwork(parseInt(this.id));
        this.showDeleteModal = false;
        this.$router.push('/artworks');
      } catch (error) {
        console.error('Error deleting artwork:', error);
        alert('Failed to delete artwork. Please try again.');
      }
    },

    formatDate(dateString) {
      if (!dateString) return 'N/A';
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    },

    formatCurrency(value) {
      if (!value) return 'N/A';
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        maximumFractionDigits: 0
      }).format(value);
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
.artwork-detail-page {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}
</style>
