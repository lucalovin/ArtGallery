<template>
  <!--
    AddEditVisitor.vue - Add/Edit Visitor Form
    Art Gallery Management System
  -->
  <div class="add-edit-visitor-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to Visitors
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Visitor' : 'Add New Visitor' }}
      </h1>
    </header>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Personal Information</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Full Name <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.name"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Email <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.email"
              type="email"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
            <input
              v-model="form.phone"
              type="tel"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Date of Birth</label>
            <input
              v-model="form.dateOfBirth"
              type="date"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Membership</h2>
        
        <div class="space-y-4">
          <div class="flex items-center">
            <input
              id="isMember"
              v-model="form.isMember"
              type="checkbox"
              class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <label for="isMember" class="ml-2 text-sm text-gray-700">This visitor is a member</label>
          </div>

          <div v-if="form.isMember" class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Membership Type</label>
              <select
                v-model="form.membershipType"
                class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              >
                <option value="Standard">Standard</option>
                <option value="Premium">Premium</option>
                <option value="VIP">VIP</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Expiry Date</label>
              <input
                v-model="form.membershipExpiry"
                type="date"
                class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              />
            </div>
          </div>
        </div>
      </div>

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
          {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';

/**
 * AddEditVisitor Page
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
      isSubmitting: false,
      form: {
        name: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        isMember: false,
        membershipType: 'Standard',
        membershipExpiry: '',
        visitCount: 0,
        lastVisit: new Date().toISOString().split('T')[0]
      }
    };
  },

  computed: {
    ...mapState({
      visitors: state => state.visitor?.visitors || []
    }),

    isEditMode() {
      return !!this.id;
    }
  },

  created() {
    if (this.isEditMode) {
      this.loadVisitor();
    }
  },

  methods: {
    ...mapActions({
      createVisitor: 'visitor/createVisitor',
      updateVisitor: 'visitor/updateVisitor'
    }),

    async loadVisitor() {
      const visitor = this.visitors.find(v => v.id === parseInt(this.id));
      if (visitor) {
        this.form = { ...visitor };
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        if (this.isEditMode) {
          await this.updateVisitor({ id: parseInt(this.id), ...this.form });
        } else {
          await this.createVisitor(this.form);
        }
        this.$router.push('/visitors');
      } catch (error) {
        console.error('Error saving visitor:', error);
        alert('Failed to save visitor. Please try again.');
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

<style scoped>
.add-edit-visitor-page {
  padding: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
}
</style>
