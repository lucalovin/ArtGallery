<template>
  <!--
    StaffManagement.vue - Staff Management Page
    Art Gallery Management System
  -->
  <div class="staff-management-page">
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Staff Management</h1>
        <p class="text-gray-500 mt-1">Manage gallery employees and departments</p>
      </div>
      <router-link
        to="/staff/new"
        class="mt-4 md:mt-0 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">➕</span>
        Add Staff
      </router-link>
    </header>

    <!-- Department Filter -->
    <div class="flex flex-wrap gap-2 mb-6">
      <button
        v-for="dept in departments"
        :key="dept"
        @click="selectedDepartment = selectedDepartment === dept ? '' : dept"
        class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
        :class="selectedDepartment === dept 
          ? 'bg-primary-600 text-white' 
          : 'bg-white text-gray-700 border border-gray-200 hover:bg-gray-50'"
      >
        {{ dept }}
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
              {{ staff.name.split(' ').map(n => n[0]).join('') }}
            </div>
            <div>
              <h3 class="font-semibold text-gray-900">{{ staff.name }}</h3>
              <p class="text-sm text-gray-500">{{ staff.position }}</p>
            </div>
          </div>
          <span 
            class="px-2 py-1 text-xs font-medium rounded-full"
            :class="staff.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'"
          >
            {{ staff.isActive ? 'Active' : 'Inactive' }}
          </span>
        </div>

        <div class="space-y-2 text-sm">
          <div class="flex items-center text-gray-600">
            <span class="mr-2">🏢</span>
            {{ staff.department }}
          </div>
          <div class="flex items-center text-gray-600">
            <span class="mr-2">📧</span>
            {{ staff.email }}
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
    </div>
  </div>
</template>

<script>
/**
 * StaffManagement Page
 */
export default {
  name: 'StaffManagementPage',

  data() {
    return {
      isLoading: true,
      selectedDepartment: '',
      departments: ['All', 'Curatorial', 'Administration', 'Security', 'Education', 'Maintenance'],
      staff: []
    };
  },

  computed: {
    filteredStaff() {
      if (!this.selectedDepartment || this.selectedDepartment === 'All') {
        return this.staff;
      }
      return this.staff.filter(s => s.department === this.selectedDepartment);
    }
  },

  created() {
    this.loadStaff();
  },

  methods: {
    async loadStaff() {
      this.isLoading = true;
      await new Promise(resolve => setTimeout(resolve, 500));
      this.staff = [
        { id: 1, name: 'Dr. Sarah Mitchell', position: 'Head Curator', department: 'Curatorial', email: 'sarah.m@gallery.com', hireDate: '2018-03-15', isActive: true },
        { id: 2, name: 'James Wilson', position: 'Security Manager', department: 'Security', email: 'james.w@gallery.com', hireDate: '2019-07-22', isActive: true },
        { id: 3, name: 'Emily Chen', position: 'Education Director', department: 'Education', email: 'emily.c@gallery.com', hireDate: '2020-01-10', isActive: true },
        { id: 4, name: 'Michael Brown', position: 'Administrator', department: 'Administration', email: 'michael.b@gallery.com', hireDate: '2017-11-05', isActive: true },
        { id: 5, name: 'Lisa Garcia', position: 'Assistant Curator', department: 'Curatorial', email: 'lisa.g@gallery.com', hireDate: '2021-04-18', isActive: true },
        { id: 6, name: 'Robert Taylor', position: 'Maintenance Lead', department: 'Maintenance', email: 'robert.t@gallery.com', hireDate: '2016-09-01', isActive: false }
      ];
      this.isLoading = false;
    },

    editStaff(staff) {
      this.$router.push({ name: 'EditStaff', params: { id: staff.id } });
    },

    deleteStaff(staff) {
      this.staff = this.staff.filter(s => s.id !== staff.id);
    },

    formatDate(date) {
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
