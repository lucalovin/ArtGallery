<template>
  <!--
    NavigationMenu.vue - Main Navigation Component
    Art Gallery Management System
  -->
  <header class="bg-white shadow-md sticky top-0 z-40">
    <nav class="container mx-auto px-4 lg:px-8">
      <div class="flex items-center justify-between h-16">
        <!-- Logo and Brand -->
        <div class="flex items-center">
          <router-link
            to="/"
            class="flex items-center space-x-3"
            @click="closeMobileMenu"
          >
            <!-- Gallery Icon -->
            <div class="w-10 h-10 bg-primary-600 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>

            <span class="text-xl font-display font-bold text-gray-900 hidden sm:block">
              Art Gallery
            </span>
          </router-link>
        </div>

        <!-- Desktop Navigation Links -->
        <div class="hidden md:flex items-center space-x-1">
          <template v-for="item in visibleNavigationItems" :key="item.name">
            <!-- Simple link without dropdown -->
            <router-link
              v-if="!item.children"
              :to="item.path"
              :class="[
                'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
                isActiveRoute(item.path)
                  ? 'bg-primary-50 text-primary-700'
                  : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
              ]"
            >
              {{ item.name }}
            </router-link>

            <!-- Dropdown menu -->
            <div
              v-else
              class="relative"
              @mouseenter="openDropdown(item.name)"
              @mouseleave="closeDropdown"
            >
              <button
                :class="[
                  'px-4 py-2 rounded-lg text-sm font-medium transition-colors flex items-center space-x-1',
                  isActiveParentRoute(item.children)
                    ? 'bg-primary-50 text-primary-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                ]"
              >
                <span>{{ item.name }}</span>
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              <transition name="dropdown">
                <div
                  v-if="activeDropdown === item.name"
                  class="absolute left-0 mt-1 w-48 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 py-1"
                >
                  <router-link
                    v-for="child in item.children"
                    :key="child.name"
                    :to="child.path"
                    class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    @click="closeDropdown"
                  >
                    {{ child.name }}
                  </router-link>
                </div>
              </transition>
            </div>
          </template>
        </div>

        <!-- Right side actions -->
        <div class="hidden md:flex items-center space-x-4">
          <!-- OLTP data-source selector (OLTP / AM / EU / GLOBAL) -->
          <data-source-selector />

          <!-- Search button -->
          <button
            @click="toggleSearch"
            class="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            title="Search"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
          </button>

          <!-- Quick actions dropdown -->
          <div class="relative" ref="quickActionsRef">
            <button
              @click.stop="toggleQuickActions"
              class="btn-primary text-sm flex items-center space-x-2"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span>Add New</span>
            </button>

            <transition name="dropdown">
              <div
                v-if="showQuickActions"
                class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 py-1"
                @click.stop
              >
                <router-link
                  v-for="action in visibleQuickActions"
                  :key="action.path"
                  :to="action.path"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  @click="showQuickActions = false"
                >
                  {{ action.name }}
                </router-link>
              </div>
            </transition>
          </div>
        </div>

        <!-- Mobile menu button -->
        <button
          @click="toggleMobileMenu"
          class="md:hidden p-2 rounded-lg text-gray-500 hover:text-gray-700 hover:bg-gray-100"
          :aria-expanded="isMobileMenuOpen"
          aria-label="Toggle menu"
        >
          <svg v-if="!isMobileMenuOpen" class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>

          <svg v-else class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Search bar (expandable) -->
      <transition name="slide-down">
        <div v-if="showSearch" class="py-4 border-t border-gray-200">
          <div class="relative">
            <input
              ref="searchInput"
              v-model="searchQuery"
              type="text"
              placeholder="Search artworks, exhibitions, artists..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              @keyup.enter="performSearch"
              @keyup.escape="closeSearch"
            />

            <svg class="absolute left-3 top-2.5 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>

            <button
              @click="closeSearch"
              class="absolute right-3 top-2.5 text-gray-400 hover:text-gray-600"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      </transition>
    </nav>

    <!-- Mobile Navigation Menu -->
    <transition name="slide-down">
      <div v-if="isMobileMenuOpen" class="md:hidden bg-white border-t border-gray-200">
        <div class="px-4 py-3 space-y-1">
          <template v-for="item in visibleNavigationItems" :key="item.name">
            <!-- Simple link -->
            <router-link
              v-if="!item.children"
              :to="item.path"
              :class="[
                'block px-4 py-3 rounded-lg text-base font-medium',
                isActiveRoute(item.path)
                  ? 'bg-primary-50 text-primary-700'
                  : 'text-gray-600 hover:bg-gray-100'
              ]"
              @click="closeMobileMenu"
            >
              {{ item.name }}
            </router-link>

            <!-- Expandable section -->
            <div v-else>
              <button
                @click="toggleMobileSection(item.name)"
                :class="[
                  'w-full flex items-center justify-between px-4 py-3 rounded-lg text-base font-medium',
                  isActiveParentRoute(item.children)
                    ? 'bg-primary-50 text-primary-700'
                    : 'text-gray-600 hover:bg-gray-100'
                ]"
              >
                <span>{{ item.name }}</span>

                <svg
                  :class="['w-5 h-5 transition-transform', { 'rotate-180': expandedMobileSection === item.name }]"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              <transition name="expand">
                <div v-if="expandedMobileSection === item.name" class="pl-4 mt-1 space-y-1">
                  <router-link
                    v-for="child in item.children"
                    :key="child.name"
                    :to="child.path"
                    class="block px-4 py-2 rounded-lg text-sm text-gray-600 hover:bg-gray-100"
                    @click="closeMobileMenu"
                  >
                    {{ child.name }}
                  </router-link>
                </div>
              </transition>
            </div>
          </template>

          <!-- Mobile quick actions -->
          <div class="pt-4 mt-4 border-t border-gray-200 space-y-2">
            <router-link
              v-for="action in visibleMobileQuickActions"
              :key="action.path"
              :to="action.path"
              class="block w-full text-center"
              :class="action.class"
              @click="closeMobileMenu"
            >
              {{ action.name }}
            </router-link>
          </div>
        </div>
      </div>
    </transition>
  </header>
