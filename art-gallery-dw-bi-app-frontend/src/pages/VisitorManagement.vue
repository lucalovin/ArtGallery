<template>
  <!--
    VisitorManagement.vue - Visitor Management Page
    Art Gallery Management System
  -->
  <div class="visitor-management-page">
    <header class="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Visitor Directory</h1>
        <p class="text-gray-500 mt-1">View gallery visitors and their memberships</p>
      </div>
      <router-link
        to="/reviews/new"
        class="mt-4 md:mt-0 inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 font-medium"
      >
        <span class="mr-2">➕</span>
        Add Review (New Visitor)
      </router-link>
    </header>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Total Visitors</p>
        <p class="text-2xl font-bold text-gray-900">{{ stats.total }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Members</p>
        <p class="text-2xl font-bold text-primary-600">{{ stats.members }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">Standard</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.standard }}</p>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <p class="text-sm text-gray-500">VIP</p>
        <p class="text-2xl font-bold text-blue-600">{{ stats.vip }}</p>
      </div>
    </div>

    <!-- Search & Filter -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1 relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search visitors..."
            class="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
          />
          <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
        </div>
        <select
          v-model="membershipFilter"
          class="px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary-500"
        >
          <option value="">All Types</option>
          <option value="Standard">Standard</option>
          <option value="VIP">VIP</option>
          <option value="Student">Student</option>
          <option value="none">No Membership</option>
        </select>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="n in 5" :key="n" class="bg-white rounded-xl p-4 animate-pulse">
        <div class="h-4 bg-gray-200 rounded w-1/3"></div>
      </div>
    </div>

    <!-- Visitor List -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Visitor</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Email</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Phone</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Membership</th>
            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Join Date</th>
            <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="visitor in filteredVisitors" :key="visitor.id" class="hover:bg-gray-50">
            <td class="px-6 py-4">
              <div class="flex items-center space-x-3">
                <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center text-primary-600 font-medium">
                  {{ (visitor.name || '?').charAt(0) }}
                </div>
                <span class="font-medium text-gray-900">{{ visitor.name }}</span>
              </div>
            </td>
            <td class="px-6 py-4 text-gray-600">{{ visitor.email || '-' }}</td>
            <td class="px-6 py-4 text-gray-600">{{ visitor.phone || '-' }}</td>
            <td class="px-6 py-4">
              <span 
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="visitor.membershipType ? 'bg-primary-100 text-primary-700' : 'bg-gray-100 text-gray-600'"
              >
                {{ visitor.membershipType || 'None' }}
              </span>
            </td>
            <td class="px-6 py-4 text-gray-600">{{ formatDate(visitor.joinDate) }}</td>
            <td class="px-6 py-4 text-right">
              <button @click="editVisitor(visitor)" class="p-1 hover:text-primary-600">✏️</button>
              <button @click="deleteVisitor(visitor)" class="p-1 hover:text-red-600">🗑️</button>
            </td>
          </tr>
          <tr v-if="filteredVisitors.length === 0">
            <td colspan="6" class="px-6 py-8 text-center text-gray-500">
              No visitors found. Visitors are created when adding reviews.
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';

/**
 * VisitorManagement Page
 */
export default {
  name: 'VisitorManagementPage',

  data() {
    return {
      isLoading: true,
      searchQuery: '',
      membershipFilter: ''
    };
  },

  computed: {
    ...mapState({
      visitors: state => state.visitor?.visitors || []
    }),

    stats() {
      return {
        total: this.visitors.length,
        members: this.visitors.filter(v => v.membershipType && v.membershipType !== 'None').length,
        standard: this.visitors.filter(v => v.membershipType === 'Standard').length,
        vip: this.visitors.filter(v => v.membershipType === 'VIP').length
      };
    },

    filteredVisitors() {
      let result = [...this.visitors];
      
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        result = result.filter(v => 
          (v.name && v.name.toLowerCase().includes(query)) || 
          (v.email && v.email.toLowerCase().includes(query))
        );
      }
      
      if (this.membershipFilter === 'none') {
        result = result.filter(v => !v.membershipType);
      } else if (this.membershipFilter) {
        result = result.filter(v => v.membershipType === this.membershipFilter);
      }
      
      return result;
    }
  },

  created() {
    this.loadVisitors();
  },

  methods: {
    ...mapActions({
      fetchVisitors: 'visitor/fetchVisitors',
      deleteVisitorAction: 'visitor/deleteVisitor'
    }),

    async loadVisitors() {
      this.isLoading = true;
      try {
        await this.fetchVisitors();
      } catch (error) {
        console.error('Error loading visitors:', error);
      } finally {
        this.isLoading = false;
      }
    },

    editVisitor(visitor) {
      this.$router.push({ name: 'EditVisitor', params: { id: visitor.id } });
    },

    async deleteVisitor(visitor) {
      if (confirm(`Are you sure you want to delete "${visitor.name}"?`)) {
        try {
          await this.deleteVisitorAction(visitor.id);
        } catch (error) {
          console.error('Error deleting visitor:', error);
        }
      }
    },

    formatDate(date) {
      return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
  }
};
</script>

<style scoped>
.visitor-management-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
