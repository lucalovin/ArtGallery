<template>
  <!--
    AddEditStaff.vue - Add/Edit Staff Form
    Art Gallery Management System
    Matches backend CreateStaffDto: Name, Role, HireDate, CertificationLevel
  -->
  <div class="add-edit-staff-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to Staff
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">
        {{ isEditMode ? 'Edit Staff Member' : 'Add Staff Member' }}
      </h1>
    </header>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Staff Information</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Full Name <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="e.g., John Smith"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Role <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.role"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select Role</option>
              <option v-for="role in roles" :key="role" :value="role">{{ role }}</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Hire Date <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.hireDate"
              type="date"
              required
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Certification Level</label>
            <select
              v-model="form.certificationLevel"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="">None</option>
              <option value="Basic">Basic</option>
              <option value="Intermediate">Intermediate</option>
              <option value="Advanced">Advanced</option>
              <option value="Expert">Expert</option>
            </select>
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
/**
 * AddEditStaff Page
 * Matches backend DTO: Name, Role, HireDate, CertificationLevel
 */
export default {
  name: 'AddEditStaffPage',

  props: {
    id: {
      type: [String, Number],
      default: null
    }
  },

  data() {
    return {
      isSubmitting: false,
      roles: ['Curator', 'Restorer', 'Security', 'Guide', 'Administrator', 'Maintenance'],
      form: {
        name: '',
        role: '',
        hireDate: '',
        certificationLevel: ''
      }
    };
  },

  computed: {
    isEditMode() {
      return !!this.id;
    }
  },

  created() {
    if (this.isEditMode) {
      this.loadStaff();
    }
  },

  methods: {
    async loadStaff() {
      try {
        const response = await this.$api.staff?.getStaffById?.(this.$route.params.id);
        if (response?.data?.success && response.data?.data) {
          const staff = response.data.data;
          this.form = {
            name: staff.name || '',
            role: staff.role || '',
            hireDate: staff.hireDate ? staff.hireDate.split('T')[0] : '',
            certificationLevel: staff.certificationLevel || ''
          };
        }
      } catch (error) {
        console.error('Failed to load staff:', error);
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        // Build DTO matching backend CreateStaffDto/UpdateStaffDto
        const submitData = {
          name: this.form.name,
          role: this.form.role,
          hireDate: this.form.hireDate,
          certificationLevel: this.form.certificationLevel || null
        };

        if (this.isEditMode) {
          await this.$api.staff?.updateStaff?.(this.$route.params.id, submitData);
        } else {
          await this.$api.staff?.createStaff?.(submitData);
        }
        this.$router.push('/staff');
      } catch (error) {
        console.error('Failed to save staff:', error);
        alert('Failed to save staff. Please check all required fields.');
      } finally {
        this.isSubmitting = false;
      }
    },

    goBack() {
      this.$router.push('/staff');
    }
  }
};
</script>

<style scoped>
.add-edit-staff-page {
  padding: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
}
</style>
