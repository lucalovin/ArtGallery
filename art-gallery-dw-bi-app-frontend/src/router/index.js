/**
 * Vue Router Configuration
 * Art Gallery Management System
 * 
 * This file configures Vue Router 4 for the application.
 * Includes basic routes, dynamic routes, and nested routes.
 * 
 * Routes:
 * - / (Home) - Dashboard with KPIs
 * - /artworks - Artwork inventory list
 * - /artworks/:id - Artwork details (dynamic)
 * - /artworks/new - Add new artwork
 * - /artworks/:id/edit - Edit artwork (dynamic + nested)
 * - /exhibitions - Exhibition inventory list
 * - /exhibitions/:id - Exhibition details
 * - /exhibitions/new - Add new exhibition
 * - /exhibitions/:id/edit - Edit exhibition
 * - /etl - ETL Management page
 * - /reports - BI Reports dashboard
 * - 404 - Not Found page
 */

import { createRouter, createWebHistory } from 'vue-router';

// Lazy-loaded page components for code splitting
// This improves initial load performance

const Home = () => import('@/pages/Home.vue');
const ArtworkInventory = () => import('@/pages/ArtworkInventory.vue');
const ArtworkDetail = () => import('@/pages/ArtworkDetail.vue');
const AddEditArtwork = () => import('@/pages/AddEditArtwork.vue');
const ExhibitionInventory = () => import('@/pages/ExhibitionInventory.vue');
const ExhibitionDetail = () => import('@/pages/ExhibitionDetail.vue');
const AddEditExhibition = () => import('@/pages/AddEditExhibition.vue');
const VisitorManagement = () => import('@/pages/VisitorManagement.vue');
const AddEditVisitor = () => import('@/pages/AddEditVisitor.vue');
const StaffManagement = () => import('@/pages/StaffManagement.vue');
const AddEditStaff = () => import('@/pages/AddEditStaff.vue');
const LoanManagement = () => import('@/pages/LoanManagement.vue');
const AddEditLoan = () => import('@/pages/AddEditLoan.vue');
const ETLManagement = () => import('@/pages/ETLManagement.vue');
const ETLMappingPage = () => import('@/pages/ETLMappingPage.vue');
const ETLHistoryPage = () => import('@/pages/ETLHistoryPage.vue');
const Reports = () => import('@/pages/Reports.vue');
const NotFound = () => import('@/pages/NotFound.vue');

/**
 * Route definitions
 * Each route includes:
 * - path: URL path
 * - name: Route name for programmatic navigation
 * - component: Page component (lazy-loaded)
 * - meta: Additional route metadata (title, requires auth, etc.)
 */
