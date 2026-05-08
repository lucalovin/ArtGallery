You are a senior full-stack developer and Oracle database architect helping complete 
a university project for the course "METODE DE OPTIMIZARE ȘI DISTRIBUIRE ÎN BAZE DE DATE" 
(MODBD) at the University of Bucharest, Master's program in Database and Software Technologies.

## PROJECT CONTEXT

The application is called "Art Gallery DW & BDD Application" and currently implements:
- An OLTP Oracle database (schema: ARTGALLERY_OLTP) with tables: Artist, Collection, 
  Location, Artwork, Visitor, Exhibitor, Exhibition, Staff, Acquisition, Loan, 
  Restoration, InsurancePolicy, Insurance, GalleryReview, ArtworkExhibition
- A Data Warehouse (schema: ARTGALLERY_DW) with star schema: FACT_EXHIBITION_ACTIVITY, 
  DIM_ARTIST, DIM_ARTWORK, DIM_COLLECTION, DIM_LOCATION, DIM_EXHIBITOR, DIM_EXHIBITION, 
  DIM_DATE, DIM_POLICY
- A .NET (C# / ASP.NET Core Web API) backend
- An Angular (Vue.js) frontend
- ETL procedures in Oracle PL/SQL (MERGE-based, full + incremental sync)
- Module 1 (OLTP CRUD), Module 2 (ETL OLTP→DW), Module 3 (Analytics/Reports dashboards)

## DISTRIBUTED DATABASE ARCHITECTURE (NEW - needs to be added/adapted)

The project must now also support a DISTRIBUTED DATABASE (BDD) layer with:

### Schemas / "Stations":
- ARTGALLERY_AM  → local database Americas (city: 'New York')
- ARTGALLERY_EU  → local database Europe (city: 'Paris', 'London', 'Madrid')
- ARTGALLERY_GLOBAL → global database (connects to both via database links: BDAM, BDEU)

### Fragmentation:
- HORIZONTAL PRIMARY: Exhibitor, Exhibition, Loan, GalleryReview, ArtworkExhibition 
  fragmented by Exhibitor.city:
    _AM fragment: city = 'New York'
    _EU fragment: city IN ('Paris','London','Madrid')
- HORIZONTAL DERIVED: Exhibition, ArtworkExhibition, GalleryReview, Loan derived from 
  Exhibitor fragmentation (FK chain)
- VERTICAL: Artwork split into:
    ARTWORK_CORE(artworkid, title, artistid) → on ARTGALLERY_EU
    ARTWORK_DETAILS(artworkid, yearcreated, medium, collectionid, locationid, estimatedvalue) → on ARTGALLERY_AM
- REPLICATED tables (static/dimension): Artist, Collection, Staff, Location, 
  InsurancePolicy → copies on both _AM and _EU, synchronized via triggers

### Global views (INSTEAD OF triggers for transparency):
- EXHIBITION = EXHIBITION_AM@BDAM UNION ALL EXHIBITION_EU@BDEU
- EXHIBITOR  = EXHIBITOR_AM@BDAM UNION ALL EXHIBITOR_EU@BDEU
- ARTWORKEXHIBITION = ARTWORKEXHIBITION_AM@BDAM UNION ALL ARTWORKEXHIBITION_EU@BDEU
- GALLERYREVIEW = GALLERYREVIEW_AM@BDAM UNION ALL GALLERYREVIEW_EU@BDEU
- ARTWORK = JOIN(ARTWORK_CORE@BDEU, ARTWORK_DETAILS@BDAM) ON artworkid
- ARTIST, COLLECTION, STAFF, LOCATION, INSURANCEPOLICY → synonyms/views pointing 
  to replicated copies

## YOUR TASK

Audit the existing application (all files in this workspace) and determine what changes 
are needed to satisfy the Module 3 (Front-End) requirements of the BDD project.

### Module 3 requirements (graded, from the project specification):

**Cerința 1 (3p) — Local data management module:**
Implement UI module(s) that allow CREATE/READ/UPDATE/DELETE of data directly on the 
LOCAL databases (ARTGALLERY_AM and ARTGALLERY_EU), NOT through the global views.
- Must allow selecting which "station" (AM or EU) to operate on
- Must show clearly that the user is operating locally (label/badge: "Americas" or "Europe")
- Must cover: Exhibitor, Exhibition, ArtworkExhibition, Loan, GalleryReview 
  (the horizontally fragmented tables), plus the vertically fragmented Artwork (CORE vs DETAILS)
- Must cover replicated tables (Artist, Collection) with changes visible on both replicas
- API endpoints must connect to local schemas, not global

**Cerința 2 (1p) — Global data visualization module:**
- A module that reads ONLY from ARTGALLERY_GLOBAL (through the global views)
- The UI must look like a unified, non-distributed system (no indication of fragmentation)
- Must show: exhibitions, artworks, exhibitors, reviews in a combined global view
- The existing analytics/reports module can be adapted for this if it reads from global views

**Cerința 3 (2p) — Local LMD → visible globally:**
- Demonstrate (with a dedicated UI page or test panel) that:
  - INSERT/UPDATE/DELETE on EXHIBITION_AM (local) → visible in the global EXHIBITION view
  - INSERT/UPDATE/DELETE on ARTWORKEXHIBITION_EU (local) → visible globally
  - UPDATE on ARTWORK_CORE (vertical fragment) → visible in global ARTWORK view (JOIN reconstructed)
  - UPDATE on a replicated table (e.g. ARTIST on _AM) → triggers sync to _EU replica, 
    visible globally
- UI must show: "before" state (global), perform local operation, "after" state (global)
- This can be a "BDD Demo" / "Distributed Test" page with step-by-step scenarios

**Cerința 4 (3p) — Global LMD → propagated to local:**
- Demonstrate that:
  - INSERT into global EXHIBITION view (via INSTEAD OF trigger) → routed to correct fragment 
    (_AM if city='New York', _EU if city='Paris')
  - UPDATE on global ARTWORK view (via INSTEAD OF trigger) → updates correct vertical fragment
  - INSERT on global ARTIST view (replicated) → propagated to both _AM and _EU replicas
- UI must show: perform global operation → verify in local fragment that data arrived
- Must show all three cases: horizontal fragment routing, vertical fragment update, 
  replicated table propagation

## AUDIT CHECKLIST — for each file you inspect, verify:

1. **API layer (.NET controllers / services):**
   - Are there separate connection strings for ARTGALLERY_AM, ARTGALLERY_EU, ARTGALLERY_GLOBAL?
   - Are there endpoints that explicitly target local schemas (not only global views)?
   - Is there a "BDD" or "Distributed" controller handling local CRUD and demo scenarios?

2. **Frontend (Angular/Vue components):**
   - Is there a "Local Management" page with station selector (AM/EU)?
   - Is there a "Global View" page that reads from ARTGALLERY_GLOBAL?
   - Is there a "BDD Demo" page showing Cerința 3 (local→global) and Cerința 4 (global→local)?
   - Do existing CRUD pages (Artworks, Exhibitions) target global or local schemas?

3. **Database connections / OracleDataSource config:**
   - Are 3 connection strings configured: one per station + global?
   - Are database links (BDAM, BDEU) referenced anywhere?

4. **Missing features — identify gaps and implement or scaffold:**
   - If there is no "local station" concept → add it
   - If all API calls go to a single connection string → add multi-tenancy per station
   - If no BDD demo page exists → create it with the 4 scenarios above

## OUTPUT FORMAT

For each file or component:
1. State: COMPLIANT / NEEDS MODIFICATION / MISSING
2. If NEEDS MODIFICATION: show the exact diff / new code
3. If MISSING: generate the full implementation

Priority order:
1. Backend: add 3 connection strings + BddController with local CRUD + demo endpoints
2. Frontend: add BDD module pages (LocalManagement.vue, GlobalView.vue, BddDemo.vue)
3. Verify existing OLTP CRUD module still works (don't break it)
4. Add API endpoints: 
   - GET/POST/PUT/DELETE /api/bdd/local/{station}/{entity}
   - GET /api/bdd/global/{entity}
   - POST /api/bdd/demo/local-to-global
   - POST /api/bdd/demo/global-to-local

## TECH STACK
- Backend: .NET 8, C#, ASP.NET Core Web API, Oracle.ManagedDataAccess.Client
- Frontend: Vue.js 3 (or Angular) + TypeScript, Axios/Fetch for HTTP
- Database: Oracle 19c, PL/SQL, database links
- Connection pattern: OracleConnection with connection string per schema

## IMPORTANT CONSTRAINTS
- DO NOT modify the existing OLTP CRUD module (Module 1) or the ETL module (Module 2)
- DO NOT change the existing DW analytics/reports module (Module 3 existing)
- The global views are already assumed to exist in ARTGALLERY_GLOBAL schema
- Use ARTGALLERY_AM / ARTGALLERY_EU as distinct Oracle users/schemas in the same instance
- Local operations must use the correct schema prefix or separate connection string
- All new UI must have screenshot-ready pages (clean layout, labeled clearly for project documentation)
- Code must be runnable and demonstrable at the exam (oral defense)