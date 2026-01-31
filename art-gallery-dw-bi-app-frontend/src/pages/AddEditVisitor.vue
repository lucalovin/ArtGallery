<template>
  <!--
    AddEditVisitor.vue - Edit Visitor Form
    Art Gallery Management System
    
    Visitors are primarily created through Reviews.
    This page is for editing existing visitor information.
  -->
  <div class="add-edit-visitor-page max-w-2xl mx-auto">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to Visitors
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">Edit Visitor</h1>
      <p class="text-gray-500 mt-1">Update visitor information</p>
    </header>

    <!-- Loading -->
    <div v-if="isLoading" class="text-center py-8">
      <p class="text-gray-500">Loading visitor...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
      <p class="text-red-800">{{ error }}</p>
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="handleSubmit" class="space-y-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Personal Information</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Name -->
          <div class="md:col-span-2">
            <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
              Full Name <span class="text-red-500">*</span>
            </label>
            <input
              id="name"
              v-model="form.name"
              type="text"
              required
              maxlength="128"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              placeholder="Enter full name"
            />
          </div>

          <!-- Email -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              maxlength="128"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              placeholder="visitor@example.com"
            />
          </div>

          <!-- Phone -->
          <div>
            <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
            <input
              id="phone"
              v-model="form.phone"
              type="tel"
              maxlength="32"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              placeholder="+40 123 456 789"
            />
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Membership</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Membership Type -->
          <div>
            <label for="membershipType" class="block text-sm font-medium text-gray-700 mb-1">
              Membership Type
            </label>
            <select
              id="membershipType"
              v-model="form.membershipType"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="">None</option>
              <option value="Standard">Standard</option>
              <option value="VIP">VIP</option>
              <option value="Student">Student</option>
            </select>
          </div>

          <!-- Join Date -->
          <div>
            <label for="joinDate" class="block text-sm font-medium text-gray-700 mb-1">
              Join Date
            </label>
            <input
              id="joinDate"
              v-model="form.joinDate"
              type="date"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex justify-end space-x-4">
        <button
          type="button"
          @click="goBack"
          class="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 font-medium"
        >
          Cancel
        </button>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium disabled:opacity-50"
        >
          {{ isSubmitting ? 'Saving...' : 'Update Visitor' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { visitorAPI } from '@/api/visitorAPI';

/**
 * AddEditVisitor Page - For editing existing visitors
 * Visitors are created through the Review flow
 */
export default {
  name: 'AddEditVisitorPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      isLoading: true,
      isSubmitting: false,
      error: '',
      form: {
        name: '',
        email: '',
        phone: '',
        membershipType: '',
        joinDate: ''
      }
    };
  },

  async mounted() {
    if (!this.id) {
      // No ID provided - redirect to reviews to create new visitor
      this.$router.push('/reviews/new');
      return;
    }
    await this.loadVisitor();
  },

  methods: {
    async loadVisitor() {
      try {
        this.isLoading = true;
        const response = await visitorAPI.getById(this.id);
        const visitor = response.data?.data || response.data;
        
        this.form = {
          name: visitor.name || '',
          email: visitor.email || '',
          phone: visitor.phone || '',
          membershipType: visitor.membershipType || '',
          joinDate: visitor.joinDate ? visitor.joinDate.split('T')[0] : ''
        };
      } catch (error) {
        console.error('Error loading visitor:', error);
        this.error = 'Failed to load visitor';
      } finally {
        this.isLoading = false;
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      this.error = '';

      try {
        const payload = {
          name: this.form.name,
          email: this.form.email || null,
          phone: this.form.phone || null,
          membershipType: this.form.membershipType || null,
          joinDate: this.form.joinDate ? new Date(this.form.joinDate).toISOString() : null
        };

        await visitorAPI.update(this.id, payload);
        this.$router.push('/visitors');
      } catch (error) {
        console.error('Error saving visitor:', error);
        this.error = error.response?.data?.message || 'Failed to save visitor';
      } finally {
        this.isSubmitting = false;
      }
    },

    goBack() {
      this.$router.push('/visitors');
    }
  }
};
</script>