const routes = [
  // ========================================
  // HOME ROUTE
  // ========================================
  {
    path: '/',
    name: 'Home',
    component: Home,
    meta: {
      title: 'Dashboard',
      description: 'Art Gallery Management Dashboard with KPIs',
      icon: 'home'
    }
  },

  // ========================================
  // ARTWORK ROUTES
  // ========================================
  {
    path: '/artworks',
    name: 'ArtworkInventory',
    component: ArtworkInventory,
    meta: {
      title: 'Artwork Inventory',
      description: 'Browse and manage artworks collection',
      icon: 'photo'
    }
  },
  {
    path: '/artworks/new',
    name: 'AddArtwork',
    component: AddEditArtwork,
    meta: {
      title: 'Add New Artwork',
      description: 'Create a new artwork entry',
      icon: 'plus',
      isForm: true
    }
  },
  {
    // Dynamic route for artwork details
    path: '/artworks/:id',
    name: 'ArtworkDetail',
    component: ArtworkDetail,
    props: true, // Pass route params as props
    meta: {
      title: 'Artwork Details',
      description: 'View artwork information',
      icon: 'photo'
    }
  },
  {
    // Nested dynamic route for editing
    path: '/artworks/:id/edit',
    name: 'EditArtwork',
    component: AddEditArtwork,
    props: true,
    meta: {
      title: 'Edit Artwork',
      description: 'Modify artwork information',
      icon: 'pencil',
      isForm: true
    }
  },

  // ========================================
  // EXHIBITION ROUTES
  // ========================================
  {
    path: '/exhibitions',
    name: 'ExhibitionInventory',
    component: ExhibitionInventory,
    meta: {
      title: 'Exhibition Inventory',
      description: 'Browse and manage exhibitions',
      icon: 'calendar'
    }
  },
  {
    path: '/exhibitions/new',
    name: 'AddExhibition',
    component: AddEditExhibition,
    meta: {
      title: 'Add New Exhibition',
      description: 'Create a new exhibition',
      icon: 'plus',
      isForm: true
    }
  },
  {
    path: '/exhibitions/:id/edit',
    name: 'EditExhibition',
    component: AddEditExhibition,
    props: true,
    meta: {
      title: 'Edit Exhibition',
      description: 'Modify exhibition information',
      icon: 'pencil',
      isForm: true
    }
  },
  {
    path: '/exhibitions/:id',
    name: 'ExhibitionDetail',
    component: ExhibitionDetail,
    props: true,
    meta: {
      title: 'Exhibition Details',
      description: 'View exhibition information',
      icon: 'calendar'
    }
  },

  // ========================================
  // ETL MANAGEMENT ROUTE
  // ========================================
  {
    path: '/etl',
    name: 'ETLManagement',
    component: ETLManagement,
    meta: {
      title: 'ETL Management',
      description: 'Data Warehouse synchronization and ETL operations',
      icon: 'database'
    }
  },
  {
    path: '/etl/mapping',
    name: 'ETLMapping',
    component: ETLMappingPage,
    meta: {
      title: 'ETL Mapping',
      description: 'Configure field mappings for ETL operations',
      icon: 'database'
    }
  },
  {
    path: '/etl/history',
    name: 'ETLHistory',
    component: ETLHistoryPage,
    meta: {
      title: 'ETL History',
      description: 'View ETL sync history and logs',
      icon: 'database'
    }
  },

  // ========================================
  // VISITOR ROUTES
  // ========================================
  {
    path: '/visitors',
    name: 'VisitorManagement',
    component: VisitorManagement,
    meta: {
      title: 'Visitor Management',
      description: 'Manage gallery visitors and memberships',
      icon: 'users'
    }
  },
  {
    path: '/visitors/new',
    name: 'AddVisitor',
    component: AddEditVisitor,
    meta: {
      title: 'Add New Visitor',
      description: 'Register a new visitor',
      icon: 'user-plus',
      isForm: true
    }
  },
  {
    path: '/visitors/:id/edit',
    name: 'EditVisitor',
    component: AddEditVisitor,
    props: true,
    meta: {
      title: 'Edit Visitor',
      description: 'Update visitor information',
      icon: 'pencil',
      isForm: true
    }
  },

  // ========================================
  // STAFF ROUTES
  // ========================================
  {
    path: '/staff',
    name: 'StaffManagement',
    component: StaffManagement,
    meta: {
      title: 'Staff Management',
      description: 'Manage gallery staff and employees',
      icon: 'users'
    }
  },
  {
    path: '/staff/new',
    name: 'AddStaff',
    component: AddEditStaff,
    meta: {
      title: 'Add New Staff',
      description: 'Add a new staff member',
      icon: 'user-plus',
      isForm: true
    }
  },
  {
    path: '/staff/:id/edit',
    name: 'EditStaff',
    component: AddEditStaff,
    props: true,
    meta: {
      title: 'Edit Staff',
      description: 'Update staff information',
      icon: 'pencil',
      isForm: true
    }
  },

  // ========================================
  // LOAN ROUTES
  // ========================================
  {
    path: '/loans',
    name: 'LoanManagement',
    component: LoanManagement,
    meta: {
      title: 'Loan Management',
      description: 'Manage artwork loans and agreements',
      icon: 'document'
    }
  },
  {
    path: '/loans/new',
    name: 'AddLoan',
    component: AddEditLoan,
    meta: {
      title: 'Add New Loan',
      description: 'Create a new loan agreement',
      icon: 'plus',
      isForm: true
    }
  },
  {
    path: '/loans/:id/edit',
    name: 'EditLoan',
    component: AddEditLoan,
    props: true,
    meta: {
      title: 'Edit Loan',
      description: 'Update loan agreement',
      icon: 'pencil',
      isForm: true
    }
  },

  // ========================================
  // BI REPORTS ROUTE
  // ========================================
  {
    path: '/reports',
    name: 'Reports',
    component: Reports,
    meta: {
      title: 'BI Reports',
      description: 'Business Intelligence dashboards and reports',
      icon: 'chart-bar'
    }
  },

  // ========================================
  // 404 NOT FOUND ROUTE
  // ========================================
  {
    // Catch-all route for 404
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFound,
    meta: {
      title: 'Page Not Found',
      description: 'The requested page could not be found'
    }
  }
];

/**
 * Create router instance with HTML5 History mode
 * History mode provides clean URLs without hash (#)
 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  
  // Scroll behavior configuration
  scrollBehavior(to, from, savedPosition) {
    // If user navigates back, restore scroll position
    if (savedPosition) {
      return savedPosition;
    }
    
    // If navigating to an anchor, scroll to it
    if (to.hash) {
      return {
        el: to.hash,
        behavior: 'smooth'
      };
    }
    
    // Default: scroll to top
    return { top: 0, behavior: 'smooth' };
  }
});

/**
 * Global navigation guard: beforeEach
 * Runs before every route navigation
 */
router.beforeEach((to, from, next) => {
  // Update document title
  const appName = 'Art Gallery';
  const pageTitle = to.meta.title || 'Page';
  document.title = `${pageTitle} | ${appName}`;

  // Log navigation in development
  if (import.meta.env.DEV) {
    console.log(`🚀 Navigating: ${from.path} → ${to.path}`);
  }

  // Continue navigation
  next();
});

/**
 * Global navigation guard: afterEach
 * Runs after every route navigation
 */
router.afterEach((to, from) => {
  // Track page views (placeholder for analytics)
  if (import.meta.env.PROD) {
    // TODO: Send to analytics service
    // analytics.trackPageView(to.path, to.meta.title);
  }
});

/**
 * Global error handler for navigation failures
 */
router.onError((error) => {
  console.error('Navigation error:', error);
  
  // Handle chunk loading errors (code splitting)
  if (error.message.includes('Failed to fetch dynamically imported module')) {
    // Reload the page to get fresh assets
    window.location.reload();
  }
});

export default router;
