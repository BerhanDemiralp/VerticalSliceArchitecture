# Feature Flags system with Vertical Slice Architecture

An example project built on **.NET Minimal API** using **Vertical Slice Architecture (VSA)**.
The project demonstrates how to structure endpoints, caching, and persistence, while also serving as a **template for building a Feature Flag system**.

Feature flags are used here to dynamically enable/disable application features without redeployment. The system integrates **Redis** for fast lookups and **SQLite** for persistence, packaged and orchestrated with **Docker Compose**.

---

## Features

* **Vertical Slice Architecture**: Each feature (slice) has its own endpoints, request/response models, and domain logic.
* **Feature Flag System**:

  * Store feature flags in the database (SQLite).
  * Cache them in Redis for fast access.
  * Provide endpoints to add, remove, and check flags.
  * Use flags to conditionally enable/disable functionality at runtime.
* **.NET Minimal API**: Lightweight endpoint definitions, easy to extend.
* **EF Core + SQLite**: Simple relational database with migrations.
* **Redis Cache**: Used for feature flags and product caching.
* **Docker Compose Profiles**:

  * `dev`: development profile with hot reload.
  * `prod`: production profile with clean runtime.
* **Persistent Data**: SQLite database stored via Docker volumes.

---

## Architecture Overview

```
/Domain                 # Domain models (Product, Category, FeatureFlag)
/Features               # Vertical slices: endpoints, requests, responses, handlers
/Infrastructure         # AppDbContext, cache services, Redis integration
/Migrations             # EF Core migrations
/Services               # Application services (e.g. cache, flag service)
/Frontend               # UI integration
Program.cs              # Minimal API, DI, middleware
Dockerfile              # App image definition
docker-compose.yml      # Dev/prod profiles, Redis, SQLite persistence, frontend 
```

---

## Technology Stack

* **.NET 9.0** (Minimal API)
* **Entity Framework Core** (SQLite provider)
* **Redis** (cache layer)
* **Docker & Docker Compose**

## Running the Project

### Requirements

* Docker & Docker Compose
* .NET SDK 9.0

### Development

```bash
docker compose --profile dev up --build
```

* Runs the API on `http://localhost:5000`
* SQLite persisted via volume at `./dockerdata/sqlite`
* Redis available at `redis:6379`
* Frontend server running on `http://localhost:8000`

### Production

```bash
docker compose --profile prod up --build -d
```

## Endpoints Overview

### Products

* `GET /api/products` — List products
* `POST /api/products` — Create product
* `POST /api/products/v2` — Create product V2
* `PUT /api/products/{id}` — Update product
* `PUT /api/products/v2/{id}` — Update product V2
* `DELETE /api/products/{id}` — Delete product

### Feature Flags

* `GET /api/flags` — Check all feature flags
* `PUT /api/flags/{name}` — Update the existing new flag
