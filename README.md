# 🎨 Art Gallery Management System

A comprehensive Vue.js 3 frontend application for managing an Art Gallery, serving as the interface for both:
- **DW/BI Project** - Data Warehouse & Business Intelligence
- **Vue.js Project** - Vue.js

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Running the Application](#-running-the-application)
- [Project Structure](#-project-structure)
- [Architecture](#-architecture)
- [Vue.js Requirements Checklist](#-vuejs-requirements-checklist)
- [DW/BI Requirements Checklist](#-dwbi-requirements-checklist)
- [API Integration](#-api-integration)
- [Contributing](#-contributing)

## ✨ Features

### OLTP Data Management
- Complete CRUD operations for Artworks, Exhibitions, Visitors, Staff, Loans, and more
- Real-time form validation with error handling
- Auto-save to LocalStorage
- Search, filter, and sort functionality

### ETL & Data Warehouse Synchronization
- "Refresh DW" button to trigger ETL processes
- Progress tracking with operation logs
- Before/after data comparison
- Data validation views

### BI Reporting Dashboard
- **5 Interactive Reports:**
  1. Top Artists by Insured Value (Bar Chart)
  2. Exhibition Performance Trends (Line Chart)
  3. Insurance Coverage Status (Pie Chart)
  4. Collection Composition by Nationality (Stacked Bar)
  5. Loan Status Dashboard (KPI Cards)
- Drill-down capabilities
- Real-time data refresh

### Responsive Design
- Mobile-first approach with TailwindCSS
- Hamburger menu on mobile devices
- Flexible grid layouts

## 🛠 Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Vue.js | 3.4+ | Frontend framework (OPTIONS API) |
| Vue Router | 4.2+ | Client-side routing |
| Vuex | 4.1+ | State management |
| Axios | 1.6+ | HTTP client |
| TailwindCSS | 3.4+ | Utility-first CSS |
| Chart.js | 4.4+ | Data visualization |
| vue-chartjs | 5.3+ | Vue Chart.js wrapper |
| Vee-Validate | 4.12+ | Form validation |
| Yup | 1.3+ | Schema validation |
| Vite | 5.0+ | Build tool |
| json-server | 0.17+ | Mock API server |

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js** >= 18.0.0
- **npm** >= 9.0.0 or **yarn** >= 1.22.0
- **Git** (for version control)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd art-gallery-dw-bi-app
```

### 2. Install Dependencies

Using npm:
```bash
npm install
```

Using yarn:
```bash
yarn install
```

### 3. Environment Configuration

Create a `.env` file in the root directory:

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:3000/api

# Application Settings
VITE_APP_TITLE=Art Gallery Management System
VITE_APP_VERSION=1.0.0

# Feature Flags
VITE_ENABLE_MOCK_API=true
VITE_ENABLE_DEV_TOOLS=true
```

### 4. Setup Mock API (json-server)

Create a `db.json` file in the root directory for mock data:

```json
{
  "artworks": [],
  "exhibitions": [],
  "artists": [],
  "visitors": [],
  "loans": [],
  "restorations": [],
  "reviews": [],
  "collections": [],
  "locations": [],
  "staff": [],
  "insurances": []
}
```

## ▶️ Running the Application

### Development Mode

Start the Vite development server:

```bash
npm run dev
```

The application will be available at: `http://localhost:5173`

### Start Mock API Server

In a separate terminal, start json-server:

```bash
npm run json-server
```

The mock API will be available at: `http://localhost:3000`

### Production Build

Build for production:

```bash
npm run build
```

Preview production build:

```bash
npm run preview
```

## 📁 Project Structure

```
art-gallery-dw-bi-app/
├── public/                     # Static assets
├── src/
│   ├── assets/
│   │   ├── styles/
│   │   │   └── tailwind.css   # TailwindCSS entry point
│   │   └── images/            # Image assets
│   │
│   ├── components/
│   │   ├── common/            # Reusable components
│   │   │   ├── NavigationMenu.vue
│   │   │   ├── Footer.vue
│   │   │   ├── FormComponent.vue
│   │   │   ├── ListComponent.vue
│   │   │   └── ItemComponent.vue
│   │   ├── artwork/           # Artwork-specific components
│   │   ├── exhibition/        # Exhibition-specific components
│   │   ├── visitor/           # Visitor-specific components
│   │   ├── reports/           # Chart components
│   │   └── etl/               # ETL status components
│   │
│   ├── pages/                 # Page/View components
│   │   ├── Home.vue
│   │   ├── ArtworkInventory.vue
│   │   ├── ArtworkDetail.vue
│   │   ├── AddEditArtwork.vue
│   │   ├── ExhibitionInventory.vue
│   │   ├── ExhibitionDetail.vue
│   │   ├── ETLManagement.vue
│   │   ├── Reports.vue
│   │   └── NotFound.vue
│   │
│   ├── store/                 # Vuex store
│   │   ├── modules/
│   │   │   ├── artworkStore.js
│   │   │   ├── exhibitionStore.js
│   │   │   ├── visitorStore.js
│   │   │   ├── staffStore.js
│   │   │   ├── loansStore.js
│   │   │   └── reportsStore.js
│   │   └── index.js
│   │
│   ├── api/                   # API layer
│   │   ├── client.js
│   │   ├── artworkAPI.js
│   │   ├── exhibitionAPI.js
│   │   ├── visitorAPI.js
│   │   ├── etlAPI.js
│   │   └── reportsAPI.js
│   │
│   ├── utils/                 # Utilities
│   │   ├── validators.js
│   │   ├── formatters.js
│   │   ├── localStorageService.js
│   │   └── mockData.js
│   │
│   ├── router/                # Vue Router
│   │   └── index.js
│   │
│   ├── App.vue                # Root component
│   └── main.js                # Entry point
│
├── package.json
├── vite.config.js
├── tailwind.config.js
├── postcss.config.js
└── README.md
```

## 🏗 Architecture

### Component Pattern: OPTIONS API

All components follow the Vue 3 OPTIONS API pattern:

```vue
<template>
  <!-- Template markup -->
</template>

<script>
export default {
  name: 'ComponentName',
  components: {},
  props: {},
  emits: [],
  data() {
    return {
      // Reactive state
    };
  },
  computed: {
    // Derived properties
  },
  watch: {
    // Watchers
  },
  methods: {
    // Methods
  },
  created() {
    // Initialize data
  },
  mounted() {
    // DOM-dependent init
  },
  beforeUnmount() {
    // Cleanup
  }
};
</script>

<style scoped>
/* Scoped styles */
</style>
```

### State Management with Vuex

```javascript
// Accessing state in components
this.$store.state.artwork.artworks

// Committing mutations
this.$store.commit('artwork/SET_ARTWORKS', artworks)

// Dispatching actions
this.$store.dispatch('artwork/fetchArtworks')

// Using getters
this.$store.getters['artwork/artworkById'](id)
```

## ✅ Vue.js Requirements Checklist

| # | Requirement | Status | Implementation |
|---|-------------|--------|----------------|
| 1 | Minimum 4 pages | ✅ | Home, Inventory, Details, Add/Edit, Reports |
| 2 | Vue Router | ✅ | Basic, dynamic, and nested routes |
| 3 | 5+ Reusable components | ✅ | NavigationMenu, Footer, FormComponent, ListComponent, ItemComponent |
| 4 | Data manipulation | ✅ | v-model, validation, filtering, sorting |
| 5 | Data persistence | ✅ | LocalStorage + Mock API |
| 6 | Props & Custom Events | ✅ | Parent/child communication |
| 7 | Lifecycle hooks | ✅ | created(), mounted(), beforeUnmount() |
| 8 | Vuex (2+ modules) | ✅ | artwork, exhibition, visitor, staff, loans, reports |
| 9 | Responsive design | ✅ | TailwindCSS + mobile menu |
| 10 | Async functionality | ✅ | Axios HTTP calls with error handling |

## 📊 DW/BI Requirements Checklist

| Module | Requirement | Status | Implementation |
|--------|-------------|--------|----------------|
| 3.1 | OLTP CRUD | ✅ | Full CRUD for all entities |
| 3.1 | Form validation | ✅ | Real-time Vee-Validate + Yup |
| 3.2 | ETL trigger | ✅ | "Refresh DW" button |
| 3.2 | Progress tracking | ✅ | Operation logs, timestamps |
| 3.2 | Before/after comparison | ✅ | DataValidationView component |
| 3.3 | 5 BI reports | ✅ | Bar, Line, Pie, Stacked Bar, KPI |
| 3.3 | Interactive charts | ✅ | Drill-down, filters |
| 3.3 | Real-time refresh | ✅ | Post-ETL data update |

## 🔌 API Integration

### Current: Mock API (json-server)

```javascript
// Base URL: http://localhost:3000
GET    /artworks      // Get all artworks
GET    /artworks/:id  // Get artwork by ID
POST   /artworks      // Create artwork
PUT    /artworks/:id  // Update artwork
DELETE /artworks/:id  // Delete artwork
```

### Future: .NET 10 REST API

To switch to the .NET backend, update the `.env` file:

```env
VITE_API_BASE_URL=https://your-dotnet-api.com/api
```

The API layer is designed for easy backend swapping.

## 📝 Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production |
| `npm run preview` | Preview production build |
| `npm run lint` | Run ESLint |
| `npm run json-server` | Start mock API server |

## 🤝 Contributing

1. Create a feature branch
2. Make your changes following OPTIONS API pattern
3. Run linting: `npm run lint`
4. Test your changes
5. Submit a pull request

## 📄 License

MIT License - See LICENSE file for details.

---

**Art Gallery Management System** | Vue.js 3 | DW/BI Project | Master's Degree

Built with ❤️ using Vue.js 3 OPTIONS API
