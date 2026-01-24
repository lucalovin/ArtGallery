<template>
  <!--
    ItemComponent.vue - Reusable Item Display Card Component
    Art Gallery Management System
    
    OPTIONS API Pattern following Vue.js Lab structure:
    - Generic card display for list items
    - Uses props for data and configuration
    - Uses slots for content customization
    - Uses v-if/v-else for conditional sections
    - Demonstrates computed for display logic
    - Uses $emit for action events
  -->
  <div 
    :class="containerClasses"
    @click="handleClick"
    @mouseenter="isHovered = true"
    @mouseleave="isHovered = false"
  >
    <!-- Image/Media Section -->
    <div v-if="showImage" class="item-image relative overflow-hidden" :class="imageContainerClasses">
      <!-- Image with lazy loading -->
      <img
        v-if="image"
        :src="image"
        :alt="title"
        class="w-full h-full object-cover transition-transform duration-300"
        :class="{ 'scale-110': isHovered && hoverEffect }"
        loading="lazy"
        @error="handleImageError"
      />
      
      <!-- Placeholder when no image -->
      <div v-else class="w-full h-full bg-gray-100 flex items-center justify-center">
        <slot name="image-placeholder">
          <svg class="w-12 h-12 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </slot>
      </div>

      <!-- Image Overlay -->
      <div 
        v-if="$slots['image-overlay'] || (showActions && isHovered)"
        class="absolute inset-0 bg-black bg-opacity-0 hover:bg-opacity-30 transition-all duration-300 flex items-center justify-center"
      >
        <slot name="image-overlay">
          <!-- Quick action buttons on hover -->
          <div 
            v-if="showActions && isHovered"
            class="flex items-center space-x-2 opacity-0 hover:opacity-100 transition-opacity"
            :class="{ 'opacity-100': isHovered }"
          >
            <button
              v-if="actions.includes('view')"
              type="button"
              @click.stop="handleAction('view')"
              class="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
              title="View Details"
            >
              <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            </button>
            <button
              v-if="actions.includes('edit')"
              type="button"
              @click.stop="handleAction('edit')"
              class="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
              title="Edit"
            >
              <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
              </svg>
            </button>
            <button
              v-if="actions.includes('delete')"
              type="button"
              @click.stop="handleAction('delete')"
              class="p-2 bg-white rounded-full shadow-lg hover:bg-red-100 transition-colors"
              title="Delete"
            >
              <svg class="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>
        </slot>
      </div>

      <!-- Status Badge on Image -->
      <div v-if="status" class="absolute top-3 left-3">
        <span :class="statusBadgeClasses">
          {{ status }}
        </span>
      </div>

      <!-- Featured Badge -->
      <div v-if="featured" class="absolute top-3 right-3">
        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
          <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/>
          </svg>
          Featured
        </span>
      </div>
    </div>

    <!-- Content Section -->
    <div class="item-content" :class="contentClasses">
      <!-- Category/Type Tag -->
      <div v-if="category" class="mb-2">
        <span class="text-xs font-medium text-primary-600 uppercase tracking-wider">
          {{ category }}
        </span>
      </div>

      <!-- Title -->
      <h3 v-if="title" :class="titleClasses">
        <router-link 
          v-if="link"
          :to="link"
          class="hover:text-primary-600 transition-colors"
        >
          {{ title }}
        </router-link>
        <span v-else>{{ title }}</span>
      </h3>

      <!-- Subtitle -->
      <p v-if="subtitle" class="text-gray-600 text-sm mt-1">
        {{ subtitle }}
      </p>

      <!-- Description with truncation -->
      <p 
        v-if="description" 
        class="text-gray-600 mt-2"
        :class="descriptionClasses"
      >
        {{ truncatedDescription }}
      </p>

      <!-- Custom Content Slot -->
      <slot name="content"></slot>

      <!-- Metadata Row -->
      <div v-if="$slots.metadata || metadata.length > 0" class="mt-3 flex flex-wrap items-center gap-3 text-sm text-gray-500">
        <slot name="metadata">
          <span 
            v-for="(meta, index) in metadata" 
            :key="index"
            class="flex items-center space-x-1"
          >
            <span v-if="meta.icon" v-html="meta.icon"></span>
            <span>{{ meta.label }}: {{ meta.value }}</span>
          </span>
        </slot>
      </div>

      <!-- Tags -->
      <div v-if="tags.length > 0" class="mt-3 flex flex-wrap gap-2">
        <span 
          v-for="tag in tags" 
          :key="tag"
          class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-700"
        >
          {{ tag }}
        </span>
      </div>

      <!-- Price or Value Display -->
      <div v-if="price !== null" class="mt-3">
        <span class="text-xl font-bold text-gray-900" v-currency="price"></span>
        <span v-if="priceLabel" class="text-sm text-gray-500 ml-1">{{ priceLabel }}</span>
      </div>

      <!-- Footer Slot -->
      <slot name="footer"></slot>
    </div>

    <!-- Actions Footer (for list view) -->
    <div 
      v-if="showActionsFooter && actions.length > 0" 
      class="item-actions border-t border-gray-100 px-4 py-3 flex items-center justify-end space-x-2"
    >
      <button
        v-if="actions.includes('view')"
        type="button"
        @click.stop="handleAction('view')"
        class="btn btn-secondary btn-sm"
      >
        View
      </button>
      <button
        v-if="actions.includes('edit')"
        type="button"
        @click.stop="handleAction('edit')"
        class="btn btn-secondary btn-sm"
      >
        Edit
      </button>
      <button
        v-if="actions.includes('delete')"
        type="button"
        @click.stop="handleAction('delete')"
        class="btn btn-danger btn-sm"
      >
        Delete
      </button>
    </div>
  </div>
