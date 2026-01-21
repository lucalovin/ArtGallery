<template>
  <!--
    NotFound.vue - 404 Page
    Art Gallery Management System
  -->
  <div class="not-found-page">
    <div class="text-center">
      <span class="text-8xl mb-6 block">🖼️</span>
      <h1 class="text-6xl font-bold text-gray-900 mb-4">404</h1>
      <h2 class="text-2xl font-semibold text-gray-700 mb-4">Page Not Found</h2>
      <p class="text-gray-500 mb-8 max-w-md mx-auto">
        The page you're looking for doesn't exist or has been moved to a different gallery.
      </p>
      <div class="flex items-center justify-center space-x-4">
        <button
          @click="goBack"
          class="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors font-medium"
        >
          Go Back
        </button>
        <router-link
          to="/"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors font-medium"
        >
          Return Home
        </router-link>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * NotFound Page
 */
export default {
  name: 'NotFoundPage',

  methods: {
    goBack() {
      this.$router.go(-1);
    }
  }
};
</script>

<style scoped>
.not-found-page {
  min-height: 80vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}
</style>
