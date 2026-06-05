# AutoMarket API

**Course:** Service-Oriented Architectures — South East European University (SEEU)  
**Type:** Final project  
**Stack:** .NET 8 · ASP.NET Core Web API · SQL Server · Entity Framework Core 8 · JWT Bearer · AutoMapper · Serilog · xUnit + Moq

A RESTful Web API for a used-car marketplace with integrated vehicle service-history tracking.
Four user roles — **Admin**, **Seller**, **Buyer**, and **Mechanic** — each with distinct permissions.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0.x |
| SQL Server **or** SQL Server LocalDB | any recent |
| `dotnet-ef` global tool | 8.0.x |

Install the EF global tool once (skip if already installed):

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

---

## Solution Structure

```
AutoMarket/
├── AutoMarket.API/           ← Entry point: Controllers, Middleware, Program.cs
├── AutoMarket.Core/          ← Domain: Entities, DTOs, Interfaces, Enums, Helpers
├── AutoMarket.Infrastructure/← Data: EF DbContext, Configurations, Repositories
├── AutoMarket.Services/      ← Logic: Services, AutoMapper profile, NhtsaService
└── AutoMarket.Tests/         ← xUnit unit tests (59 tests)
```

---

## Configuration

### 1. Connection string

Open `AutoMarket.API/appsettings.json` and update `ConnectionStrings:DefaultConnection`
to point at your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AutoMarketDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Common alternatives:

| Scenario | Value |
|----------|-------|
| LocalDB (default) | `Server=(localdb)\\mssqllocaldb;Database=AutoMarketDb;Trusted_Connection=True;` |
| SQL Server Express | `Server=.\\SQLEXPRESS;Database=AutoMarketDb;Trusted_Connection=True;` |
| Full SQL Server | `Server=localhost;Database=AutoMarketDb;User Id=sa;Password=yourpw;` |

### 2. JWT key

The key in `appsettings.json` is a development placeholder.
Replace it with any random string of **32 or more characters** before deploying:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyHereMakeItAtLeast32CharactersLong!",
  "Issuer": "AutoMarket",
  "Audience": "AutoMarketUsers",
  "ExpiresInMinutes": 60
}
```

---

## Database Setup — EF Core Migrations

Run both commands from the **solution root** (`AutoMarket/`):

```bash
# 1. Create the initial migration (generates SQL from your entity model)
dotnet ef migrations add InitialCreate --project AutoMarket.Infrastructure --startup-project AutoMarket.API

# 2. Apply the migration — creates the AutoMarketDb database and all tables
dotnet ef database update --project AutoMarket.Infrastructure --startup-project AutoMarket.API
```

To reset the database and start fresh:

```bash
dotnet ef database drop --project AutoMarket.Infrastructure --startup-project AutoMarket.API
dotnet ef database update --project AutoMarket.Infrastructure --startup-project AutoMarket.API
```

---

## Running the Project

```bash
# From the solution root
dotnet run --project AutoMarket.API
```

The API starts on `https://localhost:7xxx` / `http://localhost:5xxx` (exact ports are printed at startup).

**Swagger UI** is available at `https://localhost:<port>/swagger` in the Development environment.
Use the **Authorize** button to enter a Bearer token obtained from `POST /api/auth/login`.

Logs are written to the console and to `AutoMarket.API/logs/automarket-YYYYMMDD.log` (rolling daily, 30-day retention).

---

## Running the Tests

```bash
# From the solution root — builds and runs all 59 unit tests
dotnet test AutoMarket.Tests

# Verbose output (shows each test name and pass/fail)
dotnet test AutoMarket.Tests --verbosity normal
```

All tests run fully in-memory using Moq. No database or network connection is required.

---

## API Endpoint Reference

All endpoints return JSON. Enum values are serialized as strings.  
Protected endpoints require the header: `Authorization: Bearer <token>`

### Authentication — `/api/auth`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | Public | Register a new account. Body: `{ username, email, password, role }` |
| POST | `/api/auth/login` | Public | Authenticate. Returns JWT token. Body: `{ email, password }` |
| GET | `/api/auth/me` | Any role | Get own profile |
| PUT | `/api/auth/me` | Any role | Update own username / email |
| GET | `/api/auth/users` | Admin | Paginated list of all users |
| DELETE | `/api/auth/users/{id}` | Admin | Delete a user |

