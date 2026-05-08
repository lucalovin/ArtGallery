<template>
  <!--
    DataSourceSelector.vue

    Dropdown that lets the user switch which Oracle schema the OLTP-style
    pages run against:
      - OLTP   : ART_GALLERY_OLTP (default)
      - AM     : ARTGALLERY_AM
      - EU     : ARTGALLERY_EU
      - GLOBAL : ARTGALLERY_GLOBAL (unified views)

    The choice is stored via Vuex (dataSource module) and persisted in
    localStorage. Every API request automatically attaches it as the
    X-Data-Source header (see src/api/client.js), so the entire app -
    not just the BDD section - operates on the chosen schema.
  -->
  <div class="relative inline-block text-left" ref="root">
    <button
      type="button"
      @click.stop="open = !open"
      :title="`Active schema: ${schema}`"
      class="inline-flex items-center gap-2 px-3 py-1.5 rounded-md border border-gray-300 bg-white hover:bg-gray-50 text-sm font-medium text-gray-700 transition-colors"
    >
      <span
        class="w-2 h-2 rounded-full"
        :class="dotClass"
        aria-hidden="true"
      ></span>
      <span class="hidden sm:inline">Schema:</span>
      <span :class="['px-2 py-0.5 rounded text-xs font-semibold', badgeClass]">
        {{ current }}
      </span>
      <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <transition name="dropdown">
      <div
        v-if="open"
        class="absolute right-0 mt-2 w-72 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 z-50"
        @click.stop
      >
        <div class="px-4 py-2 border-b border-gray-100">
          <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider">
            OLTP data source
          </p>
          <p class="text-xs text-gray-500 mt-0.5">
            Switches the schema used by all OLTP pages (Artworks, Exhibitions,
            Visitors, Loans, ...). The BDD module is unaffected.
          </p>
        </div>
        <ul class="py-1">
          <li v-for="opt in options" :key="opt.code">
            <button
              type="button"
              @click="select(opt.code)"
              :class="[
                'w-full text-left px-4 py-2 flex items-center gap-3 text-sm hover:bg-gray-50',
                opt.code === current ? 'bg-primary-50' : ''
              ]"
            >
              <span :class="['px-2 py-0.5 rounded text-xs font-semibold', opt.badge]">
                {{ opt.code }}
              </span>
              <span class="flex-1">
                <span class="block text-gray-900 font-medium">{{ opt.label }}</span>
                <span class="block text-xs text-gray-500">{{ opt.schema }}</span>
              </span>
              <svg
                v-if="opt.code === current"
                class="w-4 h-4 text-primary-600"
                fill="none" stroke="currentColor" viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
            </button>
          </li>
        </ul>
        <div class="px-4 py-2 border-t border-gray-100 text-xs text-gray-500">
          <p v-if="current !== 'OLTP'">
            ⚠ Modules unsupported on this schema will return errors / be hidden.
          </p>
          <p v-else>
            Default OLTP schema. All modules are available.
          </p>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
import { mapGetters } from 'vuex';

export default {
  name: 'DataSourceSelector',

  data() {
    return {
      open: false
    };
  },

  computed: {
    ...mapGetters('dataSource', {
      current: 'current',
      label: 'label',
      badgeClass: 'badgeClass',
      schema: 'schema',
      options: 'options'
    }),
    dotClass() {
      switch (this.current) {
        case 'AM':     return 'bg-amber-500';
        case 'EU':     return 'bg-blue-500';
        case 'GLOBAL': return 'bg-emerald-500';
        default:       return 'bg-gray-400';
      }
    }
  },

  mounted() {
    document.addEventListener('click', this.handleOutside);
  },

  beforeUnmount() {
    document.removeEventListener('click', this.handleOutside);
  },

  methods: {
    handleOutside(ev) {
      if (this.$refs.root && !this.$refs.root.contains(ev.target)) {
        this.open = false;
      }
    },
    select(code) {
      if (code !== this.current) {
        this.$store.dispatch('dataSource/setSource', code);
        // Force a clean reload so any cached store data is re-fetched
        // against the newly-selected schema.
        this.$nextTick(() => window.location.reload());
      } else {
        this.open = false;
      }
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
  transform: translateY(-4px);
}
</style>
