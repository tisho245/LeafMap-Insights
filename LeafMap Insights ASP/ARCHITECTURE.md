# LeafMap Insights - Architecture Documentation

## Overview

LeafMap Insights is a distributed ASP.NET Core MVC application designed for managing and visualizing tree taxonomy data with geographic locations. The application follows clean MVC architecture principles and uses Entity Framework Core for data access, ASP.NET Identity for authentication and authorization, and includes QR code generation for tree identification.

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB for development)
- **Authentication**: ASP.NET Identity with Role-Based Access Control (RBAC)
- **QR Code Generation**: QRCoder library
- **Frontend**: Razor Views with Bootstrap 5
- **Mapping**: Leaflet.js (OpenStreetMap)

## Architecture Overview

### Layer Structure

```
┌─────────────────────────────────────────┐
│          Presentation Layer             │
│  (Controllers, Views, ViewModels)       │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│          Business Logic Layer           │
│     (Services, Application Logic)       │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│          Data Access Layer              │
│  (DbContext, Entity Models, EF Core)    │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│          Infrastructure Layer           │
│  (Identity, Authentication, Services)   │
└─────────────────────────────────────────┘
```

## Database Schema

### Entity Relationships

```
Family (1) ──────< (Many) Genus
                         │
                         │ (1)
                         │
                         │
                    (Many) Species
                         │
                         │ (1)
                         │
                    (Many) Tree
```

### Entity Models

#### 1. Family
- **Purpose**: Represents the botanical family classification
- **Properties**: Id, Name, Description
- **Relationships**: One-to-Many with Genus

#### 2. Genus
- **Purpose**: Represents the genus classification within a family
- **Properties**: Id, Name, LatinName, Description, FamilyId
- **Relationships**: Many-to-One with Family, One-to-Many with Species

#### 3. Species
- **Purpose**: Represents the species classification within a genus
- **Properties**: Id, Name, LatinName, Description, GenusId
- **Relationships**: Many-to-One with Genus, One-to-Many with Tree

#### 4. Tree
- **Purpose**: Represents individual tree instances with geographic data
- **Properties**: 
  - Id (Primary Key)
  - SpeciesId (Foreign Key)
  - Latitude, Longitude (Geographic coordinates)
  - EstimatedAge, Height, Condition
  - QRCodeValue (Base64 encoded QR code image)
  - CreatedAt, UpdatedAt (Audit fields)
- **Relationships**: Many-to-One with Species

## Controllers

### 1. TreesController (Public)
- **Purpose**: Public-facing tree browsing and viewing
- **Actions**:
  - `Index()`: List all trees
  - `Details(int id)`: View tree details with QR code
  - `Map()`: Interactive map visualization
  - `QRCode(int id)`: Download QR code as PNG

### 2. AdminController (Admin Only)
- **Purpose**: Administrative CRUD operations
- **Authorization**: `[Authorize(Roles = "Admin")]`
- **Actions**:
  - **Tree Management**: Trees, CreateTree, EditTree, DeleteTree
  - **Taxonomy Management**: 
    - Families (CRUD)
    - Genera (CRUD)
    - Species (CRUD)
  - **User Management**: Users (placeholder for future expansion)

### 3. API Controllers (For .NET MAUI Mobile App)

#### TreesApiController
- **Route**: `/api/TreesApi`
- **Endpoints**:
  - `GET /api/TreesApi`: Get all trees with full taxonomy
  - `GET /api/TreesApi/{id}`: Get specific tree
  - `GET /api/TreesApi/WithinBounds?minLat&minLng&maxLat&maxLng`: Get trees within geographic bounds

#### TaxonomyApiController
- **Route**: `/api/TaxonomyApi`
- **Endpoints**:
  - `GET /api/TaxonomyApi/Families`: Get all families
  - `GET /api/TaxonomyApi/Genera?familyId={id}`: Get genera (optionally filtered by family)
  - `GET /api/TaxonomyApi/Species?genusId={id}`: Get species (optionally filtered by genus)

## Services

### QRCodeService
- **Interface**: `IQRCodeService`
- **Purpose**: Generate QR codes for tree identification
- **Methods**:
  - `GenerateQRCode(string url)`: Returns Base64 encoded PNG string
  - `GenerateQRCodeBytes(string url)`: Returns byte array
- **QR Code Content**: Links to `/Trees/Details/{id}`

## Identity & Security

### Roles
- **Admin**: Full access to all CRUD operations and admin panel
- **User**: Read-only access to public tree information

### Default Admin Account
- **Email**: admin@leafmap.com
- **Password**: Admin@123
- **Note**: Change password in production!

### Configuration
- Password requirements: Minimum 6 characters, requires digit, lowercase, uppercase
- Email confirmation: Disabled for easier development (enable in production)
- Roles are automatically seeded on application startup

