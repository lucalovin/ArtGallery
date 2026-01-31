<template>
  <!--
    AddEditStaff.vue - Add/Edit Staff Form
    Art Gallery Management System
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
            <label class="block text-sm font-medium text-gray-700 mb-1">Address</label>
            <input
              v-model="form.address"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Employment Details</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Position</label>
            <input
              v-model="form.position"
              type="text"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Department</label>
            <select
              v-model="form.department"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select Department</option>
              <option v-for="dept in departments" :key="dept" :value="dept">{{ dept }}</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Hire Date</label>
            <input
              v-model="form.hireDate"
              type="date"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Salary</label>
            <input
              v-model.number="form.salary"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div class="flex items-center">
            <input
              id="isActive"
              v-model="form.isActive"
              type="checkbox"
              class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <label for="isActive" class="ml-2 text-sm text-gray-700">Active Employee</label>
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
      departments: ['Curatorial', 'Administration', 'Security', 'Education', 'Maintenance'],
      form: {
        name: '',
        email: '',
        phone: '',
        address: '',
        position: '',
        department: '',
        hireDate: '',
        salary: null,
        isActive: true
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
            name: staff.name || `${staff.firstName} ${staff.lastName}`,
            email: staff.email || '',
            phone: staff.phone || '',
            address: staff.address || '',
            position: staff.position || staff.jobTitle || '',
            department: staff.department || '',
            hireDate: staff.hireDate || staff.startDate || '',
            salary: staff.salary || 0,
            isActive: staff.isActive !== false
          };
        }
      } catch (error) {
        console.error('Failed to load staff:', error);
      }
    },

    async handleSubmit() {
      this.isSubmitting = true;
      try {
        if (this.isEditMode) {
          await this.$api.staff?.updateStaff?.(this.$route.params.id, this.form);
        } else {
          await this.$api.staff?.createStaff?.(this.form);
        }
        this.$router.push('/staff');
      } catch (error) {
        console.error('Failed to save staff:', error);
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
