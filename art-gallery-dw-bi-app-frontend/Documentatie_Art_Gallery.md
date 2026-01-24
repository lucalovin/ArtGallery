# Art Gallery Management System

## Documentație Tehnică

---

**Autor:** Luca Lovin

**Versiune:** 1.0.0

**Data:** Ianuarie 2026

**Framework:** Vue.js 3 (Options API)

**Host URL:** https://mango-field-033899a03.2.azurestaticapps.net/

**GitHub URL:** https://github.com/lucalovin/ArtGallery

---

\newpage

## Cuprins

1. [Descrierea Aplicației și a Temei Alese](#1-descrierea-aplicației-și-a-temei-alese)
2. [Structura Componentelor](#2-structura-componentelor)
3. [Funcționalitățile Implementate](#3-funcționalitățile-implementate)
4. [Deployment Azure](#4-deployment-azure)
5. [Capturi de Ecran](#5-capturi-de-ecran)

---

\newpage

## 1. Descrierea Aplicației și a Temei Alese

### 1.1 Prezentare Generală

**Art Gallery Management System** este o aplicație web modernă construită cu Vue.js 3, destinată gestionării complete a unei galerii de artă. Aplicația oferă funcționalități pentru:

- Gestionarea operelor de artă (CRUD complet)
- Organizarea expozițiilor
- Înregistrarea vizitatorilor
- Managementul personalului
- Gestionarea împrumuturilor și asigurărilor
- Procese ETL pentru sincronizare Data Warehouse
- Dashboard BI cu rapoarte interactive

### 1.2 Tema Aleasă: Galeria de Artă

Tema **Galeria de Artă** a fost aleasă pentru complexitatea și diversitatea funcționalităților pe care le permite:

**Entități principale:**
- **Artworks** - opere de artă cu detalii complete (artist, an, dimensiuni, valoare asigurată)
- **Exhibitions** - expoziții cu date, locație, opere incluse
- **Visitors** - vizitatori înregistrați cu istoric vizite
- **Staff** - personal cu roluri și responsabilități
- **Loans** - împrumuturi de opere către alte instituții

### 1.3 Stack Tehnologic

| Tehnologie | Versiune | Scop |
|------------|---------|------|
| Vue.js | 3.4+ | Framework frontend (Options API) |
| Vue Router | 4.2+ | Rutare SPA |
| Vuex | 4.1+ | State management |
| Axios | 1.6+ | HTTP client |
| TailwindCSS | 3.4+ | Framework CSS |
| Chart.js | 4.4+ | Vizualizări date |
| Vee-Validate + Yup | 4.12+ | Validare formulare |
| Vite | 5.0+ | Build tool |

---

\newpage

## 2. Structura Componentelor

### 2.1 Arhitectura Aplicației

```
src/
├── App.vue                 # Root component
├── main.js                 # Entry point
├── components/             # Componente reutilizabile
│   ├── artworks/          # Componente opere
│   ├── exhibitions/       # Componente expoziții
│   ├── visitors/          # Componente vizitatori
│   ├── staff/             # Componente personal
│   ├── loans/             # Componente împrumuturi
│   ├── etl/               # Componente ETL
│   ├── reports/           # Componente rapoarte BI
│   └── common/            # Componente generice
├── pages/                  # Page components
├── router/                 # Vue Router config
├── store/                  # Vuex modules
├── api/                    # API services
└── utils/                  # Funcții utilitare
```

### 2.2 Componente Principale

#### Common Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **NavigationMenu.vue** | Meniu principal cu suport mobile | - |
| **FormComponent.vue** | Formular generic cu validare | `@submit`, `fields`, `initialData` |
| **ListComponent.vue** | Listă generică cu paginare | `items`, `columns`, `@edit`, `@delete` |
| **Footer.vue** | Footer global | - |

#### Artwork Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **ArtworkList.vue** | Grid opere cu search/filter | `@view-details` |
| **ArtworkCard.vue** | Card operă de artă | `artwork`, `@edit`, `@delete` |
| **ArtworkDetail.vue** | Detalii complete operă | `id` (route param) |
| **ArtworkForm.vue** | Formular add/edit | `artwork`, `@submit` |

#### Exhibition Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **ExhibitionList.vue** | Listă expoziții cu tabs | - |
| **ExhibitionCard.vue** | Card expoziție | `exhibition` |
| **ExhibitionForm.vue** | Formular expoziție | `exhibition`, `@submit` |

#### Visitor & Staff Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **VisitorCard.vue** | Card vizitator | `visitor` |
| **VisitorForm.vue** | Formular vizitator | `@submit` |
| **StaffCard.vue** | Card personal | `staff` |
| **StaffForm.vue** | Formular personal | `@submit` |

#### Loan Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **LoanCard.vue** | Card împrumut | `loan` |
| **LoanForm.vue** | Formular împrumut | `@submit` |
| **InsuranceCard.vue** | Card asigurare | `insurance` |
| **RestorationCard.vue** | Card restaurare | `restoration` |

#### ETL Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **ETLDashboard.vue** | Dashboard ETL cu "Refresh DW" | - |
| **ETLStatus.vue** | Status proces ETL | `status` |
| **ETLLog.vue** | Vizualizare logs | - |
| **ETLMapping.vue** | Mapping OLTP → DW | - |
| **ETLHistory.vue** | Istoric execuții | - |

#### Reports Components

| Component | Descriere | Props/Events |
|-----------|-----------|--------------|
| **ReportsDashboard.vue** | Dashboard BI principal | - |
| **KPICard.vue** | Card KPI | `title`, `value`, `trend` |
| **ReportTable.vue** | Tabel rapoarte | `data`, `columns` |
| **BarChart.vue** | Grafic bare | `data`, `options` |
| **LineChart.vue** | Grafic linii | `data`, `options` |
| **PieChart.vue** | Grafic pie | `data`, `options` |
| **DoughnutChart.vue** | Grafic doughnut | `data`, `options` |

### 2.3 Pages (Route Views)

| Page | Route | Descriere |
|------|-------|-----------|
| Home.vue | `/` | Dashboard principal |
| ArtworkInventory.vue | `/artworks` | Inventar opere |
| ArtworkDetail.vue | `/artworks/:id` | Detalii operă (dynamic route) |
| AddEditArtwork.vue | `/artworks/new`, `/artworks/:id/edit` | Add/Edit operă |
| ExhibitionInventory.vue | `/exhibitions` | Listă expoziții |
| ExhibitionDetail.vue | `/exhibitions/:id` | Detalii expoziție |
| AddEditExhibition.vue | `/exhibitions/new` | Add/Edit expoziție |
| VisitorManagement.vue | `/visitors` | Management vizitatori |
| StaffManagement.vue | `/staff` | Management personal |
| LoanManagement.vue | `/loans` | Management împrumuturi |
| ETLManagement.vue | `/etl` | ETL Dashboard |
| ETLMappingPage.vue | `/etl/mapping` | Mapping (nested route) |
| ETLHistoryPage.vue | `/etl/history` | Istoric (nested route) |
| Reports.vue | `/reports` | Rapoarte BI |
| NotFound.vue | `*` | Pagina 404 |

### 2.4 Vuex Store Modules

| Modul | State Principal | Actions |
|-------|-----------------|---------|
| **artworkStore** | `artworks`, `currentArtwork` | `fetchArtworks`, `createArtwork`, `updateArtwork`, `deleteArtwork` |
| **exhibitionStore** | `exhibitions` | `fetchExhibitions`, CRUD operations |
| **visitorStore** | `visitors`, `reviews` | `fetchVisitors`, CRUD operations |
| **staffStore** | `staff`, `activities` | `fetchStaff`, CRUD operations |
| **loansStore** | `loans`, `insurances`, `restorations` | `fetchLoans`, CRUD operations |
| **reportsStore** | `reports` | `fetchTopArtists`, `fetchExhibitionPerformance`, etc. |

### 2.5 API Services

| Service | Endpoint | Metode |
|---------|----------|--------|
| **artworkAPI.js** | `/artworks` | GET, POST, PUT, DELETE |
| **exhibitionAPI.js** | `/exhibitions` | GET, POST, PUT, DELETE |
| **visitorAPI.js** | `/visitors` | GET, POST, PUT, DELETE |
| **etlAPI.js** | `/etl` | `triggerETL()`, `getStatus()`, `getLogs()` |
| **reportsAPI.js** | `/reports` | `getTopArtists()`, `getExhibitionPerformance()` |

---

\newpage

## 3. Funcționalitățile Implementate

### 3.1 Cerințe Vue.js Îndeplinite

| Cerință | Status | Implementare |
|---------|--------|--------------|
| Vue.js 3 cu Options API | ✅ | Toate componentele folosesc Options API |
| Vue Router | ✅ | Basic, dynamic (`:id`), nested routes |
| Vuex State Management | ✅ | 6 module namespaced |
| Axios HTTP | ✅ | API client cu interceptors |
| Minimum 10 componente | ✅ | 40+ componente implementate |
| Props și Events | ✅ | În toate componentele |
| Lifecycle Hooks | ✅ | `created`, `mounted`, `updated`, `beforeUnmount` |
| Computed Properties | ✅ | Filtrare, sortare, calcule derivate |
| Watchers | ✅ | Search debounce, route changes |
| Directives | ✅ | `v-if`, `v-for`, `v-model`, `v-bind`, `v-on` |
| Form Validation | ✅ | Vee-Validate + Yup |
| CSS Framework | ✅ | TailwindCSS 3.4+ |
| Responsive Design | ✅ | Mobile-first, hamburger menu |

### 3.2 CRUD Operations

Operații complete pentru toate entitățile:

**Create:**
- Formulare cu validare pentru fiecare entitate
- Error handling și success notifications

**Read:**
- Liste cu paginare, search și filtrare
- Detail views cu informații complete

**Update:**
- Edit forms pre-populat cu date existente
- Validare la submit

**Delete:**
- Confirmation dialogs
- Toast notifications

### 3.3 Routing

**Basic Routes:**
```javascript
{ path: '/', component: Home }
{ path: '/artworks', component: ArtworkInventory }
{ path: '/reports', component: Reports }
```

**Dynamic Routes:**
```javascript
{ path: '/artworks/:id', component: ArtworkDetail, props: true }
{ path: '/exhibitions/:id', component: ExhibitionDetail, props: true }
```

**Nested Routes:**
```javascript
{
  path: '/etl',
  component: ETLManagement,
  children: [
    { path: 'mapping', component: ETLMappingPage },
    { path: 'history', component: ETLHistoryPage }
  ]
}
```

### 3.4 State Management (Vuex)

```javascript
// Exemplu artworkStore
export default {
  namespaced: true,
  state: () => ({
    artworks: [],
    isLoading: false
  }),
  getters: {
    filteredArtworks: (state) => { /* ... */ }
  },
  mutations: {
    SET_ARTWORKS(state, artworks) { state.artworks = artworks; }
  },
  actions: {
    async fetchArtworks({ commit }) {
      const response = await artworkAPI.getAll();
      commit('SET_ARTWORKS', response.data);
    }
  }
}
```

### 3.5 ETL Integration

- **Buton "Refresh DW"** pentru trigger proces ETL
- **Progress tracking** în timp real (Extract → Transform → Load)
- **Logs viewer** cu filtrare după severity
- **Istoric** execuții cu status success/failed
- **Mapping view** pentru câmpuri OLTP → DW

### 3.6 Rapoarte BI

5 rapoarte interactive implementate:

| Raport | Tip Grafic | Date |
|--------|-----------|------|
| Top Artists by Insured Value | Bar Chart | Artist, valoare totală |
| Exhibition Performance | Line Chart | Vizitatori în timp |
| Insurance Coverage | Pie Chart | Status asigurare |
| Collection by Nationality | Stacked Bar | Naționalitate, mediu |
| Loan Status | KPI Cards | Active, overdue, total |

### 3.7 Form Validation

Validare cu Vee-Validate și Yup:

```javascript
const schema = yup.object({
  title: yup.string().required('Titlul este obligatoriu'),
  artist: yup.string().required('Artistul este obligatoriu'),
  year: yup.number()
    .min(1000, 'Anul trebuie să fie după 1000')
    .max(2026, 'Anul nu poate fi în viitor')
    .required('Anul este obligatoriu')
});
```

### 3.8 Responsive Design

- **Mobile-first** approach cu TailwindCSS
- **Hamburger menu** pentru dispozitive mobile
- **Grid responsive** pentru liste și cards
- **Breakpoints:** sm (640px), md (768px), lg (1024px), xl (1280px)

---

\newpage

## 4. Deployment Azure

### 4.1 Configurare Azure Static Web Apps

Aplicația este deployed pe **Azure Static Web Apps** cu configurare automată pentru Vue.js SPA.

**URL Producție:** https://mango-field-033899a03.2.azurestaticapps.net/

### 4.2 Configurare staticwebapp.config.json

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "*.{css,js,png,jpg,svg,ico}"]
  },
  "routes": [
    {
      "route": "/*",
      "serve": "/index.html",
      "statusCode": 200
    }
  ]
}
```

### 4.3 GitHub Actions CI/CD

Deployment automat la fiecare push pe branch-ul `main`:

1. **Build:** `npm run build` generează folder `dist/`
2. **Deploy:** Azure Static Web Apps Action uploadează în Azure
3. **URL:** Aplicația este disponibilă instant

### 4.4 Repository GitHub

**URL:** https://github.com/lucalovin/ArtGallery

Conține:
- Cod sursă complet
- Configurații build (Vite, TailwindCSS)
- GitHub Actions workflow
- Documentație

---

\newpage

## 5. Capturi de Ecran

### 5.1 Dashboard Principal (Home)

*[Inserați screenshot aici]*

---

### 5.2 Artwork Inventory

*[Inserați screenshot aici]*

---

### 5.3 Artwork Detail

*[Inserați screenshot aici]*

---

### 5.4 Add/Edit Artwork Form

*[Inserați screenshot aici]*

---

### 5.5 Exhibition List

*[Inserați screenshot aici]*

---

### 5.6 Exhibition Detail

*[Inserați screenshot aici]*

---

### 5.7 Visitor Management

*[Inserați screenshot aici]*

---

### 5.8 Staff Management

*[Inserați screenshot aici]*

---

### 5.9 Loan Management

*[Inserați screenshot aici]*

---

### 5.10 ETL Dashboard

*[Inserați screenshot aici]*

---

### 5.11 ETL Progress / Refresh DW

*[Inserați screenshot aici]*

---

### 5.12 Reports Dashboard

*[Inserați screenshot aici]*

---

### 5.13 Bar Chart - Top Artists

*[Inserați screenshot aici]*

---

### 5.14 Line Chart - Exhibition Performance

*[Inserați screenshot aici]*

---

### 5.15 Pie Chart - Insurance Coverage

*[Inserați screenshot aici]*

---

### 5.16 Mobile Responsive View

*[Inserați screenshot aici]*

---

### 5.17 Form Validation

*[Inserați screenshot aici]*

---

### 5.18 Toast Notifications

*[Inserați screenshot aici]*

---

\newpage

## Referințe

- Vue.js 3 Documentation: https://vuejs.org/
- Vue Router: https://router.vuejs.org/
- Vuex: https://vuex.vuejs.org/
- TailwindCSS: https://tailwindcss.com/
- Chart.js: https://www.chartjs.org/
- Azure Static Web Apps: https://azure.microsoft.com/en-us/products/app-service/static

---

**© 2026 Luca Lovin - Art Gallery Management System**
