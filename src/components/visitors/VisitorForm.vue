<template>
  <!--
    VisitorForm.vue - Visitor Create/Edit Form Component
    Art Gallery Management System
  -->
  <div class="visitor-form">
    <!-- Form Header -->
    <div class="mb-6">
      <h2 class="text-2xl font-display font-bold text-gray-900">
        {{ isEditMode ? 'Edit Visitor' : 'Add New Visitor' }}
      </h2>
      <p class="text-gray-600 mt-1">
        {{ isEditMode ? 'Update visitor information' : 'Enter visitor details' }}
      </p>
    </div>

    <!-- Error Alert -->
    <div v-if="error" class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
      <div class="flex items-start space-x-3">
        <svg class="w-5 h-5 text-red-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-red-800 font-medium flex-1">{{ error }}</p>
        <button type="button" @click="error = ''" class="text-red-500 hover:text-red-700">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Form -->
    <form @submit.prevent="handleSubmit" class="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Personal Info -->
        <div class="lg:col-span-2">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
            Personal Information
          </h3>
        </div>

        <!-- First Name -->
        <div>
          <label for="firstName" class="block text-sm font-medium text-gray-700 mb-1">
            First Name <span class="text-red-500">*</span>
          </label>
          <input
            id="firstName"
            v-model="form.firstName"
            type="text"
            class="form-input w-full"
            :class="{ 'border-red-300': errors.firstName }"
            placeholder="Enter first name"
            required
            v-focus
          />
          <p v-if="errors.firstName" class="mt-1 text-sm text-red-600">{{ errors.firstName }}</p>
        </div>

        <!-- Last Name -->
        <div>
          <label for="lastName" class="block text-sm font-medium text-gray-700 mb-1">
            Last Name <span class="text-red-500">*</span>
          </label>
          <input
            id="lastName"
            v-model="form.lastName"
            type="text"
            class="form-input w-full"
            :class="{ 'border-red-300': errors.lastName }"
            placeholder="Enter last name"
            required
          />
          <p v-if="errors.lastName" class="mt-1 text-sm text-red-600">{{ errors.lastName }}</p>
        </div>

        <!-- Email -->
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
            Email <span class="text-red-500">*</span>
          </label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            class="form-input w-full"
            :class="{ 'border-red-300': errors.email }"
            placeholder="visitor@example.com"
            required
          />
          <p v-if="errors.email" class="mt-1 text-sm text-red-600">{{ errors.email }}</p>
        </div>

        <!-- Phone -->
        <div>
          <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">
            Phone
          </label>
          <input
            id="phone"
            v-model="form.phone"
            type="tel"
            class="form-input w-full"
            placeholder="+1 (555) 123-4567"
          />
        </div>

        <!-- Date of Birth -->
        <div>
          <label for="dateOfBirth" class="block text-sm font-medium text-gray-700 mb-1">
            Date of Birth
          </label>
          <input
            id="dateOfBirth"
            v-model="form.dateOfBirth"
            type="date"
            class="form-input w-full"
            :max="today"
          />
        </div>

        <!-- Gender -->
        <div>
          <label for="gender" class="block text-sm font-medium text-gray-700 mb-1">
            Gender
          </label>
          <select
            id="gender"
            v-model="form.gender"
            class="form-input w-full"
          >
            <option value="">Prefer not to say</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
          </select>
        </div>

        <!-- Address Section -->
        <div class="lg:col-span-2 mt-4">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
            Address (Optional)
          </h3>
        </div>

        <!-- Street Address -->
        <div class="lg:col-span-2">
          <label for="address" class="block text-sm font-medium text-gray-700 mb-1">
            Street Address
          </label>
          <input
            id="address"
            v-model="form.address"
            type="text"
            class="form-input w-full"
            placeholder="123 Main Street"
          />
        </div>

        <!-- City -->
        <div>
          <label for="city" class="block text-sm font-medium text-gray-700 mb-1">
            City
          </label>
          <input
            id="city"
            v-model="form.city"
            type="text"
            class="form-input w-full"
            placeholder="City"
          />
        </div>

        <!-- Country -->
        <div>
          <label for="country" class="block text-sm font-medium text-gray-700 mb-1">
            Country
          </label>
          <input
            id="country"
            v-model="form.country"
            type="text"
            class="form-input w-full"
            placeholder="Country"
          />
        </div>

        <!-- Membership Section -->
        <div class="lg:col-span-2 mt-4">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-100 pb-2 mb-4">
            Membership
          </h3>
        </div>

        <!-- Is Member Checkbox -->
        <div class="lg:col-span-2">
          <div class="flex items-center">
            <input
              id="isMember"
              v-model="form.isMember"
              type="checkbox"
              class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <label for="isMember" class="ml-2 text-sm text-gray-700">
              Gallery Member
            </label>
          </div>
        </div>

        <!-- Membership Type (if member) -->
        <div v-if="form.isMember">
          <label for="membershipType" class="block text-sm font-medium text-gray-700 mb-1">
            Membership Type
          </label>
          <select
            id="membershipType"
            v-model="form.membershipType"
            class="form-input w-full"
          >
            <option value="basic">Basic</option>
            <option value="premium">Premium</option>
            <option value="patron">Patron</option>
            <option value="benefactor">Benefactor</option>
          </select>
        </div>

        <!-- Membership Expiry (if member) -->
        <div v-if="form.isMember">
          <label for="membershipExpiry" class="block text-sm font-medium text-gray-700 mb-1">
            Membership Expiry
          </label>
          <input
            id="membershipExpiry"
            v-model="form.membershipExpiry"
            type="date"
            class="form-input w-full"
            :min="today"
          />
        </div>

        <!-- Notes -->
        <div class="lg:col-span-2 mt-4">
          <label for="notes" class="block text-sm font-medium text-gray-700 mb-1">
            Notes
          </label>
          <textarea
            id="notes"
            v-model="form.notes"
            rows="3"
            class="form-input w-full"
            placeholder="Additional notes about the visitor..."
          ></textarea>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-3 pt-6 mt-6 border-t border-gray-100">
        <button
          type="button"
          @click="handleCancel"
          :disabled="loading"
          class="btn btn-secondary"
        >
          Cancel
        </button>
        <button
          type="submit"
          :disabled="loading || !isFormValid"
          class="btn btn-primary"
          :class="{ 'opacity-50 cursor-not-allowed': !isFormValid }"
        >
          <span v-if="loading">{{ isEditMode ? 'Updating...' : 'Saving...' }}</span>
          <span v-else>{{ isEditMode ? 'Update Visitor' : 'Add Visitor' }}</span>
        </button>
      </div>
    </form>
  </div>
