<template>
  <!--
    ETLMapping.vue - ETL Data Mapping Configuration Component
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-mapping bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <!-- Header -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
      <div>
        <h2 class="text-lg font-semibold text-gray-900">Data Mapping Configuration</h2>
        <p class="text-sm text-gray-500 mt-1">Configure field mappings from OLTP to Data Warehouse</p>
      </div>
      <div class="flex items-center space-x-2">
        <button
          @click="validateMappings"
          class="btn btn-secondary"
        >
          Validate
        </button>
        <button
          @click="saveMappings"
          :disabled="!hasChanges"
          class="btn btn-primary"
          :class="{ 'opacity-50 cursor-not-allowed': !hasChanges }"
        >
          Save Mappings
        </button>
      </div>
    </div>

    <!-- Source/Target Selection -->
    <div class="px-6 py-4 border-b border-gray-100 bg-gray-50">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Source Table (OLTP)</label>
          <select v-model="selectedSource" class="form-input" @change="loadSourceFields">
            <option value="">Select source table</option>
            <option v-for="table in sourceTables" :key="table.name" :value="table.name">
              {{ table.name }} ({{ table.recordCount }} records)
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Target Table (DW)</label>
          <select v-model="selectedTarget" class="form-input" @change="loadTargetFields">
            <option value="">Select target table</option>
            <option v-for="table in targetTables" :key="table.name" :value="table.name">
              {{ table.name }} ({{ table.type }})
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Mapping Area -->
    <div class="p-6">
      <div v-if="selectedSource && selectedTarget" class="space-y-6">
        <!-- Auto-Map Button -->
        <div class="flex items-center justify-between">
          <span class="text-sm text-gray-500">{{ mappings.length }} field mappings configured</span>
          <button
            @click="autoMapFields"
            class="btn btn-secondary btn-sm"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            Auto-Map
          </button>
        </div>

        <!-- Mapping Grid -->
        <div class="grid grid-cols-3 gap-4">
          <!-- Source Fields Column -->
          <div>
            <h3 class="text-sm font-medium text-gray-700 mb-3 flex items-center">
              <span class="w-3 h-3 bg-blue-500 rounded-full mr-2"></span>
              Source Fields
            </h3>
            <div class="space-y-2">
              <div
                v-for="field in sourceFields"
                :key="field.name"
                class="p-3 bg-blue-50 rounded-lg border border-blue-100 cursor-move"
                :class="{ 'ring-2 ring-blue-500': selectedSourceField === field.name }"
                @click="selectSourceField(field.name)"
                draggable="true"
                @dragstart="handleDragStart($event, field.name, 'source')"
              >
                <div class="flex items-center justify-between">
                  <span class="text-sm font-medium text-gray-900">{{ field.name }}</span>
                  <span class="text-xs text-gray-500 bg-blue-100 px-2 py-0.5 rounded">{{ field.type }}</span>
                </div>
                <div v-if="field.nullable" class="text-xs text-gray-400 mt-1">Nullable</div>
              </div>
            </div>
          </div>

          <!-- Mapping Connections Column -->
          <div class="flex flex-col items-center justify-center">
            <div class="text-center mb-4">
              <svg class="w-8 h-8 text-gray-400 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
              </svg>
              <p class="text-xs text-gray-500 mt-2">Drag & Drop to Map</p>
            </div>

            <!-- Active Mappings List -->
            <div class="w-full space-y-2">
              <div
                v-for="(mapping, index) in mappings"
                :key="index"
                class="p-2 bg-gray-100 rounded-lg flex items-center justify-between"
              >
                <div class="text-xs">
                  <span class="text-blue-600">{{ mapping.source }}</span>
                  <span class="mx-1 text-gray-400">→</span>
                  <span class="text-green-600">{{ mapping.target }}</span>
                </div>
                <button
                  @click="removeMapping(index)"
                  class="text-gray-400 hover:text-red-500"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Add Mapping Button -->
            <button
              v-if="selectedSourceField && selectedTargetField"
              @click="addMapping"
              class="mt-4 btn btn-primary btn-sm"
            >
              Add Mapping
            </button>
          </div>

          <!-- Target Fields Column -->
          <div>
            <h3 class="text-sm font-medium text-gray-700 mb-3 flex items-center">
              <span class="w-3 h-3 bg-green-500 rounded-full mr-2"></span>
              Target Fields
            </h3>
            <div 
              class="space-y-2"
              @dragover.prevent
              @drop="handleDrop"
            >
              <div
                v-for="field in targetFields"
                :key="field.name"
                class="p-3 bg-green-50 rounded-lg border border-green-100 cursor-pointer"
                :class="{ 
                  'ring-2 ring-green-500': selectedTargetField === field.name,
                  'opacity-50': isFieldMapped(field.name, 'target')
                }"
                @click="selectTargetField(field.name)"
              >
                <div class="flex items-center justify-between">
                  <span class="text-sm font-medium text-gray-900">{{ field.name }}</span>
                  <span class="text-xs text-gray-500 bg-green-100 px-2 py-0.5 rounded">{{ field.type }}</span>
                </div>
                <div class="flex items-center mt-1 text-xs">
                  <span v-if="field.required" class="text-red-500 mr-2">Required</span>
                  <span v-if="field.isPrimaryKey" class="text-purple-500">PK</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Transformations Section -->
        <div class="pt-6 border-t border-gray-200">
          <h3 class="text-lg font-medium text-gray-900 mb-4">Transformations</h3>
          <div class="space-y-4">
            <div
              v-for="(mapping, index) in mappings"
              :key="index"
              class="p-4 bg-gray-50 rounded-lg"
            >
              <div class="flex items-center justify-between mb-3">
                <span class="text-sm font-medium text-gray-900">
                  {{ mapping.source }} → {{ mapping.target }}
                </span>
              </div>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-medium text-gray-600 mb-1">Transformation</label>
                  <select v-model="mapping.transformation" class="form-input text-sm">
                    <option value="none">None (Direct Copy)</option>
                    <option value="uppercase">Uppercase</option>
                    <option value="lowercase">Lowercase</option>
                    <option value="trim">Trim Whitespace</option>
                    <option value="date_format">Date Format</option>
                    <option value="currency">Currency Format</option>
                    <option value="lookup">Lookup Table</option>
                    <option value="custom">Custom Expression</option>
                  </select>
                </div>
                <div v-if="mapping.transformation === 'custom'">
                  <label class="block text-xs font-medium text-gray-600 mb-1">Expression</label>
                  <input
                    type="text"
                    v-model="mapping.expression"
                    class="form-input text-sm"
                    placeholder="e.g., CONCAT(first_name, ' ', last_name)"
                  />
                </div>
                <div v-if="mapping.transformation === 'lookup'">
                  <label class="block text-xs font-medium text-gray-600 mb-1">Lookup Table</label>
                  <select v-model="mapping.lookupTable" class="form-input text-sm">
                    <option value="">Select lookup table</option>
                    <option value="dim_artist">dim_artist</option>
                    <option value="dim_category">dim_category</option>
                    <option value="dim_location">dim_location</option>
                    <option value="dim_time">dim_time</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="py-12 text-center">
        <svg class="w-16 h-16 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
        </svg>
        <p class="mt-4 text-gray-500">Select source and target tables to configure mappings</p>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * ETLMapping Component
 * Configure data mappings between OLTP and Data Warehouse
 */
