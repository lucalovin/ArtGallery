<template>
  <!--
    StaffManagement.vue - Staff Management Page
    Art Gallery Management System
    Displays staff with fields: Name, Role, HireDate, CertificationLevel
  -->
  <div class="staff-management-page">
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Staff Management</h1>
        <p class="text-gray-500 mt-1">Manage gallery employees</p>
      </div>
      <router-link
        to="/staff/new"
        class="mt-4 md:mt-0 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">➕</span>
        Add Staff
      </router-link>
    </header>

    <!-- Role Filter -->
    <div class="flex flex-wrap gap-2 mb-6">
      <button
        v-for="role in roles"
        :key="role"
        @click="selectedRole = selectedRole === role ? '' : role"
        class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
        :class="selectedRole === role 
          ? 'bg-primary-600 text-white' 
          : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'"
      >
        {{ role }}
      </button>
    </div>

    <!-- Staff Grid -->
    <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="n in 6" :key="n" class="bg-white rounded-xl p-6 animate-pulse">
        <div class="flex items-center space-x-4">
          <div class="w-16 h-16 bg-gray-200 rounded-full"></div>
          <div class="space-y-2 flex-1">
            <div class="h-4 bg-gray-200 rounded w-3/4"></div>
            <div class="h-3 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="staff in filteredStaff" 
        :key="staff.id"
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start justify-between mb-4">
          <div class="flex items-center space-x-4">
            <div class="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center text-2xl text-primary-600 font-bold">
              {{ getInitials(staff.name) }}
            </div>
            <div>
              <h3 class="font-semibold text-gray-900">{{ staff.name }}</h3>
              <p class="text-sm text-gray-500">{{ staff.role }}</p>
            </div>
          </div>
          <span 
            v-if="staff.certificationLevel"
            class="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-700"
          >
            {{ staff.certificationLevel }}
          </span>
        </div>

        <div class="space-y-2 text-sm">
          <div class="flex items-center text-gray-600">
            <span class="mr-2">💼</span>
            {{ staff.role }}
          </div>
          <div class="flex items-center text-gray-600">
            <span class="mr-2">📅</span>
            Joined {{ formatDate(staff.hireDate) }}
          </div>
        </div>

        <div class="mt-4 pt-4 border-t border-gray-100 flex justify-end space-x-2">
          <button 
            @click="editStaff(staff)"
            class="px-3 py-1 text-sm text-primary-600 hover:bg-primary-50 rounded-lg"
          >
            Edit
          </button>
          <button 
            @click="deleteStaff(staff)"
            class="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded-lg"
          >
            Remove
          </button>
        </div>
      </div>

      <div v-if="filteredStaff.length === 0" class="col-span-full text-center py-12 text-gray-500">
        No staff members found.
        <router-link to="/staff/new" class="text-primary-600 hover:text-primary-700 ml-1">
          Add one now
        </router-link>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * StaffManagement Page
 * Displays staff with backend fields: Name, Role, HireDate, CertificationLevel
 */
export default {
  name: 'StaffManagementPage',

  data() {
    return {
      isLoading: true,
      selectedRole: '',
      roles: ['All', 'Curator', 'Restorer', 'Security', 'Guide', 'Administrator', 'Maintenance'],
      staff: []
    };
  },

  computed: {
    filteredStaff() {
      if (!this.selectedRole || this.selectedRole === 'All') {
        return this.staff;
      }
      return this.staff.filter(s => s.role === this.selectedRole);
    }
  },

  created() {
    this.loadStaff();
  },

  methods: {
    async loadStaff() {
      this.isLoading = true;
      try {
        const response = await this.$api.staff?.getStaff?.();
        // API returns { success: true, data: { items: [...], totalCount: N } }
        if (response?.data?.success && response.data?.data) {
          const data = response.data.data;
          const items = Array.isArray(data.items) ? data.items : (Array.isArray(data) ? data : []);
          this.staff = items.map(s => ({
            id: s.id,
            name: s.name,
            role: s.role,
            hireDate: s.hireDate,
            certificationLevel: s.certificationLevel
          }));
        } else if (Array.isArray(response?.data)) {
          this.staff = response.data;
        }
      } catch (error) {
        console.error('Failed to load staff:', error);
        this.staff = [];
      } finally {
        this.isLoading = false;
      }
    },

    editStaff(staff) {
      this.$router.push({ name: 'EditStaff', params: { id: staff.id } });
    },

    async deleteStaff(staff) {
      if (!confirm(`Are you sure you want to remove "${staff.name}"?`)) return;
      
      try {
        await this.$api.staff?.deleteStaff?.(staff.id);
        this.staff = this.staff.filter(s => s.id !== staff.id);
      } catch (error) {
        console.error('Failed to delete staff:', error);
        alert('Failed to remove staff member. Please try again.');
      }
    },

    getInitials(name) {
      if (!name) return '?';
      return name.split(' ').map(n => n[0]).join('').toUpperCase();
    },

    formatDate(date) {
      if (!date) return 'N/A';
      return new Date(date).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
    }
  }
};
</script>

<style scoped>
.staff-management-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
