<template>
  <!--
    FormComponent.vue - Reusable Form Wrapper Component
    Art Gallery Management System
  -->
  <div class="form-wrapper">
    <!-- Form Title Section -->
    <div v-if="title || $slots.header" class="form-header mb-6">
      <slot name="header">
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-2xl font-display font-bold text-gray-900">{{ title }}</h2>
            <p v-if="subtitle" class="text-gray-600 mt-1">{{ subtitle }}</p>
          </div>
          
          <!-- Header Actions Slot -->
          <slot name="header-actions"></slot>
        </div>
      </slot>
    </div>

    <!-- Main Form -->
    <form 
      @submit.prevent="handleSubmit"
      :class="formClasses"
      :id="formId"
      novalidate
    >
      <!-- Loading Overlay -->
      <div v-if="loading" class="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10 rounded-lg">
        <div class="flex flex-col items-center space-y-3">
          <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600"></div>
          <span class="text-gray-600 text-sm">{{ loadingText }}</span>
        </div>
      </div>

      <!-- Error Alert -->
      <div 
        v-if="error" 
        class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4"
      >
        <div class="flex items-start space-x-3">
          <svg class="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div class="flex-1">
            <h4 class="text-red-800 font-medium">{{ errorTitle }}</h4>
            <p class="text-red-600 text-sm mt-1">{{ error }}</p>
          </div>
          <button 
            type="button"
            @click="clearError"
            class="text-red-500 hover:text-red-700"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Success Alert -->
      <div 
        v-if="success" 
        class="mb-6 bg-green-50 border border-green-200 rounded-lg p-4"
      >
        <div class="flex items-start space-x-3">
          <svg class="w-5 h-5 text-green-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div class="flex-1">
            <p class="text-green-800 font-medium">{{ success }}</p>
          </div>
          <button 
            type="button"
            @click="clearSuccess"
            class="text-green-500 hover:text-green-700"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Form Content Slot -->
      <div :class="contentClasses">
        <slot></slot>
      </div>

      <!-- Form Actions -->
      <div 
        v-if="showActions" 
        :class="actionsClasses"
      >
        <slot name="actions">
          <!-- Cancel Button -->
          <button
            v-if="showCancel"
            type="button"
            @click="handleCancel"
            :disabled="loading"
            class="btn btn-secondary"
          >
            {{ cancelText }}
          </button>

          <!-- Reset Button -->
          <button
            v-if="showReset"
            type="button"
            @click="handleReset"
            :disabled="loading"
            class="btn btn-secondary"
          >
            {{ resetText }}
          </button>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="loading || !isValid"
            :class="submitButtonClasses"
          >
            <span v-if="loading" class="flex items-center space-x-2">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>{{ loadingText }}</span>
            </span>
            <span v-else>{{ submitText }}</span>
          </button>
        </slot>
      </div>
    </form>

    <!-- Form Footer Slot -->
    <slot name="footer"></slot>
  </div>
</template>

<script>
/**
 * FormComponent - Reusable Form Wrapper
 */