### Listings — `/api/listings`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/listings` | Public | Paginated, filterable list (make, model, year, price, fuel, status) |
| GET | `/api/listings/{id}` | Public | Single listing with vehicle + images + seller |
| GET | `/api/listings/my` | Seller | Own listings, paginated |
| POST | `/api/listings` | Seller | Create a listing |
| PUT | `/api/listings/{id}` | Seller (owner) / Admin | Update a listing |
| DELETE | `/api/listings/{id}` | Seller (owner) / Admin | Mark listing as Inactive |
| POST | `/api/listings/{id}/images` | Seller (owner) | Add an image URL to a listing |

### Vehicles — `/api/vehicles`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/vehicles` | Admin | Paginated list of all vehicles |
| GET | `/api/vehicles/{id}` | Any role | Get vehicle by id |
| POST | `/api/vehicles` | Seller | Create a vehicle record manually |
| POST | `/api/vehicles/decode-vin` | Seller | Decode a VIN via NHTSA (no DB write). Body: `{ vin }` |
| PUT | `/api/vehicles/{id}` | Seller (owner via listing) / Admin | Update vehicle details |
| GET | `/api/vehicles/{id}/service-history` | Public | All service records for a vehicle |

### Service History — `/api/service-history`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/service-history` | Mechanic | Own logged service records |
| GET | `/api/service-history/vehicle/{vehicleId}` | Any role | All records for a vehicle |
| GET | `/api/service-history/{id}` | Any role | Single service record |
| POST | `/api/service-history` | Mechanic | Log a new service record |
| PUT | `/api/service-history/{id}` | Mechanic (owner) / Admin | Update a service record |
| DELETE | `/api/service-history/{id}` | Admin | Hard-delete a service record |

### Watchlist — `/api/watchlist`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/watchlist` | Buyer | All watchlist items |
| POST | `/api/watchlist` | Buyer | Add listing to watchlist. Body: `{ listingId }` |
| DELETE | `/api/watchlist/{listingId}` | Buyer | Remove a listing from watchlist |
| GET | `/api/watchlist/check/{listingId}` | Buyer | Returns `true`/`false` — is listing in watchlist? |
| DELETE | `/api/watchlist/clear` | Buyer | Remove all watchlist items |

### Enquiries — `/api/enquiries`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/enquiries` | Any role | Own enquiries (sent if Buyer, received if Seller) |
| GET | `/api/enquiries/{id}` | Participant only | Single enquiry |
| POST | `/api/enquiries` | Buyer | Send an enquiry about a listing |
| PUT | `/api/enquiries/{id}/reply` | Seller (receiver) | Add a reply |
| PUT | `/api/enquiries/{id}/status` | Seller (receiver) | Update enquiry status (Open / Replied / Closed) |
| DELETE | `/api/enquiries/{id}` | Admin | Delete an enquiry |

### Reviews — `/api/reviews`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/reviews/listing/{listingId}` | Public | All reviews for a listing + average rating |
| GET | `/api/reviews/{id}` | Public | Single review |
| GET | `/api/reviews/my` | Buyer | Own reviews |
| POST | `/api/reviews` | Buyer | Submit a review (one per listing per buyer) |
| PUT | `/api/reviews/{id}` | Buyer (owner) | Update own review |
| DELETE | `/api/reviews/{id}` | Buyer (owner) / Admin | Delete a review |

---

## User Roles

| Role | Numeric value | Typical capabilities |
|------|--------------|----------------------|
| Admin | 0 | Full access, user management, hard deletes |
| Seller | 1 | Create/manage listings and vehicles |
| Buyer | 2 | Browse listings, watchlist, enquiries, reviews |
| Mechanic | 3 | Log and manage service records |

Specify the role in the `role` field when registering. Valid string values: `Admin`, `Seller`, `Buyer`, `Mechanic`.

---

## Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (delete succeeded) |
| 400 | Bad Request / validation error |
| 401 | Unauthenticated (missing or invalid JWT) |
| 403 | Forbidden (authenticated but wrong role or not the owner) |
| 404 | Resource not found |
| 409 | Conflict (duplicate watchlist entry or duplicate review) |
| 500 | Unexpected server error |

---

## External API

Vehicle VIN decoding uses the **NHTSA vPIC API** — no API key required.

`GET https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVin/{vin}?format=json`

Call `POST /api/vehicles/decode-vin` with `{ "vin": "..." }` to invoke it from this API.
The result is returned directly and is **never persisted** to the database.
