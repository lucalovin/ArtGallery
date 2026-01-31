<template>
  <!--
    AddEditReview.vue - Add/Edit Gallery Review Page
    Art Gallery Management System
    
    This page allows creating a new review with optional inline visitor creation.
    A review must reference either an artwork or an exhibition (or both).
  -->
  <div class="add-edit-review-page max-w-4xl mx-auto">
    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Review' : 'Add New Review' }}
      </h1>
      <p class="text-gray-500 mt-1">
        {{ isEditMode ? 'Update review information' : 'Create a new gallery review' }}
      </p>
    </header>

    <!-- Error Alert -->
    <div v-if="error" class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
      <p class="text-red-800 font-medium">{{ error }}</p>
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Visitor Section -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
          Visitor Information
        </h2>

        <!-- Visitor Selection Mode -->
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">Visitor</label>
          <div class="flex space-x-4">
            <label class="inline-flex items-center">
              <input
                v-model="visitorMode"
                type="radio"
                value="existing"
                class="form-radio text-primary-600"
              />
              <span class="ml-2">Select Existing Visitor</span>
            </label>
            <label class="inline-flex items-center">
              <input
                v-model="visitorMode"
                type="radio"
                value="new"
                class="form-radio text-primary-600"
              />
              <span class="ml-2">Create New Visitor</span>
            </label>
          </div>
        </div>

        <!-- Existing Visitor Selection -->
        <div v-if="visitorMode === 'existing'" class="space-y-4">
          <div>
            <label for="visitorId" class="block text-sm font-medium text-gray-700 mb-1">
              Select Visitor <span class="text-red-500">*</span>
            </label>
            <select
              id="visitorId"
              v-model="form.visitorId"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.visitorId }"
              required
            >
              <option value="">-- Select a Visitor --</option>
              <option v-for="visitor in visitors" :key="visitor.id" :value="visitor.id">
                {{ visitor.name }} {{ visitor.email ? `(${visitor.email})` : '' }}
              </option>
            </select>
            <p v-if="errors.visitorId" class="mt-1 text-sm text-red-600">{{ errors.visitorId }}</p>
          </div>
        </div>

        <!-- New Visitor Form -->
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="visitorName" class="block text-sm font-medium text-gray-700 mb-1">
              Visitor Name <span class="text-red-500">*</span>
            </label>
            <input
              id="visitorName"
              v-model="newVisitor.name"
              type="text"
              class="form-input w-full"
              :class="{ 'border-red-300': errors.visitorName }"
              placeholder="Enter visitor name"
              required
            />
            <p v-if="errors.visitorName" class="mt-1 text-sm text-red-600">{{ errors.visitorName }}</p>
          </div>

          <div>
            <label for="visitorEmail" class="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              id="visitorEmail"
              v-model="newVisitor.email"
              type="email"
              class="form-input w-full"
              placeholder="visitor@example.com"
            />
          </div>

          <div>
            <label for="visitorPhone" class="block text-sm font-medium text-gray-700 mb-1">
              Phone
            </label>
            <input
              id="visitorPhone"
              v-model="newVisitor.phone"
              type="tel"
              class="form-input w-full"
              placeholder="+40 123 456 789"
            />
          </div>

          <div>
            <label for="membershipType" class="block text-sm font-medium text-gray-700 mb-1">
              Membership Type
            </label>
            <select
              id="membershipType"
              v-model="newVisitor.membershipType"
              class="form-input w-full"
            >
              <option value="">None</option>
              <option value="Standard">Standard</option>
              <option value="VIP">VIP</option>
              <option value="Student">Student</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Review Subject Section -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
          Review Subject
        </h2>
        <p class="text-sm text-gray-500 mb-4">Select at least one artwork or exhibition to review.</p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="artworkId" class="block text-sm font-medium text-gray-700 mb-1">
              Artwork
            </label>
            <select
              id="artworkId"
              v-model="form.artworkId"
              class="form-input w-full"
            >
              <option value="">-- No Artwork Selected --</option>
              <option v-for="artwork in artworks" :key="artwork.id" :value="artwork.id">
                {{ artwork.title }} {{ artwork.artistName ? `by ${artwork.artistName}` : '' }}
              </option>
            </select>
          </div>

          <div>
            <label for="exhibitionId" class="block text-sm font-medium text-gray-700 mb-1">
              Exhibition
            </label>
            <select
              id="exhibitionId"
              v-model="form.exhibitionId"
              class="form-input w-full"
            >
              <option value="">-- No Exhibition Selected --</option>
              <option v-for="exhibition in exhibitions" :key="exhibition.id" :value="exhibition.id">
                {{ exhibition.title }}
              </option>
            </select>
          </div>
        </div>
        <p v-if="errors.subject" class="mt-2 text-sm text-red-600">{{ errors.subject }}</p>
      </div>

      <!-- Review Details Section -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
          Review Details
        </h2>

        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Rating <span class="text-red-500">*</span>
            </label>
            <div class="flex space-x-2">
              <button
                v-for="star in 5"
                :key="star"
                type="button"
                @click="form.rating = star"
                class="text-3xl transition-colors"
                :class="star <= form.rating ? 'text-yellow-400' : 'text-gray-300'"
              >
                ⭐
              </button>
            </div>
            <p v-if="errors.rating" class="mt-1 text-sm text-red-600">{{ errors.rating }}</p>
          </div>

          <div>
            <label for="reviewText" class="block text-sm font-medium text-gray-700 mb-1">
              Review Text
            </label>
            <textarea
              id="reviewText"
              v-model="form.reviewText"
              rows="4"
              class="form-input w-full"
              placeholder="Share your experience..."
              maxlength="256"
            ></textarea>
            <p class="mt-1 text-sm text-gray-500">{{ (form.reviewText || '').length }}/256 characters</p>
          </div>

          <div>
            <label for="reviewDate" class="block text-sm font-medium text-gray-700 mb-1">
              Review Date <span class="text-red-500">*</span>
            </label>
            <input
              id="reviewDate"
              v-model="form.reviewDate"
              type="date"
              class="form-input w-full"
              :max="today"
              required
            />
            <p v-if="errors.reviewDate" class="mt-1 text-sm text-red-600">{{ errors.reviewDate }}</p>
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex justify-end space-x-4">
        <router-link
          to="/reviews"
          class="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </router-link>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
        >
          {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update Review' : 'Create Review') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { visitorAPI } from '@/api/visitorAPI';
import { artworkAPI } from '@/api/artworkAPI';
import { exhibitionAPI } from '@/api/exhibitionAPI';

export default {
  name: 'AddEditReviewPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      visitorMode: 'existing',
      form: {
        visitorId: '',
        artworkId: '',
        exhibitionId: '',
        rating: 5,
        reviewText: '',
        reviewDate: new Date().toISOString().split('T')[0]
      },
      newVisitor: {
        name: '',
        email: '',
        phone: '',
        membershipType: ''
      },
      visitors: [],
      artworks: [],
      exhibitions: [],
      errors: {},
      error: '',
      isSubmitting: false,
      isLoading: true
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    },
    today() {
      return new Date().toISOString().split('T')[0];
    }
  },

  async mounted() {
    await this.loadLookups();
    if (this.isEditMode) {
      await this.loadReview();
    }
    this.isLoading = false;
  },

  methods: {
    async loadLookups() {
      try {
        const [visitorsRes, artworksRes, exhibitionsRes] = await Promise.all([
          visitorAPI.getAll(),
          artworkAPI.getAll(),
          exhibitionAPI.getAll()
        ]);

        // Extract data from paginated responses
        this.visitors = this.extractItems(visitorsRes);
        this.artworks = this.extractItems(artworksRes);
        this.exhibitions = this.extractItems(exhibitionsRes);
      } catch (error) {
        console.error('Error loading lookups:', error);
      }
    },

    extractItems(response) {
      if (response.data?.success && response.data?.data) {
        const data = response.data.data;
        return Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
      }
      return Array.isArray(response.data) ? response.data : [];
    },

    async loadReview() {
      try {
        const response = await visitorAPI.getReviewById(this.id);
        const review = response.data?.data || response.data;
        
        this.form = {
          visitorId: review.visitorId,
          artworkId: review.artworkId || '',
          exhibitionId: review.exhibitionId || '',
          rating: review.rating,
          reviewText: review.reviewText || '',
          reviewDate: review.reviewDate ? review.reviewDate.split('T')[0] : ''
        };
        this.visitorMode = 'existing';
      } catch (error) {
        console.error('Error loading review:', error);
        this.error = 'Failed to load review';
      }
    },

    validate() {
      this.errors = {};

      // Validate visitor
      if (this.visitorMode === 'existing') {
        if (!this.form.visitorId) {
          this.errors.visitorId = 'Please select a visitor';
        }
      } else {
        if (!this.newVisitor.name?.trim()) {
          this.errors.visitorName = 'Visitor name is required';
        }
      }

      // Validate subject
      if (!this.form.artworkId && !this.form.exhibitionId) {
        this.errors.subject = 'Please select at least an artwork or exhibition';
      }

      // Validate rating
      if (!this.form.rating || this.form.rating < 1 || this.form.rating > 5) {
        this.errors.rating = 'Rating must be between 1 and 5';
      }

      // Validate date
      if (!this.form.reviewDate) {
        this.errors.reviewDate = 'Review date is required';
      }

      return Object.keys(this.errors).length === 0;
    },

    async handleSubmit() {
      if (!this.validate()) return;

      this.isSubmitting = true;
      this.error = '';

      try {
        const payload = {
          artworkId: this.form.artworkId || null,
          exhibitionId: this.form.exhibitionId || null,
          rating: this.form.rating,
          reviewText: this.form.reviewText || null,
          reviewDate: new Date(this.form.reviewDate).toISOString()
        };

        if (this.visitorMode === 'existing') {
          payload.visitorId = parseInt(this.form.visitorId);
        } else {
          payload.newVisitor = {
            name: this.newVisitor.name,
            email: this.newVisitor.email || null,
            phone: this.newVisitor.phone || null,
            membershipType: this.newVisitor.membershipType || null
          };
        }

        if (this.isEditMode) {
          await visitorAPI.updateReview(this.id, {
            artworkId: payload.artworkId,
            exhibitionId: payload.exhibitionId,
            rating: payload.rating,
            reviewText: payload.reviewText,
            reviewDate: payload.reviewDate
          });
        } else {
          await visitorAPI.createReview(payload);
        }

        this.$router.push('/reviews');
      } catch (error) {
        console.error('Error saving review:', error);
        this.error = error.response?.data?.message || 'Failed to save review';
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script>