export default {
  name: 'ETLMapping',

  data() {
    return {
      selectedSource: '',
      selectedTarget: '',
      selectedSourceField: null,
      selectedTargetField: null,
      sourceFields: [],
      targetFields: [],
      mappings: [],
      originalMappings: [],
      
      // Sample source tables (OLTP)
      sourceTables: [
        { name: 'artworks', recordCount: 542 },
        { name: 'artists', recordCount: 128 },
        { name: 'exhibitions', recordCount: 45 },
        { name: 'visitors', recordCount: 2341 },
        { name: 'sales', recordCount: 876 },
        { name: 'loans', recordCount: 67 },
        { name: 'staff', recordCount: 34 }
      ],
      
      // Sample target tables (Data Warehouse)
      targetTables: [
        { name: 'dim_artwork', type: 'Dimension' },
        { name: 'dim_artist', type: 'Dimension' },
        { name: 'dim_exhibition', type: 'Dimension' },
        { name: 'dim_visitor', type: 'Dimension' },
        { name: 'dim_time', type: 'Dimension' },
        { name: 'dim_location', type: 'Dimension' },
        { name: 'fact_sales', type: 'Fact' },
        { name: 'fact_visits', type: 'Fact' },
        { name: 'fact_loans', type: 'Fact' }
      ]
    };
  },

  computed: {
    hasChanges() {
      return JSON.stringify(this.mappings) !== JSON.stringify(this.originalMappings);
    }
  },

  methods: {
    loadSourceFields() {
      // Simulate loading fields based on selected source table
      const fieldsByTable = {
        'artworks': [
          { name: 'artwork_id', type: 'INT', nullable: false },
          { name: 'title', type: 'VARCHAR(255)', nullable: false },
          { name: 'artist_id', type: 'INT', nullable: false },
          { name: 'year_created', type: 'INT', nullable: true },
          { name: 'medium', type: 'VARCHAR(100)', nullable: true },
          { name: 'dimensions', type: 'VARCHAR(100)', nullable: true },
          { name: 'estimated_value', type: 'DECIMAL(12,2)', nullable: true },
          { name: 'acquisition_date', type: 'DATE', nullable: true },
          { name: 'status', type: 'VARCHAR(50)', nullable: false },
          { name: 'created_at', type: 'TIMESTAMP', nullable: false },
          { name: 'updated_at', type: 'TIMESTAMP', nullable: false }
        ],
        'artists': [
          { name: 'artist_id', type: 'INT', nullable: false },
          { name: 'first_name', type: 'VARCHAR(100)', nullable: false },
          { name: 'last_name', type: 'VARCHAR(100)', nullable: false },
          { name: 'birth_year', type: 'INT', nullable: true },
          { name: 'death_year', type: 'INT', nullable: true },
          { name: 'nationality', type: 'VARCHAR(100)', nullable: true },
          { name: 'biography', type: 'TEXT', nullable: true }
        ],
        'visitors': [
          { name: 'visitor_id', type: 'INT', nullable: false },
          { name: 'first_name', type: 'VARCHAR(100)', nullable: false },
          { name: 'last_name', type: 'VARCHAR(100)', nullable: false },
          { name: 'email', type: 'VARCHAR(255)', nullable: true },
          { name: 'is_member', type: 'BOOLEAN', nullable: false },
          { name: 'member_since', type: 'DATE', nullable: true },
          { name: 'total_visits', type: 'INT', nullable: false }
        ],
        'sales': [
          { name: 'sale_id', type: 'INT', nullable: false },
          { name: 'artwork_id', type: 'INT', nullable: false },
          { name: 'buyer_id', type: 'INT', nullable: false },
          { name: 'sale_date', type: 'DATE', nullable: false },
          { name: 'sale_price', type: 'DECIMAL(12,2)', nullable: false },
          { name: 'commission', type: 'DECIMAL(12,2)', nullable: true }
        ]
      };

      this.sourceFields = fieldsByTable[this.selectedSource] || [];
      this.mappings = [];
      this.selectedSourceField = null;
    },

    loadTargetFields() {
      // Simulate loading fields based on selected target table
      const fieldsByTable = {
        'dim_artwork': [
          { name: 'artwork_key', type: 'INT', required: true, isPrimaryKey: true },
          { name: 'artwork_id', type: 'INT', required: true },
          { name: 'title', type: 'VARCHAR(255)', required: true },
          { name: 'artist_key', type: 'INT', required: true },
          { name: 'year_created', type: 'INT', required: false },
          { name: 'medium', type: 'VARCHAR(100)', required: false },
          { name: 'current_value', type: 'DECIMAL(12,2)', required: false },
          { name: 'status', type: 'VARCHAR(50)', required: true },
          { name: 'effective_date', type: 'DATE', required: true },
          { name: 'expiry_date', type: 'DATE', required: false },
          { name: 'is_current', type: 'BOOLEAN', required: true }
        ],
        'dim_artist': [
          { name: 'artist_key', type: 'INT', required: true, isPrimaryKey: true },
          { name: 'artist_id', type: 'INT', required: true },
          { name: 'full_name', type: 'VARCHAR(200)', required: true },
          { name: 'birth_year', type: 'INT', required: false },
          { name: 'death_year', type: 'INT', required: false },
          { name: 'nationality', type: 'VARCHAR(100)', required: false },
          { name: 'is_living', type: 'BOOLEAN', required: true }
        ],
        'fact_sales': [
          { name: 'sale_key', type: 'INT', required: true, isPrimaryKey: true },
          { name: 'artwork_key', type: 'INT', required: true },
          { name: 'buyer_key', type: 'INT', required: true },
          { name: 'date_key', type: 'INT', required: true },
          { name: 'sale_amount', type: 'DECIMAL(12,2)', required: true },
          { name: 'commission_amount', type: 'DECIMAL(12,2)', required: false }
        ],
        'fact_visits': [
          { name: 'visit_key', type: 'INT', required: true, isPrimaryKey: true },
          { name: 'visitor_key', type: 'INT', required: true },
          { name: 'exhibition_key', type: 'INT', required: false },
          { name: 'date_key', type: 'INT', required: true },
          { name: 'ticket_price', type: 'DECIMAL(8,2)', required: true },
          { name: 'visit_duration', type: 'INT', required: false }
        ]
      };

      this.targetFields = fieldsByTable[this.selectedTarget] || [];
      this.selectedTargetField = null;
    },

    selectSourceField(fieldName) {
      this.selectedSourceField = this.selectedSourceField === fieldName ? null : fieldName;
    },

    selectTargetField(fieldName) {
      if (!this.isFieldMapped(fieldName, 'target')) {
        this.selectedTargetField = this.selectedTargetField === fieldName ? null : fieldName;
      }
    },

    isFieldMapped(fieldName, type) {
      if (type === 'source') {
        return this.mappings.some(m => m.source === fieldName);
      }
      return this.mappings.some(m => m.target === fieldName);
    },

    addMapping() {
      if (this.selectedSourceField && this.selectedTargetField) {
        this.mappings.push({
          source: this.selectedSourceField,
          target: this.selectedTargetField,
          transformation: 'none',
          expression: '',
          lookupTable: ''
        });
        this.selectedSourceField = null;
        this.selectedTargetField = null;
      }
    },

    removeMapping(index) {
      this.mappings.splice(index, 1);
    },

    handleDragStart(event, fieldName, type) {
      event.dataTransfer.setData('fieldName', fieldName);
      event.dataTransfer.setData('type', type);
    },

    handleDrop(event) {
      const fieldName = event.dataTransfer.getData('fieldName');
      const type = event.dataTransfer.getData('type');
      
      if (type === 'source' && this.selectedTargetField) {
        this.mappings.push({
          source: fieldName,
          target: this.selectedTargetField,
          transformation: 'none',
          expression: '',
          lookupTable: ''
        });
        this.selectedTargetField = null;
      }
    },

    autoMapFields() {
      // Auto-map fields with matching or similar names
      this.mappings = [];
      
      this.sourceFields.forEach(sourceField => {
        const matchingTarget = this.targetFields.find(targetField => {
          const sourceName = sourceField.name.toLowerCase().replace(/_/g, '');
          const targetName = targetField.name.toLowerCase().replace(/_/g, '');
          return sourceName === targetName || 
                 targetName.includes(sourceName) || 
                 sourceName.includes(targetName);
        });

        if (matchingTarget && !this.isFieldMapped(matchingTarget.name, 'target')) {
          this.mappings.push({
            source: sourceField.name,
            target: matchingTarget.name,
            transformation: 'none',
            expression: '',
            lookupTable: ''
          });
        }
      });
    },

    validateMappings() {
      // Check if all required target fields are mapped
      const unmappedRequired = this.targetFields
        .filter(f => f.required && !f.isPrimaryKey)
        .filter(f => !this.isFieldMapped(f.name, 'target'));

      if (unmappedRequired.length > 0) {
        alert(`Warning: The following required fields are not mapped:\n${unmappedRequired.map(f => f.name).join(', ')}`);
      } else {
        alert('All required fields are mapped. Validation passed!');
      }
    },

    saveMappings() {
      // Save mappings (in production, this would call an API)
      this.originalMappings = JSON.parse(JSON.stringify(this.mappings));
      alert('Mappings saved successfully!');
    }
  }
};
</script>
