<template>
  <!--
    AddEditExhibition.vue - Add/Edit Exhibition Form
    Art Gallery Management System
  -->
  <div class="add-edit-exhibition-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Exhibition' : 'New Exhibition' }}
      </h1>
    </header>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Exhibition Details</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.title"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              placeholder="Exhibition title"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Start Date <span class="text-red-500">*</span></label>
            <input
              v-model="form.startDate"
              type="date"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">End Date <span class="text-red-500">*</span></label>
            <input
              v-model="form.endDate"
              type="date"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Exhibitor <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.exhibitorId"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="" disabled>Select an exhibitor</option>
              <option v-for="exhibitor in exhibitors" :key="exhibitor.id" :value="exhibitor.id">
                {{ exhibitor.name }}
              </option>
            </select>
          </div>

          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              v-model="form.description"
              rows="4"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
              placeholder="Describe the exhibition..."
            ></textarea>
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
import { mapActions } from 'vuex';

/**
 * AddEditExhibition Page
 * Form respects Exhibition table schema with ExhibitorId FK
 */
export default {
  name: 'AddEditExhibitionPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      isSubmitting: false,
      exhibitors: [],
      form: {
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        exhibitorId: ''
      }
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    }
  },

  async created() {
    await this.loadLookups();
    if (this.isEditMode) {
      await this.loadExhibition();
    }
  },

  methods: {
    ...mapActions({
      createExhibition: 'exhibition/createExhibition',
      updateExhibition: 'exhibition/updateExhibition',
      fetchExhibitionById: 'exhibition/fetchExhibitionById'
    }),

    async loadLookups() {
      try {
        const response = await this.$api.lookups.getExhibitors();
        // API returns { success: true, data: [...] }
        if (response.data?.success && response.data?.data) {
          this.exhibitors = response.data.data;
        } else if (Array.isArray(response.data)) {
          this.exhibitors = response.data;
        } else {
          this.exhibitors = [];
        }
      } catch (error) {
        console.error('Failed to load lookups:', error);
        this.exhibitors = [];
      }
    },

    async loadExhibition() {
      try {
        const exhibition = await this.fetchExhibitionById(this.id);
        this.form = {
          title: exhibition.title || '',
          description: exhibition.description || '',
          startDate: exhibition.startDate ? exhibition.startDate.split('T')[0] : '',
          endDate: exhibition.endDate ? exhibition.endDate.split('T')[0] : '',
          exhibitorId: exhibition.exhibitorId || ''
        };
      } catch (error) {
        console.error('Failed to load exhibition:', error);
        this.$router.push('/exhibitions');
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        const submitData = {
          title: this.form.title,
          description: this.form.description || null,
          startDate: this.form.startDate,
          endDate: this.form.endDate,
          exhibitorId: parseInt(this.form.exhibitorId)
        };

        if (this.isEditMode) {
          await this.updateExhibition({
            id: this.id,
            ...submitData
          });
        } else {
          await this.createExhibition(submitData);
        }
        this.$router.push('/exhibitions');
      } catch (error) {
        console.error('Failed to save exhibition:', error);
        alert('Failed to save exhibition. Please try again.');
      } finally {
        this.isSubmitting = false;
      }
    },

    goBack() {
      this.$router.push('/exhibitions');
    }
  }
};
</script>

<style scoped>
.add-edit-exhibition-page {
  padding: 1.5rem;
  max-width: 1000px;
  margin: 0 auto;
}
</style>
