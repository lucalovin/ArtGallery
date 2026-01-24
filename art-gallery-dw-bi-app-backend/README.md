﻿# Art Gallery Management System - Backend API

A production-ready .NET 10 Web API backend using Clean Architecture for the Art Gallery Management System. **Now with Oracle Database support for OLTP and Data Warehouse operations.**

## 🏗️ Architecture

This project follows **Clean Architecture** with the following layers:

- **ArtGallery.Domain** - Core business entities, enums, and interfaces (includes DW dimension entities)
- **ArtGallery.Application** - DTOs, services, validators, and AutoMapper profiles
- **ArtGallery.Infrastructure** - EF Core DbContext (Oracle), repositories, and Oracle-specific services
- **ArtGallery.API** - Controllers, middleware, rate limiting, and API configuration

## 🗄️ Oracle Database Integration

This API connects to Oracle Database hosted on Azure with two schemas:

### OLTP Schema (ART_GALLERY_OLTP)
- 12 operational tables for day-to-day gallery operations
- Full CRUD operations via REST APIs
- Audit trails with CreatedAt/UpdatedAt timestamps

### DW Schema (ART_GALLERY_DW)
- 8 dimension tables (DIM_ARTIST, DIM_ARTWORK, DIM_EXHIBITION, etc.)
- 1 partitioned fact table (FACT_EXHIBITION_ACTIVITY)
- SCD Type 2 support for slowly changing dimensions
- Date dimension for time-based analytics

### Key Oracle Features
- Oracle.EntityFrameworkCore for EF Core integration
- Connection pooling configured via connection string
- Oracle-specific exception handling (ORA codes)
- PL/SQL procedure execution for ETL operations

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Oracle Database 19c+ (or Oracle XE for development)
- Azure subscription (for production deployment)
- Visual Studio 2022, VS Code, or JetBrains Rider

### Configuration

Update `appsettings.json` with your Oracle connection strings:

```json
{
  "ConnectionStrings": {
    "OltpConnection": "User Id=ART_GALLERY_OLTP;Password=***;Data Source=your-host:1521/service;",
    "DwConnection": "User Id=ART_GALLERY_DW;Password=***;Data Source=your-host:1521/service;"
  }
}
```

### Running the API

1. **Navigate to the project:**
   ```powershell
   cd C:\_dev\dwbi_vue_backend\Artwork
   ```

2. **Restore packages:**
   ```powershell
   dotnet restore
   ```

3. **Run the API:**
   ```powershell
   dotnet run --project ArtGallery.API
   ```

4. **Access Swagger UI:**
   - URL: `https://localhost:7xxx/swagger`

## 📊 API Endpoints

### OLTP Operations

#### Artworks
- `GET /api/artworks` - Get all artworks (paginated)
- `GET /api/artworks/{id}` - Get artwork by ID
- `POST /api/artworks` - Create new artwork
- `PUT /api/artworks/{id}` - Update artwork
- `DELETE /api/artworks/{id}` - Delete artwork
- `GET /api/artworks/search?q={query}` - Search artworks
- `GET /api/artworks/statistics` - Get statistics

#### Exhibitions
- `GET /api/exhibitions` - Get all exhibitions
- `GET /api/exhibitions/{id}` - Get exhibition with artworks
- `POST /api/exhibitions` - Create exhibition
- `GET /api/exhibitions/upcoming` - Get upcoming exhibitions
- `GET /api/exhibitions/active` - Get active exhibitions
- `POST /api/exhibitions/{id}/artworks/{artworkId}` - Add artwork to exhibition

#### Visitors
- `GET /api/visitors` - Get all visitors
- `GET /api/visitors/members` - Get members only
- `GET /api/visitors/statistics` - Get visitor statistics

#### Staff
- `GET /api/staff` - Get all staff
- `GET /api/staff/by-department/{department}` - Get by department
- `GET /api/staff/statistics` - Get staff statistics

#### Loans
- `GET /api/loans` - Get all loans
- `GET /api/loans/active` - Get active loans
- `GET /api/loans/overdue` - Get overdue loans

#### Insurance
- `GET /api/insurance` - Get all policies
- `GET /api/insurance/active` - Get active policies
- `GET /api/insurance/expiring` - Get expiring policies

#### Restoration
- `GET /api/restoration` - Get all restoration records
- `GET /api/restoration/in-progress` - Get in-progress restorations

#### ETL Operations
- `GET /api/etl/syncs` - Get sync history
- `POST /api/etl/sync` - Trigger new sync
- `POST /api/etl/run-propagation` - Run OLTP to DW propagation (PL/SQL)
- `GET /api/etl/status` - Get current status
- `GET /api/etl/mappings` - Get field mappings
- `POST /api/etl/validate` - Validate data consistency
- `POST /api/etl/validate-integrity` - Validate DW referential integrity
- `POST /api/etl/explain-plan` - Get Oracle execution plan for query optimization

#### Reports (OLTP-based)
- `GET /api/reports/kpis` - Get KPI dashboard
- `GET /api/reports/dashboard` - Get complete dashboard data
- `GET /api/reports/visitor-trends` - Get visitor trends
- `GET /api/reports/artwork-distribution` - Get artwork distribution

### DW Analytics (Data Warehouse Queries)

#### Analytics Endpoints
- `GET /api/analytics/exhibition-summary` - Exhibition summary with revenue metrics
- `GET /api/analytics/artwork-inventory` - Artwork inventory with exhibition history
- `GET /api/analytics/insurance-analysis` - Insurance coverage analysis
- `GET /api/analytics/artist-performance` - Artist performance metrics
- `GET /api/analytics/visitor-trends` - Visitor trends over time
- `GET /api/analytics/revenue-breakdown` - Revenue breakdown by dimension
- `GET /api/analytics/dashboard` - KPI dashboard (DW-based)
- `GET /api/analytics/partition-stats` - DW partition statistics

### Health Check
- `GET /health` - Basic health check
- `GET /health/ready` - Readiness check (includes DB)

## 🔧 Configuration

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ArtGalleryDb;Trusted_Connection=true;"
  }
}
```

### CORS Configuration
The API is configured to allow requests from:
- `http://localhost:5173` (Vue.js default)
- `http://localhost:3000`

## 📦 NuGet Packages

- **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider
- **Oracle.EntityFrameworkCore** - Oracle Database provider
- **AutoMapper** - Object-object mapping
- **FluentValidation** - Request validation
- **Serilog.AspNetCore** - Structured logging
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI

## 🗄️ Database

The database is automatically created and seeded with sample data on first run:
- 8 Artworks (Monet, Van Gogh, Vermeer, etc.)
- 4 Exhibitions
- 5 Visitors
- 6 Staff members
- Sample loans, insurance policies, and restorations

## 📝 Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional message",
  "errors": [],
  "timestamp": "2026-01-17T19:30:00Z"
}
```

Paginated responses include:
```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## 🔐 Error Handling

The API uses global exception handling middleware that returns standardized error responses:

- **400** - Bad Request (validation errors)
- **404** - Not Found
- **409** - Conflict
- **500** - Internal Server Error

## 📚 For Vue.js Frontend Integration

Update your Vue.js API client:

```javascript
const apiClient = axios.create({
  baseURL: 'https://localhost:7xxx/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});
```

## 📄 License

This project is part of Master's degree coursework for Data Warehouse/BI and Backend Web Development.
