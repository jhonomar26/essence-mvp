# EssenceMvp

OMG Essence v1.2 project health assessment tool. Tracks 7 Alphas per project with traffic-light (verde/amarillo/rojo) status.

## Stack

| Layer | Tech |
|-------|------|
| Backend API | ASP.NET Core 8, Minimal API, no controllers |
| Frontend | Blazor Server (.NET 8), MudBlazor (Material Design) |
| Database | PostgreSQL 16, EF Core 8, Npgsql |
| Auth | JWT Bearer, PasswordHasher, 8h expiry |
| Infra | Docker Compose (Postgres only) |

## Project Structure

```
EssenceMvp/
├── src/
│   ├── EssenceMvp.Api/          # REST API
│   │   ├── Features/            # Vertical slice (endpoints + DTOs)
│   │   │   ├── AuthEndpoints.cs
│   │   │   ├── ProjectEndpoints.cs
│   │   │   ├── AuthContracts.cs
│   │   │   └── ProjectDtos.cs
│   │   ├── Infrastructure/
│   │   │   ├── Entities/        # 8 EF Core entity classes
│   │   │   └── EssenceDbContext.cs
│   │   └── Program.cs
│   └── EssenceMvp.Web/          # Blazor Server frontend
│       ├── Components/
│       │   ├── Pages/           # Home, Login, Register, Projects
│       │   ├── Layout/          # MainLayout (AppBar + nav)
│       │   └── AuthInitializer.razor
│       └── Services/            # ApiClient, AuthState, StorageService
├── db/
│   └── essence_mvp_schema.sql   # Source of truth for DB schema
├── docker-compose.yml
└── global.json                  # SDK lock: 8.0.420
```

## API Endpoints

```
POST /auth/register   { email, password, displayName? }
POST /auth/login      { email, password } → JWT token
GET  /auth/me         [auth] current user info
GET  /projects/mine   [auth] paginated project list
GET  /health          { status: "ok", alphas: 7 }
```

## Database

**Connection (dev):**
```
Host=localhost;Port=5432;Database=essence_mvp-18-04-2026;Username=postgres;Password=123
```

**Entities:**
- `Alpha` — 7 fixed alphas (IDs 1–7), seeded
- `AlphaState` — 41 states (IDs 1–41), seeded
- `StateChecklist` — 123 checklist items (IDs 1001–1123), seeded
- `AppUser` — email, password hash, display name
- `Project` — belongs to AppUser
- `ProjectAlphaStatus` — current state per alpha per project
- `ChecklistResponse` — checkbox answers per project
- `HealthReport` — snapshot with JSONB `alpha_details` column

**Key gotchas:**
- PostgreSQL enum `health_status` values are lowercase: `('green', 'yellow', 'red')`
- Npgsql snake_case translator auto-maps `HealthStatus.Green` → `'green'`
- Requires both: `NpgsqlDataSourceBuilder.MapEnum<HealthStatus>("health_status")` in Program.cs AND `modelBuilder.HasPostgresEnum<HealthStatus>("health_status")` in DbContext
- `health_report.alpha_details` is JSONB, mapped as `string?` in EF
- No EF migrations for catalog tables — schema SQL is source of truth
- `Database.MigrateAsync()` runs on startup for AppUser/Project/etc. tables

## Ports

| Service | HTTP | HTTPS |
|---------|------|-------|
| API | 5062 | 7153 |
| Web | 5140 | 7197 |
| Postgres (Docker) | 5432 | — |

## Running Locally

```bash
# Start Postgres
docker-compose up -d

# Run API
cd src/EssenceMvp.Api
dotnet run

# Run Web
cd src/EssenceMvp.Web
dotnet run
```

## Auth Flow (Web)

1. `AuthInitializer.razor` reads JWT from `localStorage` on load
2. `AuthState` (scoped service) holds current user + token
3. `StorageService` wraps JS interop for localStorage
4. Login/Register pages call `ApiClient` → store token → navigate to `/projects`
5. `MainLayout` shows logout button when authenticated

## NuGet Packages

**API:**
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.*
- `Npgsql.EntityFrameworkCore.PostgreSQL` 8.*
- `Microsoft.EntityFrameworkCore.Design` 8.*
- `Swashbuckle.AspNetCore` 6.6.2 (Swagger)

**Web:**
- `MudBlazor` 6.*

## Git Branching Strategy

### Ramas permanentes

| Rama | Propósito | Deploy |
|------|-----------|--------|
| `main` | Producción estable. Solo merge desde `develop` vía PR. | Prod |
| `develop` | Integración. Base para todas las features. | Staging |

### Ramas de trabajo (vida corta)

```
feature/<nombre>     Nueva funcionalidad
fix/<nombre>         Bug fix en develop
hotfix/<nombre>      Bug crítico en producción (sale de main)
db/<nombre>          Cambios de schema SQL o seed data
chore/<nombre>       Config, deps, refactor sin lógica de negocio
```

### Flujo normal

```
develop
  └── feature/health-dashboard    ← creas aquí
        ↓ PR → develop            ← merge cuando listo
              ↓ PR → main         ← release cuando develop estable
```

### Flujo hotfix (bug en prod)

```
main
  └── hotfix/fix-jwt-expiry       ← branch desde main
        ↓ PR → main               ← merge + tag
        ↓ PR → develop            ← backport obligatorio
```

### Convención de commits

```
feat: descripción corta        ← nueva feature
fix: descripción corta         ← bug fix
db: descripción corta          ← schema/seed changes
chore: descripción corta       ← sin impacto en runtime
refactor: descripción corta    ← sin cambio de comportamiento
```

### Ejemplos de nombres de rama por área

| Área | Ejemplo |
|------|---------|
| Auth | `feature/refresh-token` |
| Alphas / checklist | `feature/checklist-response-endpoint` |
| Health report | `feature/health-report-calculation` |
| UI | `feature/project-detail-page` |
| DB schema | `db/add-project-archived-column` |
| Docker / CI | `chore/docker-prod-image` |

### Reglas

- Nunca commitear directo a `main` o `develop`
- PRs a `develop` necesitan que el proyecto compile y `dotnet build` pase
- Tag en `main` después de cada release: `v0.1.0`, `v0.2.0`, etc.
- Borrar la rama feature después del merge

---

## Architecture Notes

- **Vertical slice**: feature code lives in `Features/`, not split by layer
- **No controllers**: all routes registered in `Program.cs` via extension methods
- **Pure domain**: `Infrastructure/Entities/` are EF entities, no separate domain layer yet
- **Spanish UI**: all user-facing text in Spanish
- **Blazor Server** (not WASM): interactive rendering, SignalR, full server-side state