</template>

<script>
/**
 * VisitorForm Component
 */
export default {
  name: 'VisitorForm',

  props: {
    visitor: {
      type: Object,
      default: null
    },
    submitting: {
      type: Boolean,
      default: false
    }
  },

  emits: ['submit', 'cancel', 'dirty'],

  data() {
    return {
      form: {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        gender: '',
        address: '',
        city: '',
        country: '',
        isMember: false,
        membershipType: 'basic',
        membershipExpiry: '',
        notes: ''
      },
      errors: {},
      loading: false,
      error: '',
      isDirty: false
    };
  },

  computed: {
    isEditMode() {
      return this.visitor !== null && this.visitor.id !== undefined;
    },

    today() {
      return new Date().toISOString().split('T')[0];
    },

    isFormValid() {
      return (
        this.form.firstName.trim() !== '' &&
        this.form.lastName.trim() !== '' &&
        this.form.email.trim() !== '' &&
        this.isValidEmail(this.form.email) &&
        Object.keys(this.errors).length === 0
      );
    }
  },

  watch: {
    visitor: {
      immediate: true,
      deep: true,
      handler(newVisitor) {
        if (newVisitor) {
          this.populateForm(newVisitor);
        }
      }
    },

    form: {
      deep: true,
      handler() {
        this.isDirty = true;
        this.$emit('dirty', true);
        this.validateForm();
      }
    },

    'form.email'(newValue) {
      if (newValue && !this.isValidEmail(newValue)) {
        this.errors.email = 'Please enter a valid email address';
      } else {
        delete this.errors.email;
      }
    },

    submitting(newValue) {
      this.loading = newValue;
    }
  },

  created() {
    if (this.visitor) {
      this.populateForm(this.visitor);
    }
  },

  mounted() {
    window.addEventListener('beforeunload', this.handleBeforeUnload);
  },

  beforeUnmount() {
    window.removeEventListener('beforeunload', this.handleBeforeUnload);
  },

  methods: {
    populateForm(visitor) {
      this.form = {
        firstName: visitor.firstName || '',
        lastName: visitor.lastName || '',
        email: visitor.email || '',
        phone: visitor.phone || '',
        dateOfBirth: visitor.dateOfBirth || '',
        gender: visitor.gender || '',
        address: visitor.address || '',
        city: visitor.city || '',
        country: visitor.country || '',
        isMember: visitor.isMember || false,
        membershipType: visitor.membershipType || 'basic',
        membershipExpiry: visitor.membershipExpiry || '',
        notes: visitor.notes || ''
      };
      
      this.$nextTick(() => {
        this.isDirty = false;
      });
    },

    validateForm() {
      this.errors = {};

      if (!this.form.firstName.trim()) {
        this.errors.firstName = 'First name is required';
      }

      if (!this.form.lastName.trim()) {
        this.errors.lastName = 'Last name is required';
      }

      if (!this.form.email.trim()) {
        this.errors.email = 'Email is required';
      } else if (!this.isValidEmail(this.form.email)) {
        this.errors.email = 'Please enter a valid email address';
      }

      return Object.keys(this.errors).length === 0;
    },

    isValidEmail(email) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(email);
    },

    async handleSubmit() {
      if (!this.validateForm()) {
        this.error = 'Please fix the errors below';
        return;
      }

      this.loading = true;
      this.error = '';

      try {
        const visitorData = {
          ...this.form,
          updatedAt: new Date().toISOString()
        };

        if (this.isEditMode) {
          visitorData.id = this.visitor.id;
        } else {
          visitorData.createdAt = new Date().toISOString();
          visitorData.visitCount = 0;
          visitorData.reviewCount = 0;
          visitorData.totalSpent = 0;
        }

        this.$emit('submit', visitorData);
        this.isDirty = false;
      } catch (err) {
        this.error = err.message || 'An error occurred while saving';
      } finally {
        this.loading = false;
      }
    },

    handleCancel() {
      if (this.isDirty) {
        if (!confirm('You have unsaved changes. Are you sure you want to cancel?')) {
          return;
        }
      }
      this.$emit('cancel');
      this.$router.back();
    },

    handleBeforeUnload(event) {
      if (this.isDirty) {
        event.preventDefault();
        event.returnValue = 'You have unsaved changes.';
        return event.returnValue;
      }
    }
  }
};
</script>
