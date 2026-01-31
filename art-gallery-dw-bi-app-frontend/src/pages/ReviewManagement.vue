<template>
  <!--
    ReviewManagement.vue - Gallery Review Management Page
    Art Gallery Management System
  -->
  <div class="review-management-page">
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Gallery Reviews</h1>
        <p class="text-gray-500 mt-1">Manage visitor reviews for artworks and exhibitions</p>
      </div>
      <router-link
        to="/reviews/new"
        class="mt-4 md:mt-0 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">➕</span>
        Add Review
      </router-link>
    </header>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Reviews</p>
        <p class="text-2xl font-bold text-gray-900">{{ stats.total }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Average Rating</p>
        <p class="text-2xl font-bold text-primary-600">{{ stats.averageRating.toFixed(1) }} ⭐</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">5-Star Reviews</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.fiveStarCount }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">This Month</p>
        <p class="text-2xl font-bold text-blue-600">{{ stats.thisMonth }}</p>
      </div>
    </div>

    <!-- Search & Filter -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1 relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search reviews..."
            class="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          />
          <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
        </div>
        <select
          v-model="ratingFilter"
          class="px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
        >
          <option value="">All Ratings</option>
          <option value="5">5 Stars</option>
          <option value="4">4 Stars</option>
          <option value="3">3 Stars</option>
          <option value="2">2 Stars</option>
          <option value="1">1 Star</option>
        </select>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="n in 5" :key="n" class="bg-white rounded-xl p-4 animate-pulse">
        <div class="h-4 bg-gray-200 rounded w-1/3"></div>
      </div>
    </div>

    <!-- Review List -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Visitor</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Subject</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Rating</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Review</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Date</th>
            <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="review in filteredReviews" :key="review.id" class="hover:bg-gray-50">
            <td class="px-6 py-4">
              <div class="flex items-center space-x-3">
                <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center text-primary-600 font-medium">
                  {{ review.visitorName.charAt(0) }}
                </div>
                <div>
                  <span class="font-medium text-gray-900">{{ review.visitorName }}</span>
                  <p v-if="review.visitorEmail" class="text-sm text-gray-500">{{ review.visitorEmail }}</p>
                </div>
              </div>
            </td>
            <td class="px-6 py-4">
              <div>
                <span v-if="review.artworkTitle" class="text-gray-900">🖼️ {{ review.artworkTitle }}</span>
                <span v-if="review.exhibitionTitle" class="text-gray-900">🎭 {{ review.exhibitionTitle }}</span>
              </div>
            </td>
            <td class="px-6 py-4">
              <span class="text-yellow-500">{{ '⭐'.repeat(review.rating) }}</span>
            </td>
            <td class="px-6 py-4 text-gray-600 max-w-xs truncate">{{ review.reviewText || '-' }}</td>
            <td class="px-6 py-4 text-gray-600">{{ formatDate(review.reviewDate) }}</td>
            <td class="px-6 py-4 text-right">
              <button @click="editReview(review)" class="p-1 hover:text-primary-600">✏️</button>
              <button @click="deleteReview(review)" class="p-1 hover:text-red-600">🗑️</button>
            </td>
          </tr>
          <tr v-if="filteredReviews.length === 0">
            <td colspan="6" class="px-6 py-8 text-center text-gray-500">
              No reviews found. Add your first review!
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { visitorAPI } from '@/api/visitorAPI';

export default {
  name: 'ReviewManagementPage',

  data() {
    return {
      reviews: [],
      isLoading: true,
      searchQuery: '',
      ratingFilter: ''
    };
  },

  computed: {
    stats() {
      const total = this.reviews.length;
      const averageRating = total > 0 
        ? this.reviews.reduce((sum, r) => sum + r.rating, 0) / total 
        : 0;
      const fiveStarCount = this.reviews.filter(r => r.rating === 5).length;
      const today = new Date();
      const thisMonth = this.reviews.filter(r => {
        const date = new Date(r.reviewDate);
        return date.getMonth() === today.getMonth() && date.getFullYear() === today.getFullYear();
      }).length;

      return { total, averageRating, fiveStarCount, thisMonth };
    },

    filteredReviews() {
      let result = [...this.reviews];
      
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(r => 
          r.visitorName.toLowerCase().includes(query) ||
          (r.reviewText && r.reviewText.toLowerCase().includes(query)) ||
          (r.artworkTitle && r.artworkTitle.toLowerCase().includes(query)) ||
          (r.exhibitionTitle && r.exhibitionTitle.toLowerCase().includes(query))
        );
      }
      
      if (this.ratingFilter) {
        result = result.filter(r => r.rating === parseInt(this.ratingFilter));
      }
      
      return result;
    }
  },

  async mounted() {
    await this.loadReviews();
  },

  methods: {
    async loadReviews() {
      try {
        this.isLoading = true;
        const response = await visitorAPI.getAllReviews();
        // API returns { success: true, data: { items: [...], totalCount: N } }
        let reviews = [];
        if (response.data?.success && response.data?.data) {
          const data = response.data.data;
          reviews = Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
        } else if (Array.isArray(response.data)) {
          reviews = response.data;
        }
        this.reviews = reviews;
      } catch (error) {
        console.error('Error loading reviews:', error);
        this.reviews = [];
      } finally {
        this.isLoading = false;
      }
    },

    formatDate(dateStr) {
      if (!dateStr) return '-';
      return new Date(dateStr).toLocaleDateString();
    },

    editReview(review) {
      this.$router.push(`/reviews/${review.id}/edit`);
    },

    async deleteReview(review) {
      if (!confirm(`Delete review by ${review.visitorName}?`)) return;
      
      try {
        await visitorAPI.deleteReview(review.id);
        await this.loadReviews();
      } catch (error) {
        console.error('Error deleting review:', error);
        alert('Failed to delete review');
      }
    }
  }
};
</script>