## Views

### Public Views (Trees)
- **Index.cshtml**: Table listing of all trees
- **Details.cshtml**: Detailed tree information with QR code display
- **Map.cshtml**: Interactive Leaflet.js map with tree markers

### Admin Views
- **Dashboard**: Admin panel home with navigation
- **Tree Management**: List, Create, Edit, Delete views
- **Taxonomy Management**: Separate CRUD views for Family, Genus, Species

## Key Features

### 1. QR Code Generation
- Automatically generated when trees are created
- Links directly to tree details page
- Can be downloaded as PNG file
- Stored as Base64 string in database

### 2. Map Visualization
- Interactive map using Leaflet.js
- Shows all trees as markers
- Popups display tree information
- Links to detailed view
- Auto-fits bounds to show all trees

### 3. Taxonomy Hierarchy
- Three-level taxonomy: Family → Genus → Species
- Cascading dropdowns in admin forms
- Full taxonomic information in tree views

### 4. Geographic Data
- Latitude/Longitude stored as decimal(10,8) and decimal(11,8)
- Support for precise GPS coordinates
- Map-based visualization

## API Design for Mobile App

### RESTful Endpoints
All API endpoints return JSON and follow RESTful conventions:

- **Trees**: `/api/TreesApi`
- **Taxonomy**: `/api/TaxonomyApi`

### Response Format
```json
{
  "id": 1,
  "latitude": 51.505,
  "longitude": -0.09,
  "estimatedAge": 50,
  "height": 15.5,
  "condition": "Healthy",
  "species": {
    "id": 1,
    "name": "Oak",
    "latinName": "Quercus robur",
    "genus": {
      "latinName": "Quercus",
      "family": {
        "name": "Fagaceae"
      }
    }
  }
}
```

## Database Configuration

### Connection String
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-LeafMap_Insights-..."
  }
}
```

### Migrations
1. Initial migration creates Identity tables
2. Add migration for taxonomy and tree entities:
   ```bash
   dotnet ef migrations add AddTaxonomyAndTrees
   dotnet ef database update
   ```

## Best Practices Implemented

1. **Separation of Concerns**: Clear separation between controllers, services, and data access
2. **Dependency Injection**: All services registered and injected via DI container
3. **Repository Pattern**: Entity Framework DbContext acts as repository
4. **ViewModels**: Separate view models for complex forms
5. **Authorization**: Role-based access control with `[Authorize]` attributes
6. **Validation**: Data annotations on models and view models
7. **Error Handling**: Try-catch blocks in data seeding
8. **Audit Fields**: CreatedAt/UpdatedAt timestamps
9. **Soft Deletes**: Can be implemented by adding IsDeleted flag
10. **API Versioning**: Can be added using route versioning

## Future Enhancements

1. **User Management UI**: Complete user management interface in admin panel
2. **Image Upload**: Support for tree photos
3. **Search & Filtering**: Advanced search and filter capabilities
4. **Export**: Export trees to CSV/Excel
5. **Reporting**: Statistical reports and analytics
6. **Caching**: Implement caching for frequently accessed data
7. **Logging**: Structured logging with Serilog
8. **Unit Tests**: Comprehensive unit and integration tests
9. **API Authentication**: JWT tokens for mobile app API access
10. **Geographic Queries**: Spatial queries using SQL Server geography types

## Project Structure

```
LeafMap Insights/
├── Controllers/
│   ├── TreesController.cs
│   ├── AdminController.cs
│   └── Api/
│       ├── TreesApiController.cs
│       └── TaxonomyApiController.cs
├── Models/
│   ├── Family.cs
│   ├── Genus.cs
│   ├── Species.cs
│   ├── Tree.cs
│   └── ViewModels/
│       └── TreeViewModel.cs
├── Services/
│   ├── IQRCodeService.cs
│   └── QRCodeService.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Views/
│   ├── Trees/
│   ├── Admin/
│   └── Shared/
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/
```

## Getting Started

1. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

2. **Update database**:
   ```bash
   dotnet ef database update
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access the application**:
   - Public: Navigate to Trees or Map
   - Admin: Login with admin@leafmap.com / Admin@123

## Security Considerations

1. **Change default admin password** in production
2. **Enable HTTPS** in production
3. **Implement API authentication** for mobile endpoints
4. **Add rate limiting** for API endpoints
5. **Validate all user inputs** (already implemented with Data Annotations)
6. **Use parameterized queries** (handled by EF Core)
7. **Implement CORS policies** if needed for mobile app
8. **Regular security updates** for dependencies

---

**Project Number**: 765  
**Application Name**: LeafMap Insights  
**Version**: 1.0.0