export default {
  // Component name
  name: 'FormComponent',

  /**
   * props - Input properties
   */
  props: {
    // Form identification
    formId: {
      type: String,
      default: () => `form-${Date.now()}`
    },

    // Form title and subtitle
    title: {
      type: String,
      default: ''
    },
    subtitle: {
      type: String,
      default: ''
    },

    // Form state
    loading: {
      type: Boolean,
      default: false
    },
    loadingText: {
      type: String,
      default: 'Processing...'
    },

    // Error handling
    error: {
      type: String,
      default: ''
    },
    errorTitle: {
      type: String,
      default: 'Error'
    },

    // Success message
    success: {
      type: String,
      default: ''
    },

    // Validation state
    isValid: {
      type: Boolean,
      default: true
    },

    // Action buttons config
    showActions: {
      type: Boolean,
      default: true
    },
    showCancel: {
      type: Boolean,
      default: true
    },
    showReset: {
      type: Boolean,
      default: false
    },

    // Button texts
    submitText: {
      type: String,
      default: 'Save'
    },
    cancelText: {
      type: String,
      default: 'Cancel'
    },
    resetText: {
      type: String,
      default: 'Reset'
    },

    // Layout options
    layout: {
      type: String,
      default: 'default',
      validator: value => ['default', 'compact', 'wide'].includes(value)
    },

    // Card style wrapper
    card: {
      type: Boolean,
      default: true
    }
  },

  /**
   * emits - Declared events
   * Explicit event declarations for parent communication
   */
  emits: [
    'submit',
    'cancel',
    'reset',
    'clear-error',
    'clear-success',
    'form-dirty',
    'form-valid'
  ],

  /**
   * data() - Internal reactive state
   */
  data() {
    return {
      // Track if form has been modified
      isDirty: false,
      
      // Internal form state
      initialData: null
    };
  },

  /**
   * computed - Derived properties for CSS classes
   */
  computed: {
    /**
     * Form wrapper classes based on layout and card options
     */
    formClasses() {
      const classes = ['relative'];
      
      if (this.card) {
        classes.push('bg-white rounded-xl shadow-sm border border-gray-100 p-6');
      }
      
      if (this.loading) {
        classes.push('pointer-events-none');
      }
      
      return classes.join(' ');
    },

    /**
     * Content area classes based on layout
     */
    contentClasses() {
      switch (this.layout) {
        case 'compact':
          return 'space-y-4';
        case 'wide':
          return 'space-y-8';
        default:
          return 'space-y-6';
      }
    },

    /**
     * Actions area classes
     */
    actionsClasses() {
      return [
        'flex',
        'items-center',
        'justify-end',
        'space-x-3',
        'pt-6',
        'mt-6',
        'border-t',
        'border-gray-100'
      ].join(' ');
    },

    /**
     * Submit button classes with state variations
     */
    submitButtonClasses() {
      const base = 'btn btn-primary';
      
      if (!this.isValid) {
        return `${base} opacity-50 cursor-not-allowed`;
      }
      
      return base;
    }
  },

  /**
   * watch - Monitor changes
   */
  watch: {
    /**
     * Watch for dirty state changes and notify parent
     */
    isDirty(newValue) {
      this.$emit('form-dirty', newValue);
    },

    /**
     * Watch for validity changes
     */
    isValid(newValue) {
      this.$emit('form-valid', newValue);
    },

    /**
     * Clear success message after delay
     */
    success(newValue) {
      if (newValue) {
        setTimeout(() => {
          this.clearSuccess();
        }, 5000);
      }
    }
  },

  /**
   * created - Lifecycle hook
   * Called after instance is created
   */
  created() {
    // Log component creation for debugging
    console.log('[FormComponent] Created with formId:', this.formId);
  },

  /**
   * mounted - Lifecycle hook
   * Called after component is mounted to DOM
   */
  mounted() {
    // Add beforeunload listener to warn about unsaved changes
    window.addEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * beforeUnmount - Lifecycle hook
   * Cleanup before component is destroyed
   */
  beforeUnmount() {
    // Remove beforeunload listener
    window.removeEventListener('beforeunload', this.handleBeforeUnload);
  },

  /**
   * methods - Component methods
   */
  methods: {
    /**
     * Handle form submission
     * Validates and emits submit event to parent
     */
    handleSubmit() {
      if (this.loading || !this.isValid) {
        return;
      }
      
      // Emit submit event to parent
      this.$emit('submit');
    },

    /**
     * Handle cancel action
     * Confirms if form is dirty, then emits cancel
     */
    handleCancel() {
      if (this.isDirty) {
        const confirmed = window.confirm('You have unsaved changes. Are you sure you want to cancel?');
        if (!confirmed) {
          return;
        }
      }
      
      this.$emit('cancel');
    },

    /**
     * Handle reset action
     * Confirms reset, then emits reset event
     */
    handleReset() {
      const confirmed = window.confirm('Are you sure you want to reset the form?');
      if (confirmed) {
        this.isDirty = false;
        this.$emit('reset');
      }
    },

    /**
     * Clear error message
     */
    clearError() {
      this.$emit('clear-error');
    },

    /**
     * Clear success message
     */
    clearSuccess() {
      this.$emit('clear-success');
    },

    /**
     * Mark form as dirty (modified)
     * Called by child components when form data changes
     */
    markDirty() {
      this.isDirty = true;
    },

    /**
     * Mark form as clean (unmodified)
     * Called after successful save
     */
    markClean() {
      this.isDirty = false;
    },

    /**
     * Handle browser beforeunload event
     * Warn user about unsaved changes
     */
    handleBeforeUnload(event) {
      if (this.isDirty) {
        event.preventDefault();
        event.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
        return event.returnValue;
      }
    }
  }
};
</script>

<style scoped>
/* Form wrapper transitions */
.form-wrapper {
  @apply relative;
}

/* Animation for alerts */
.form-wrapper .bg-red-50,
.form-wrapper .bg-green-50 {
  animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