</template>

<script>
import DataSourceSelector from './DataSourceSelector.vue';

export default {
  name: 'NavigationMenu',

  components: {
    DataSourceSelector
  },

  props: {
    isMobileMenuOpen: {
      type: Boolean,
      default: false
    }
  },

  emits: ['toggle-mobile-menu'],

  data() {
    return {
      navigationItems: [
        { name: 'Dashboard', path: '/' },
        {
          name: 'Artworks',
          children: [
            { name: 'All Artworks', path: '/artworks' },
            { name: 'Add Artwork', path: '/artworks/new' }
          ]
        },
        {
          name: 'Exhibitions',
          children: [
            { name: 'All Exhibitions', path: '/exhibitions' },
            { name: 'Add Exhibition', path: '/exhibitions/new' }
          ]
        },
        {
          name: 'Management',
          children: [
            { name: 'Reviews', path: '/reviews' },
            { name: 'Visitors', path: '/visitors' },
            { name: 'Staff', path: '/staff' },
            { name: 'Loans', path: '/loans' },
            { name: 'Insurance', path: '/insurance' }
          ]
        },
        { name: 'ETL Management', path: '/etl', requiresOltp: true },
        { name: 'Reports', path: '/reports' },
        { name: 'Analytics', path: '/analytics' },
        {
          name: 'BDD',
          children: [
            { name: 'Local Management (AM/EU)', path: '/bdd/local' },
            { name: 'Global Unified View', path: '/bdd/global' },
            { name: 'BDD Demo (C3 & C4)', path: '/bdd/demo' }
          ]
        }
      ],

      quickActions: [
        { name: 'New Artwork', path: '/artworks/new' },
        { name: 'New Exhibition', path: '/exhibitions/new' },
        { name: 'New Review', path: '/reviews/new' },
        { name: 'New Loan', path: '/loans/new' }
      ],

      mobileQuickActions: [
        { name: 'Add New Artwork', path: '/artworks/new', class: 'btn-primary' },
        { name: 'Add New Exhibition', path: '/exhibitions/new', class: 'btn-outline' }
      ],

      activeDropdown: null,
      showQuickActions: false,
      showSearch: false,
      searchQuery: '',
      expandedMobileSection: null,
      dropdownTimeout: null
    };
  },

  computed: {
    currentPath() {
      return this.$route?.path || '/';
    },

    currentSchema() {
      return this.$store?.state?.dataSource?.source || 'OLTP';
    },

    isOltpSchema() {
      return this.currentSchema === 'OLTP';
    },

    visibleNavigationItems() {
      return this.navigationItems.filter(item => {
        if (item.requiresOltp) {
          return this.isOltpSchema;
        }

        return true;
      });
    },

    visibleQuickActions() {
      return this.quickActions;
    },

    visibleMobileQuickActions() {
      return this.mobileQuickActions;
    }
  },

  watch: {
    '$route'() {
      this.closeDropdown();
      this.showQuickActions = false;
    },

    currentSchema() {
      if (!this.isOltpSchema && this.currentPath.startsWith('/etl')) {
        this.$router.push('/');
      }
    }
  },

  methods: {
    isActiveRoute(path) {
      if (path === '/') {
        return this.currentPath === '/';
      }

      return this.currentPath.startsWith(path);
    },

    isActiveParentRoute(children) {
      return children.some(child => this.isActiveRoute(child.path));
    },

    toggleMobileMenu() {
      this.$emit('toggle-mobile-menu');
    },

    closeMobileMenu() {
      if (this.isMobileMenuOpen) {
        this.$emit('toggle-mobile-menu');
      }

      this.expandedMobileSection = null;
    },

    openDropdown(name) {
      if (this.dropdownTimeout) {
        clearTimeout(this.dropdownTimeout);
      }

      this.activeDropdown = name;
    },

    closeDropdown() {
      this.dropdownTimeout = setTimeout(() => {
        this.activeDropdown = null;
      }, 150);
    },

    toggleQuickActions() {
      this.showQuickActions = !this.showQuickActions;
    },

    toggleSearch() {
      this.showSearch = !this.showSearch;

      if (this.showSearch) {
        this.$nextTick(() => {
          this.$refs.searchInput?.focus();
        });
      }
    },

    closeSearch() {
      this.showSearch = false;
      this.searchQuery = '';
    },

    performSearch() {
      if (this.searchQuery.trim()) {
        this.$router.push({
          path: '/artworks',
          query: { search: this.searchQuery }
        });

        this.closeSearch();
      }
    },

    toggleMobileSection(name) {
      this.expandedMobileSection = this.expandedMobileSection === name ? null : name;
    },

    handleClickOutside(event) {
      if (!this.$el.contains(event.target)) {
        this.showQuickActions = false;
        this.activeDropdown = null;
      }
    }
  },

  mounted() {
    document.addEventListener('click', this.handleClickOutside);
  },

  beforeUnmount() {
    document.removeEventListener('click', this.handleClickOutside);

    if (this.dropdownTimeout) {
      clearTimeout(this.dropdownTimeout);
    }
  }
};
</script>

<style scoped>
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.2s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  max-height: 0;
  overflow: hidden;
}

.slide-down-enter-to,
.slide-down-leave-from {
  max-height: 500px;
}

.expand-enter-active,
.expand-leave-active {
  transition: all 0.2s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
}

.expand-enter-to,
.expand-leave-from {
  max-height: 200px;
}
</style>