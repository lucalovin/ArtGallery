<template>
  <!--
    StaffForm.vue - Staff Member Create/Edit Form Component
    Art Gallery Management System
  -->
  <div class="staff-form bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Form Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
      <h2 class="text-xl font-semibold text-gray-900">
        {{ isEditMode ? 'Edit Staff Member' : 'Add New Staff Member' }}
      </h2>
      <p class="text-sm text-gray-500 mt-1">
        {{ isEditMode ? 'Update staff member information' : 'Fill in the details to add a new staff member' }}
      </p>
    </div>

    <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
      <!-- Personal Information Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Personal Information
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- First Name -->
          <div>
            <label for="firstName" class="block text-sm font-medium text-gray-700 mb-1">
              First Name <span class="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="firstName"
              v-model.trim="form.firstName"
              v-focus
              class="form-input"
              :class="{ 'border-red-500': errors.firstName }"
              placeholder="Enter first name"
            />
            <p v-if="errors.firstName" class="text-red-500 text-xs mt-1">{{ errors.firstName }}</p>
          </div>

          <!-- Last Name -->
          <div>
            <label for="lastName" class="block text-sm font-medium text-gray-700 mb-1">
              Last Name <span class="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="lastName"
              v-model.trim="form.lastName"
              class="form-input"
              :class="{ 'border-red-500': errors.lastName }"
              placeholder="Enter last name"
            />
            <p v-if="errors.lastName" class="text-red-500 text-xs mt-1">{{ errors.lastName }}</p>
          </div>

          <!-- Email -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
              Email <span class="text-red-500">*</span>
            </label>
            <input
              type="email"
              id="email"
              v-model.trim="form.email"
              class="form-input"
              :class="{ 'border-red-500': errors.email }"
              placeholder="email@gallery.com"
            />
            <p v-if="errors.email" class="text-red-500 text-xs mt-1">{{ errors.email }}</p>
          </div>

          <!-- Phone -->
          <div>
            <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">
              Phone
            </label>
            <input
              type="tel"
              id="phone"
              v-model.trim="form.phone"
              class="form-input"
              placeholder="+1 (555) 000-0000"
            />
          </div>

          <!-- Date of Birth -->
          <div>
            <label for="dateOfBirth" class="block text-sm font-medium text-gray-700 mb-1">
              Date of Birth
            </label>
            <input
              type="date"
              id="dateOfBirth"
              v-model="form.dateOfBirth"
              class="form-input"
              :max="maxBirthDate"
            />
          </div>

          <!-- Profile Image URL -->
          <div>
            <label for="imageUrl" class="block text-sm font-medium text-gray-700 mb-1">
              Profile Image URL
            </label>
            <input
              type="url"
              id="imageUrl"
              v-model.trim="form.imageUrl"
              class="form-input"
              placeholder="https://example.com/photo.jpg"
            />
          </div>
        </div>
      </div>

      <!-- Employment Information Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Employment Information
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Position -->
          <div>
            <label for="position" class="block text-sm font-medium text-gray-700 mb-1">
              Position <span class="text-red-500">*</span>
            </label>
            <select
              id="position"
              v-model="form.position"
              class="form-input"
              :class="{ 'border-red-500': errors.position }"
            >
              <option value="">Select a position</option>
              <option v-for="position in positions" :key="position" :value="position">
                {{ position }}
              </option>
            </select>
            <p v-if="errors.position" class="text-red-500 text-xs mt-1">{{ errors.position }}</p>
          </div>

          <!-- Department -->
          <div>
            <label for="department" class="block text-sm font-medium text-gray-700 mb-1">
              Department <span class="text-red-500">*</span>
            </label>
            <select
              id="department"
              v-model="form.department"
              class="form-input"
              :class="{ 'border-red-500': errors.department }"
            >
              <option value="">Select a department</option>
              <option v-for="department in departments" :key="department" :value="department">
                {{ department }}
              </option>
            </select>
            <p v-if="errors.department" class="text-red-500 text-xs mt-1">{{ errors.department }}</p>
          </div>

          <!-- Hire Date -->
          <div>
            <label for="hireDate" class="block text-sm font-medium text-gray-700 mb-1">
              Hire Date <span class="text-red-500">*</span>
            </label>
            <input
              type="date"
              id="hireDate"
              v-model="form.hireDate"
              class="form-input"
              :class="{ 'border-red-500': errors.hireDate }"
            />
            <p v-if="errors.hireDate" class="text-red-500 text-xs mt-1">{{ errors.hireDate }}</p>
          </div>

          <!-- Status -->
          <div>
            <label for="status" class="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select id="status" v-model="form.status" class="form-input">
              <option value="Active">Active</option>
              <option value="On Leave">On Leave</option>
              <option value="Terminated">Terminated</option>
              <option value="Retired">Retired</option>
            </select>
          </div>

          <!-- Salary -->
          <div>
            <label for="salary" class="block text-sm font-medium text-gray-700 mb-1">
              Annual Salary
            </label>
            <div class="relative">
              <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
              <input
                type="number"
                id="salary"
                v-model.number="form.salary"
                class="form-input pl-8"
                placeholder="50000"
                min="0"
                step="1000"
              />
            </div>
          </div>

          <!-- Supervisor -->
          <div>
            <label for="supervisorId" class="block text-sm font-medium text-gray-700 mb-1">
              Supervisor
            </label>
            <select id="supervisorId" v-model="form.supervisorId" class="form-input">
              <option value="">No Supervisor</option>
              <option v-for="supervisor in availableSupervisors" :key="supervisor.id" :value="supervisor.id">
                {{ supervisor.firstName }} {{ supervisor.lastName }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- Skills Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Skills & Qualifications
        </h3>

        <!-- Current Skills -->
        <div v-if="form.skills && form.skills.length > 0" class="flex flex-wrap gap-2 mb-3">
          <span 
            v-for="(skill, index) in form.skills" 
            :key="index"
            class="inline-flex items-center px-3 py-1 rounded-full text-sm bg-primary-100 text-primary-700"
          >
            {{ skill }}
            <button 
              type="button" 
              @click="removeSkill(index)"
              class="ml-2 text-primary-500 hover:text-primary-700"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </span>
        </div>

        <!-- Add New Skill -->
        <div class="flex gap-2">
          <input
            type="text"
            v-model.trim="newSkill"
            class="form-input flex-1"
            placeholder="Add a skill (e.g., Art Restoration, Event Planning)"
            @keyup.enter.prevent="addSkill"
          />
          <button 
            type="button" 
            @click="addSkill"
            :disabled="!newSkill"
            class="btn btn-secondary"
          >
            Add
          </button>
        </div>

        <!-- Suggested Skills -->
        <div class="flex flex-wrap gap-2">
          <span class="text-sm text-gray-500 mr-2">Suggestions:</span>
          <button
            v-for="skill in suggestedSkills"
            :key="skill"
            type="button"
            @click="addSuggestedSkill(skill)"
            class="px-2 py-1 text-xs bg-gray-100 text-gray-600 rounded hover:bg-gray-200"
          >
            + {{ skill }}
          </button>
        </div>
      </div>

      <!-- Notes Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">
          Additional Notes
        </h3>

        <div>
          <textarea
            id="notes"
            v-model.trim="form.notes"
            class="form-input"
            rows="3"
            placeholder="Enter any additional notes about this staff member..."
          ></textarea>
          <p class="text-xs text-gray-400 mt-1">{{ notesCharacterCount }}</p>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex items-center justify-end space-x-3 pt-4 border-t border-gray-200">
        <button
          type="button"
          @click="handleCancel"
          class="btn btn-secondary"
        >
          Cancel
        </button>
        <button
          type="button"
          @click="resetForm"
          class="btn btn-secondary"
        >
          Reset
        </button>
        <button
          type="submit"
          :disabled="!isFormValid || isSubmitting"
          class="btn btn-primary"
          :class="{ 'opacity-50 cursor-not-allowed': !isFormValid || isSubmitting }"
        >
          <span v-if="isSubmitting" class="flex items-center">
            <svg class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Saving...
          </span>
          <span v-else>
            {{ isEditMode ? 'Update Staff Member' : 'Add Staff Member' }}
          </span>
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex';

/**
 * StaffForm Component
 * Create and Edit staff members with full validation
 */
export default {
  name: 'StaffForm',

  props: {
    staff: {
      type: Object,
      default: null
    },
    supervisorsList: {
      type: Array,
      default: () => []
    }
  },

  emits: ['submit', 'cancel'],

  data() {
    return {
      form: {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        imageUrl: '',
        position: '',
        department: '',
        hireDate: '',
        status: 'Active',
        salary: null,
        supervisorId: '',
        skills: [],
        notes: ''
      },
      errors: {},
      isSubmitting: false,
      newSkill: '',
      positions: [
        'Gallery Director',
        'Curator',
        'Assistant Curator',
        'Collections Manager',
        'Registrar',
        'Conservator',
        'Exhibition Designer',
        'Education Coordinator',
        'Marketing Manager',
        'Development Officer',
        'Visitor Services Manager',
        'Security Officer',
        'Tour Guide',
        'Administrative Assistant',
        'IT Specialist',
        'Facilities Manager'
      ],
      departments: [
        'Administration',
        'Curatorial',
        'Collections',
        'Conservation',
        'Education',
        'Marketing & Communications',
        'Development',
        'Visitor Services',
        'Security',
        'Facilities',
        'IT'
      ],
      allSuggestedSkills: [
        'Art History',
        'Art Restoration',
        'Conservation',
        'Archival Research',
        'Event Planning',
        'Public Speaking',
        'Grant Writing',
        'Budget Management',
        'Team Leadership',
        'Customer Service',
        'Photography',
        'Digital Asset Management',
        'Exhibition Design',
        'Multilingual'
      ]
    };
  },

  computed: {
    isEditMode() {
      return this.staff !== null && this.staff.id !== undefined;
    },

    maxBirthDate() {
      const date = new Date();
      date.setFullYear(date.getFullYear() - 18);
      return date.toISOString().split('T')[0];
    },

    availableSupervisors() {
      // Filter out current staff member from supervisors list
      if (this.isEditMode) {
        return this.supervisorsList.filter(s => s.id !== this.staff.id);
      }
      return this.supervisorsList;
    },

    suggestedSkills() {
      // Filter out skills already added
      return this.allSuggestedSkills.filter(skill => 
        !this.form.skills.includes(skill)
      ).slice(0, 5);
    },

    notesCharacterCount() {
      const count = (this.form.notes || '').length;
      return `${count} / 500 characters`;
    },

    isFormValid() {
      return (
        this.form.firstName &&
        this.form.firstName.length >= 2 &&
        this.form.lastName &&
        this.form.lastName.length >= 2 &&
        this.form.email &&
        this.isValidEmail(this.form.email) &&
        this.form.position &&
        this.form.department &&
        this.form.hireDate &&
        Object.keys(this.errors).length === 0
      );
    }
  },

  watch: {
    // Deep watch on staff prop for edit mode
    staff: {
      handler(newStaff) {
        if (newStaff) {
          this.populateForm(newStaff);
        } else {
          this.resetForm();
        }
      },
      immediate: true,
      deep: true
    },

    // Real-time validation watchers
    'form.firstName'(value) {
      if (!value) {
        this.errors.firstName = 'First name is required';
      } else if (value.length < 2) {
        this.errors.firstName = 'First name must be at least 2 characters';
      } else {
        delete this.errors.firstName;
      }
    },

    'form.lastName'(value) {
      if (!value) {
        this.errors.lastName = 'Last name is required';
      } else if (value.length < 2) {
        this.errors.lastName = 'Last name must be at least 2 characters';
      } else {
        delete this.errors.lastName;
      }
    },

    'form.email'(value) {
      if (!value) {
        this.errors.email = 'Email is required';
      } else if (!this.isValidEmail(value)) {
        this.errors.email = 'Please enter a valid email address';
      } else {
        delete this.errors.email;
      }
    },

    'form.position'(value) {
      if (!value) {
        this.errors.position = 'Position is required';
      } else {
        delete this.errors.position;
      }
    },

    'form.department'(value) {
      if (!value) {
        this.errors.department = 'Department is required';
      } else {
        delete this.errors.department;
      }
    },

    'form.hireDate'(value) {
      if (!value) {
        this.errors.hireDate = 'Hire date is required';
      } else {
        delete this.errors.hireDate;
      }
    }
  },

  methods: {
    ...mapActions('staff', ['createStaff', 'updateStaff']),

    populateForm(staff) {
      this.form = {
        firstName: staff.firstName || '',
        lastName: staff.lastName || '',
        email: staff.email || '',
        phone: staff.phone || '',
        dateOfBirth: staff.dateOfBirth || '',
        imageUrl: staff.imageUrl || '',
        position: staff.position || '',
        department: staff.department || '',
        hireDate: staff.hireDate || '',
        status: staff.status || 'Active',
        salary: staff.salary || null,
        supervisorId: staff.supervisorId || '',
        skills: staff.skills ? [...staff.skills] : [],
        notes: staff.notes || ''
      };
      // Clear errors when populating
      this.errors = {};
    },

    resetForm() {
      this.form = {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        imageUrl: '',
        position: '',
        department: '',
        hireDate: '',
        status: 'Active',
        salary: null,
        supervisorId: '',
        skills: [],
        notes: ''
      };
      this.errors = {};
      this.newSkill = '';

      // Re-populate if in edit mode
      if (this.staff) {
        this.populateForm(this.staff);
      }
    },

    isValidEmail(email) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(email);
    },

    addSkill() {
      if (this.newSkill && !this.form.skills.includes(this.newSkill)) {
        this.form.skills.push(this.newSkill);
        this.newSkill = '';
      }
    },

    addSuggestedSkill(skill) {
      if (!this.form.skills.includes(skill)) {
        this.form.skills.push(skill);
      }
    },

    removeSkill(index) {
      this.form.skills.splice(index, 1);
    },

    validateForm() {
      this.errors = {};

      if (!this.form.firstName) {
        this.errors.firstName = 'First name is required';
      } else if (this.form.firstName.length < 2) {
        this.errors.firstName = 'First name must be at least 2 characters';
      }

      if (!this.form.lastName) {
        this.errors.lastName = 'Last name is required';
      } else if (this.form.lastName.length < 2) {
        this.errors.lastName = 'Last name must be at least 2 characters';
      }

      if (!this.form.email) {
        this.errors.email = 'Email is required';
      } else if (!this.isValidEmail(this.form.email)) {
        this.errors.email = 'Please enter a valid email address';
      }

      if (!this.form.position) {
        this.errors.position = 'Position is required';
      }

      if (!this.form.department) {
        this.errors.department = 'Department is required';
      }

      if (!this.form.hireDate) {
        this.errors.hireDate = 'Hire date is required';
      }

      return Object.keys(this.errors).length === 0;
    },

    async handleSubmit() {
      if (!this.validateForm()) {
        return;
      }

      this.isSubmitting = true;

      try {
        const staffData = { ...this.form };

        if (this.isEditMode) {
          staffData.id = this.staff.id;
          await this.updateStaff(staffData);
        } else {
          await this.createStaff(staffData);
        }

        this.$emit('submit', staffData);
      } catch (error) {
        console.error('Error saving staff member:', error);
        alert('Failed to save staff member. Please try again.');
      } finally {
        this.isSubmitting = false;
      }
    },

    handleCancel() {
      this.$emit('cancel');
    }
  }
};
</script>
