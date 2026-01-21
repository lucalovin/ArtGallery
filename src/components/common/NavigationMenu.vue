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
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <span class="text-xl font-display font-bold text-gray-900 hidden sm:block">
              Art Gallery
            </span>
          </router-link>
        </div>

        <!-- Desktop Navigation Links -->
        <div class="hidden md:flex items-center space-x-1">
          <!-- Main navigation items using v-for -->
          <template v-for="item in navigationItems" :key="item.name">
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
            <div v-else class="relative" @mouseenter="openDropdown(item.name)" @mouseleave="closeDropdown">
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

              <!-- Dropdown content -->
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
          <!-- Search button -->
          <button 
            @click="toggleSearch"
            class="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            title="Search"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </button>

          <!-- Quick actions dropdown -->
          <div class="relative">
            <button
              @click="toggleQuickActions"
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
              >
                <router-link 
                  to="/artworks/new" 
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  @click="showQuickActions = false"
                >
                  New Artwork
                </router-link>
                <router-link 
                  to="/exhibitions/new" 
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  @click="showQuickActions = false"
                >
                  New Exhibition
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
          <!-- Hamburger icon / X icon -->
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
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
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
          <!-- Mobile nav items -->
          <template v-for="item in navigationItems" :key="item.name">
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

              <!-- Child links -->
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
              to="/artworks/new" 
              class="block w-full text-center btn-primary"
              @click="closeMobileMenu"
            >
              Add New Artwork
            </router-link>
            <router-link 
              to="/exhibitions/new" 
              class="block w-full text-center btn-outline"
              @click="closeMobileMenu"
            >
              Add New Exhibition
            </router-link>
          </div>
        </div>
      </div>
    </transition>
  </header>
</template>

<script>
/**
 * NavigationMenu Component
 */
export default {
  // Component name for debugging and recursive components
  name: 'NavigationMenu',

  // Props received from parent (App.vue)
  props: {
    /**
     * Whether mobile menu is open (controlled by parent)
     */
    isMobileMenuOpen: {
      type: Boolean,
      default: false
    }
  },

  // Events emitted to parent
  emits: ['toggle-mobile-menu'],

  /**
   * data() - Reactive state
   * Following Lab pattern: returns object with component state
   */
  data() {
    return {
      // Navigation items array - demonstrates v-for iteration
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
        { name: 'ETL Management', path: '/etl' },
        { name: 'Reports', path: '/reports' }
      ],

      // Active dropdown menu (desktop)
      activeDropdown: null,

      // Show quick actions menu
      showQuickActions: false,

      // Show search bar
      showSearch: false,

      // Search query - v-model binding example
      searchQuery: '',

      // Expanded mobile section
      expandedMobileSection: null,

      // Dropdown close timeout
      dropdownTimeout: null
    };
  },

  /**
   * computed - Derived reactive properties
   * Automatically recalculate when dependencies change
   */
  computed: {
    /**
     * Get current route path
     */
    currentPath() {
      return this.$route?.path || '/';
    }
  },

  /**
   * watch - React to data changes
   * Following Lab pattern for observing props/data
   */
  watch: {
    /**
     * Watch route changes to close menus
     */
    '$route'() {
      this.closeDropdown();
      this.showQuickActions = false;
    }
  },

  /**
   * methods - Component functions
   * Event handlers and logic
   */
  methods: {
    /**
     * Check if a route is active
     * @param {string} path - Route path to check
     * @returns {boolean}
     */
    isActiveRoute(path) {
      if (path === '/') {
        return this.currentPath === '/';
      }
      return this.currentPath.startsWith(path);
    },

    /**
     * Check if any child route is active
     * @param {Array} children - Child routes array
     * @returns {boolean}
     */
    isActiveParentRoute(children) {
      return children.some(child => this.isActiveRoute(child.path));
    },

    /**
     * Toggle mobile menu - emits event to parent
     * Demonstrates child-to-parent communication
     */
    toggleMobileMenu() {
      this.$emit('toggle-mobile-menu');
    },

    /**
     * Close mobile menu
     */
    closeMobileMenu() {
      if (this.isMobileMenuOpen) {
        this.$emit('toggle-mobile-menu');
      }
      this.expandedMobileSection = null;
    },

    /**
     * Open dropdown menu (desktop)
     * @param {string} name - Dropdown name
     */
    openDropdown(name) {
      if (this.dropdownTimeout) {
        clearTimeout(this.dropdownTimeout);
      }
      this.activeDropdown = name;
    },

    /**
     * Close dropdown with delay
     */
    closeDropdown() {
      this.dropdownTimeout = setTimeout(() => {
        this.activeDropdown = null;
      }, 150);
    },

    /**
     * Toggle quick actions menu
     */
    toggleQuickActions() {
      this.showQuickActions = !this.showQuickActions;
    },

    /**
     * Toggle search bar
     */
    toggleSearch() {
      this.showSearch = !this.showSearch;
      if (this.showSearch) {
        // Focus input after DOM update
        this.$nextTick(() => {
          this.$refs.searchInput?.focus();
        });
      }
    },

    /**
     * Close search bar
     */
    closeSearch() {
      this.showSearch = false;
      this.searchQuery = '';
    },

    /**
     * Perform search - navigate to results
     */
    performSearch() {
      if (this.searchQuery.trim()) {
        this.$router.push({
          path: '/artworks',
          query: { search: this.searchQuery }
        });
        this.closeSearch();
      }
    },

    /**
     * Toggle mobile section (accordion)
     * @param {string} name - Section name
     */
    toggleMobileSection(name) {
      this.expandedMobileSection = this.expandedMobileSection === name ? null : name;
    },

    /**
     * Handle click outside to close dropdowns
     * @param {Event} event - Click event
     */
    handleClickOutside(event) {
      if (!this.$el.contains(event.target)) {
        this.showQuickActions = false;
        this.activeDropdown = null;
      }
    }
  },

  /**
   * mounted() - DOM ready lifecycle hook
   * Add global event listeners
   */
  mounted() {
    document.addEventListener('click', this.handleClickOutside);
  },

  /**
   * beforeUnmount() - Cleanup lifecycle hook
   * Remove global event listeners
   */
  beforeUnmount() {
    document.removeEventListener('click', this.handleClickOutside);
    if (this.dropdownTimeout) {
      clearTimeout(this.dropdownTimeout);
    }
  }
};
</script>

<style scoped>
/* Dropdown transition */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Slide down transition */
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

/* Expand transition for mobile accordion */
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
