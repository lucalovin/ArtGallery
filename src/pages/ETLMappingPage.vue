<template>
  <!--
    ETLMappingPage.vue - ETL Field Mapping Page
    Art Gallery Management System - DW/BI Module
  -->
  <div class="etl-mapping-page">
    <button @click="goBack" class="inline-flex items-center text-gray-500 hover:text-gray-700 mb-6">
      <span class="mr-2">←</span>
      Back to ETL Dashboard
    </button>

    <header class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900">ETL Field Mapping</h1>
      <p class="text-gray-500 mt-1">Configure field mappings between OLTP and Data Warehouse</p>
    </header>

    <etl-mapping
      :source-tables="sourceTables"
      :target-tables="targetTables"
      @save-mapping="saveMapping"
    />
  </div>
</template>

<script>
import ETLMapping from '@/components/etl/ETLMapping.vue';

/**
 * ETLMappingPage
 */
export default {
  name: 'ETLMappingPage',

  components: {
    'etl-mapping': ETLMapping
  },

  data() {
    return {
      sourceTables: [
        { name: 'artworks', fields: ['id', 'title', 'artist', 'year', 'category', 'status', 'created_at'] },
        { name: 'exhibitions', fields: ['id', 'title', 'start_date', 'end_date', 'curator_id', 'status'] },
        { name: 'visitors', fields: ['id', 'name', 'email', 'membership_type', 'visit_count'] }
      ],
      targetTables: [
        { name: 'dim_artwork', fields: ['artwork_key', 'artwork_id', 'title', 'artist_name', 'year_created', 'category'] },
        { name: 'dim_exhibition', fields: ['exhibition_key', 'exhibition_id', 'title', 'duration_days', 'curator_name'] },
        { name: 'fact_visits', fields: ['visit_key', 'date_key', 'visitor_key', 'exhibition_key', 'ticket_revenue'] }
      ]
    };
  },

  methods: {
    goBack() {
      this.$router.push('/etl');
    },

    saveMapping(mapping) {
      console.log('Saving mapping:', mapping);
      // Save to backend
    }
  }
};
</script>

<style scoped>
.etl-mapping-page {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
}
</style>