</template>

<script>
/**
 * ItemComponent - Reusable Item Card Display
 */
export default {
  // Component name
  name: 'ItemComponent',

  /**
   * props - Input configuration
   */
  props: {
    // Item identification
    itemId: {
      type: [String, Number],
      default: null
    },

    // Link destination
    link: {
      type: [String, Object],
      default: null
    },

    // Image configuration
    image: {
      type: String,
      default: null
    },
    showImage: {
      type: Boolean,
      default: true
    },
    imageHeight: {
      type: String,
      default: 'h-48',
      validator: value => ['h-32', 'h-40', 'h-48', 'h-56', 'h-64'].includes(value)
    },

    // Content
    title: {
      type: String,
      default: ''
    },
    subtitle: {
      type: String,
      default: ''
    },
    description: {
      type: String,
      default: ''
    },
    descriptionLines: {
      type: Number,
      default: 2
    },

    // Category/Type
    category: {
      type: String,
      default: ''
    },

    // Status badge
    status: {
      type: String,
      default: ''
    },
    statusType: {
      type: String,
      default: 'default',
      validator: value => ['default', 'success', 'warning', 'danger', 'info'].includes(value)
    },

    // Featured flag
    featured: {
      type: Boolean,
      default: false
    },

    // Metadata items
    metadata: {
      type: Array,
      default: () => []
    },

    // Tags
    tags: {
      type: Array,
      default: () => []
    },

    // Price/Value display
    price: {
      type: Number,
      default: null
    },
    priceLabel: {
      type: String,
      default: ''
    },

    // Actions configuration
    actions: {
      type: Array,
      default: () => ['view', 'edit', 'delete']
    },
    showActions: {
      type: Boolean,
      default: true
    },
    showActionsFooter: {
      type: Boolean,
      default: false
    },

    // Layout options
    layout: {
      type: String,
      default: 'vertical',
      validator: value => ['vertical', 'horizontal'].includes(value)
    },

    // Styling
    hoverEffect: {
      type: Boolean,
      default: true
    },
    clickable: {
      type: Boolean,
      default: true
    },
    compact: {
      type: Boolean,
      default: false
    }
  },

  /**
   * emits - Action events
   */
  emits: ['click', 'action', 'view', 'edit', 'delete'],

  /**
   * data() - Local reactive state
   */
  data() {
    return {
      // Hover state for effects
      isHovered: false,
      
      // Image loading error state
      imageError: false
    };
  },

  /**
   * computed - Derived properties
   */
  computed: {
    /**
     * Container classes based on layout and options
     */
    containerClasses() {
      const classes = [
        'item-card',
        'bg-white',
        'rounded-xl',
        'shadow-sm',
        'border',
        'border-gray-100',
        'overflow-hidden',
        'transition-all',
        'duration-300'
      ];

      if (this.layout === 'horizontal') {
        classes.push('flex');
      }

      if (this.clickable) {
        classes.push('cursor-pointer');
      }

      if (this.hoverEffect) {
        classes.push('hover:shadow-lg', 'hover:border-gray-200');
      }

      return classes.join(' ');
    },

    /**
     * Image container classes
     */
    imageContainerClasses() {
      const classes = [this.imageHeight];

      if (this.layout === 'horizontal') {
        classes.push('w-48', 'flex-shrink-0');
      }

      return classes.join(' ');
    },

    /**
     * Content section classes
     */
    contentClasses() {
      const classes = ['flex-1'];

      if (this.compact) {
        classes.push('p-3');
      } else {
        classes.push('p-4');
      }

      return classes.join(' ');
    },

    /**
     * Title classes based on size
     */
    titleClasses() {
      if (this.compact) {
        return 'text-base font-semibold text-gray-900 line-clamp-1';
      }
      return 'text-lg font-semibold text-gray-900 line-clamp-2';
    },

    /**
     * Description classes with line clamping
     */
    descriptionClasses() {
      return `line-clamp-${this.descriptionLines} text-sm`;
    },

    /**
     * Truncated description text
     */
    truncatedDescription() {
      if (!this.description) return '';
      
      // Approximate character limit based on lines
      const charLimit = this.descriptionLines * 60;
      
      if (this.description.length <= charLimit) {
        return this.description;
      }
      
      return this.description.substring(0, charLimit).trim() + '...';
    },

    /**
     * Status badge classes based on type
     */
    statusBadgeClasses() {
      const base = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';
      
      const typeClasses = {
        default: 'bg-gray-100 text-gray-800',
        success: 'bg-green-100 text-green-800',
        warning: 'bg-yellow-100 text-yellow-800',
        danger: 'bg-red-100 text-red-800',
        info: 'bg-blue-100 text-blue-800'
      };

      return `${base} ${typeClasses[this.statusType] || typeClasses.default}`;
    }
  },

  /**
   * watch - State monitoring
   */
  watch: {
    /**
     * Reset image error when image prop changes
     */
    image() {
      this.imageError = false;
    }
  },

  /**
   * methods - Event handlers
   */
  methods: {
    /**
     * Handle item click
     */
    handleClick() {
      if (!this.clickable) return;
      
      this.$emit('click', {
        id: this.itemId,
        title: this.title
      });

      // If link is provided, navigate
      if (this.link) {
        this.$router.push(this.link);
      }
    },

    /**
     * Handle action button click
     * @param {string} action - Action type (view, edit, delete)
     */
    handleAction(action) {
      // Emit generic action event with details
      this.$emit('action', {
        action,
        id: this.itemId,
        title: this.title
      });

      // Emit specific action event
      this.$emit(action, {
        id: this.itemId,
        title: this.title
      });
    },

    /**
     * Handle image loading error
     */
    handleImageError(event) {
      this.imageError = true;
      console.warn('[ItemComponent] Image failed to load:', this.image);
      
      // Set fallback placeholder
      event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="%239ca3af" stroke-width="1"%3E%3Crect x="3" y="3" width="18" height="18" rx="2"/%3E%3Cpath d="M3 15l4-4a2 2 0 0 1 2.8 0L15 16"/%3E%3Cpath d="M14 14l1.5-1.5a2 2 0 0 1 2.8 0L21 15"/%3E%3Ccircle cx="8.5" cy="8.5" r="1.5"/%3E%3C/svg%3E';
    }
  }
};
</script>

<style scoped>
/* Line clamp utilities */
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Hover animation for action buttons */
.item-card .item-actions button {
  opacity: 0;
  transform: translateY(4px);
  transition: all 0.2s ease;
}

.item-card:hover .item-actions button {
  opacity: 1;
  transform: translateY(0);
}

/* Staggered animation for buttons */
.item-card:hover .item-actions button:nth-child(1) { transition-delay: 0ms; }
.item-card:hover .item-actions button:nth-child(2) { transition-delay: 50ms; }
.item-card:hover .item-actions button:nth-child(3) { transition-delay: 100ms; }
</style>
