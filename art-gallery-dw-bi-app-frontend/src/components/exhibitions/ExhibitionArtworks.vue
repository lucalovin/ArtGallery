<template>
  <!--
    ExhibitionArtworks.vue - Manage Artworks in Exhibition
    Art Gallery Management System

    GLOBAL is read-only:
    - show artworks
    - hide Add Artwork
    - hide Remove
    - block add/remove methods
  -->
  <div class="exhibition-artworks">
    <!-- Header -->
    <div class="flex items-center justify-between mb-4">
      <div>
        <h2 class="text-lg font-semibold text-gray-800">
          Featured Artworks
          <span class="text-sm font-normal text-gray-500 ml-2">
            ({{ exhibitionArtworks.length }} pieces)
          </span>
        </h2>

        <p v-if="effectiveReadOnly" class="text-xs text-blue-700 mt-1">
          Global view is read-only. Artwork assignment is available only on AM or EU.
        </p>
      </div>

      <button
        v-if="!effectiveReadOnly"
        @click="openAddModal"
        class="inline-flex items-center px-3 py-2 bg-primary-600 text-white text-sm font-medium rounded-lg hover:bg-primary-700 transition-colors"
      >
        <span class="mr-1">+</span>
        Add Artwork
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center py-8">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
    </div>

    <!-- Empty State -->
    <div v-else-if="exhibitionArtworks.length === 0" class="text-center py-8 bg-gray-50 rounded-lg">
      <span class="text-4xl mb-2 block">🖼️</span>
      <p class="text-gray-500">No artworks in this exhibition yet.</p>

      <button
        v-if="!effectiveReadOnly"
        @click="openAddModal"
        class="mt-4 text-primary-600 hover:text-primary-700 font-medium"
      >
        Add the first artwork →
      </button>
    </div>

    <!-- Artworks Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="artwork in exhibitionArtworks"
        :key="artwork.artworkId || artwork.id"
        class="bg-white rounded-lg border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
      >
        <!-- Artwork Image Placeholder -->
        <div class="h-32 bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center">
          <span class="text-4xl">🖼️</span>
        </div>

        <!-- Artwork Info -->
        <div class="p-4">
          <h3 class="font-medium text-gray-900 truncate">
            {{ artwork.title || artwork.artworkTitle }}
          </h3>
          <p class="text-sm text-gray-500 truncate">
            {{ artwork.artistName || 'Unknown Artist' }}
          </p>
          <p v-if="artwork.positionInGallery" class="text-xs text-gray-400 mt-1">
            Position: {{ artwork.positionInGallery }}
          </p>

          <!-- Actions -->
          <div v-if="!effectiveReadOnly" class="mt-3 flex justify-end">
            <button
              @click="confirmRemoveArtwork(artwork)"
              class="text-red-500 hover:text-red-700 text-sm font-medium"
              title="Remove from exhibition"
            >
              Remove
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Artwork Modal -->
    <div v-if="showAddModal && !effectiveReadOnly" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="fixed inset-0 bg-black bg-opacity-50" @click="closeAddModal"></div>
      <div class="relative bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[80vh] overflow-hidden z-10">
        <!-- Modal Header -->
        <div class="px-6 py-4 border-b border-gray-200">
          <h3 class="text-lg font-semibold text-gray-900">Add Artwork to Exhibition</h3>
          <p class="text-sm text-gray-500">Select artworks to add to this exhibition</p>
        </div>

        <!-- Search -->
        <div class="px-6 py-3 border-b border-gray-100">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search artworks..."
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>

        <!-- Available Artworks List -->
        <div class="px-6 py-4 max-h-96 overflow-y-auto">
          <div v-if="isLoadingAvailable" class="flex items-center justify-center py-8">
            <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-primary-600"></div>
          </div>

          <div v-else-if="filteredAvailableArtworks.length === 0" class="text-center py-8 text-gray-500">
            <p>No available artworks found.</p>
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="artwork in filteredAvailableArtworks"
              :key="artwork.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
            >
              <div class="flex items-center space-x-3">
                <span class="text-2xl">🖼️</span>
                <div>
                  <p class="font-medium text-gray-900">{{ artwork.title }}</p>
                  <p class="text-sm text-gray-500">{{ artwork.artistName || 'Unknown Artist' }}</p>
                </div>
              </div>
              <button
                @click="addArtwork(artwork)"
                :disabled="addingArtworkId === artwork.id"
                class="px-3 py-1 bg-primary-600 text-white text-sm rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {{ addingArtworkId === artwork.id ? 'Adding...' : 'Add' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-4 border-t border-gray-200 bg-gray-50 flex justify-end">
          <button
            @click="closeAddModal"
            class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>

    <!-- Remove Confirmation Modal -->
    <div v-if="showRemoveModal && !effectiveReadOnly" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="fixed inset-0 bg-black bg-opacity-50" @click="showRemoveModal = false"></div>
      <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full z-10 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-2">Remove Artwork</h3>
        <p class="text-gray-600 mb-6">
          Are you sure you want to remove
          "<strong>{{ artworkToRemove?.title || artworkToRemove?.artworkTitle }}</strong>"
          from this exhibition?
        </p>
        <div class="flex justify-end space-x-3">
          <button
            @click="showRemoveModal = false"
            class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
          >
            Cancel
          </button>
          <button
            @click="removeArtwork"
            :disabled="isRemoving"
            class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 transition-colors"
          >
            {{ isRemoving ? 'Removing...' : 'Remove' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Success/Error Toast -->
    <div
      v-if="toast.show"
      class="fixed bottom-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg transition-all"
      :class="toast.type === 'success' ? 'bg-green-600 text-white' : 'bg-red-600 text-white'"
    >
      {{ toast.message }}
    </div>
  </div>
</template>

<script>
import { artworkAPI } from '@/api/artworkAPI';
import { exhibitionAPI } from '@/api/exhibitionAPI';

/**
 * ExhibitionArtworks Component
 * Manages the artworks displayed in an exhibition.
 */
export default {
  name: 'ExhibitionArtworks',

  props: {
    exhibitionId: {
      type: [Number, String],
      required: true
    },

    readOnly: {
      type: Boolean,
      default: false
    }
  },

  emits: ['artwork-added', 'artwork-removed'],

  data() {
    return {
      exhibitionArtworks: [],
      allArtworks: [],

      isLoading: false,
      isLoadingAvailable: false,
      isRemoving: false,
      addingArtworkId: null,

      showAddModal: false,
      showRemoveModal: false,

      searchQuery: '',
      artworkToRemove: null,

      toast: {
        show: false,
        message: '',
        type: 'success'
      }
    };
  },

  computed: {
    isGlobalSource() {
      const storeSource = this.$store?.state?.dataSource?.source;
      const localSource =
        localStorage.getItem('dataSource') ||
        localStorage.getItem('selectedDataSource') ||
        localStorage.getItem('currentDataSource') ||
        localStorage.getItem('artGalleryDataSource') ||
        '';

      return String(storeSource || localSource).toUpperCase() === 'GLOBAL';
    },

    effectiveReadOnly() {
      return this.readOnly || this.isGlobalSource;
    },

    availableArtworks() {
      const exhibitedIds = this.exhibitionArtworks.map(a => a.artworkId || a.id);

      return this.allArtworks.filter(a => !exhibitedIds.includes(a.id));
    },

    filteredAvailableArtworks() {
      if (!this.searchQuery.trim()) {
        return this.availableArtworks;
      }

      const query = this.searchQuery.toLowerCase();

      return this.availableArtworks.filter(a =>
        a.title?.toLowerCase().includes(query) ||
        a.artistName?.toLowerCase().includes(query)
      );
    }
  },

  watch: {
    exhibitionId: {
      immediate: true,
      handler: 'loadExhibitionArtworks'
    }
  },

  mounted() {
    if (!this.effectiveReadOnly) {
      this.loadAllArtworks();
    }
  },

  methods: {
    async loadExhibitionArtworks() {
      if (!this.exhibitionId) return;

      this.isLoading = true;

      try {
        const response = await exhibitionAPI.getArtworks(this.exhibitionId);

        this.exhibitionArtworks = response.data?.success
          ? response.data.data
          : (Array.isArray(response.data) ? response.data : []);
      } catch (error) {
        console.error('Failed to load exhibition artworks:', error);
        this.showToast('Failed to load artworks', 'error');
      } finally {
        this.isLoading = false;
      }
    },

    async loadAllArtworks() {
      if (this.effectiveReadOnly) {
        return;
      }

      this.isLoadingAvailable = true;

      try {
        const response = await artworkAPI.getAll({ pageSize: 100 });
        const data = response.data?.data || response.data;

        this.allArtworks = data?.items || data || [];
      } catch (error) {
        console.error('Failed to load artworks:', error);
        this.showToast('Failed to load available artworks', 'error');
      } finally {
        this.isLoadingAvailable = false;
      }
    },

    async openAddModal() {
      if (this.effectiveReadOnly) {
        return;
      }

      this.showAddModal = true;

      if (this.allArtworks.length === 0) {
        await this.loadAllArtworks();
      }
    },

    async addArtwork(artwork) {
      if (this.effectiveReadOnly) {
        return;
      }

      this.addingArtworkId = artwork.id;

      try {
        await exhibitionAPI.addArtwork(this.exhibitionId, artwork.id);

        this.exhibitionArtworks.push({
          artworkId: artwork.id,
          title: artwork.title,
          artworkTitle: artwork.title,
          artistName: artwork.artistName
        });

        this.showToast(`"${artwork.title}" added to exhibition`, 'success');
        this.$emit('artwork-added', artwork);
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to add artwork';
        this.showToast(message, 'error');
      } finally {
        this.addingArtworkId = null;
      }
    },

    confirmRemoveArtwork(artwork) {
      if (this.effectiveReadOnly) {
        return;
      }

      this.artworkToRemove = artwork;
      this.showRemoveModal = true;
    },

    async removeArtwork() {
      if (this.effectiveReadOnly || !this.artworkToRemove) {
        this.showRemoveModal = false;
        return;
      }

      this.isRemoving = true;

      const artworkId = this.artworkToRemove.artworkId || this.artworkToRemove.id;
      const title = this.artworkToRemove.title || this.artworkToRemove.artworkTitle;

      try {
        await exhibitionAPI.removeArtwork(this.exhibitionId, artworkId);

        this.exhibitionArtworks = this.exhibitionArtworks.filter(
          a => (a.artworkId || a.id) !== artworkId
        );

        this.showRemoveModal = false;
        this.artworkToRemove = null;
        this.showToast(`"${title}" removed from exhibition`, 'success');
        this.$emit('artwork-removed', artworkId);
      } catch (error) {
        const message = error.response?.data?.message || 'Failed to remove artwork';
        this.showToast(message, 'error');
      } finally {
        this.isRemoving = false;
      }
    },

    closeAddModal() {
      this.showAddModal = false;
      this.searchQuery = '';
    },

    showToast(message, type = 'success') {
      this.toast = { show: true, message, type };

      setTimeout(() => {
        this.toast.show = false;
      }, 3000);
    }
  }
};
</script>

<style scoped>
.exhibition-artworks {
  @apply bg-white rounded-xl shadow-sm border border-gray-100 p-6;
}
</style>