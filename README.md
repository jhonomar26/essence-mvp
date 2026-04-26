# EssenceMvp

Tool para evaluar salud de proyectos OMG Essence. Tracks 7 Alphas per proyecto con estado semáforo (verde/amarillo/rojo).

## Stack

| Capa | Tecnología |
|------|-----------|
| Presentación | ASP.NET Core 8 MVC + Views |
| Aplicación | Services (Clean Architecture) |
| Datos | PostgreSQL 16, EF Core 8, Npgsql |
| Autenticación | Cookies (8h expiry, sliding) |

## Estructura del Proyecto

```
EssenceMvp.Mvc/
├── Application/
│   └── Services/        # Lógica de negocio (Clean Arch)
│       ├── IAuthService, AuthService
│       ├── IProjectService, ProjectService
│       ├── IAlphaService, AlphaService
│       ├── IHealthService, HealthService
│       ├── ISnapshotService, SnapshotService
│       └── Evaluadores y calculadores
├── Infrastructure/
│   ├── Entities/        # EF Core entities (8 tablas)
│   └── EssenceDbContext.cs
├── Controllers/         # MVC controllers
│   ├── AccountController
│   ├── ProjectsController
│   ├── ProjectEvaluationController
│   ├── ProjectHealthController
│   └── ProjectSnapshotController
├── Models/              # View models
├── Views/               # Razor templates
├── wwwroot/             # Static assets (CSS, JS)
└── Program.cs           # DI + middleware setup
```

## Entidades Base

| Entity | Propósito |
|--------|-----------|
| `Alpha` | 7 alphas fijos (IDs 1–7), seeded |
| `AlphaState` | 41 estados, seeded |
| `StateChecklist` | 123 items, seeded |
| `AppUser` | Usuario (email, password hash, name) |
| `Project` | Proyecto (belongs_to AppUser) |
| `ProjectAlphaStatus` | Estado actual por alpha |
| `ChecklistResponse` | Respuestas de checklist |
| `HealthReport` | Snapshots con JSONB `alpha_details` |

## Configuración PostgreSQL

**Conexión (desarrollo):**
```
Host=localhost;Port=5432;Database=essence_mvp-<date>;Username=postgres;Password=123
```

**Notas importantes:**
- PostgreSQL enum `health_status` solo acepta lowercase: `'green'`, `'yellow'`, `'red'`
- Npgsql auto-traduce `HealthStatus.Green` → `'green'` (requiere `MapEnum` + `HasPostgresEnum`)
- `health_report.alpha_details` es JSONB, mapeado como `string?`
- Seed data en SQL source of truth (`db/essence_mvp_schema.sql`)
- Migrations auto-ejecutadas en startup para tablas transaccionales

## Puertos

| Servicio | HTTP | HTTPS |
|----------|------|-------|
| App | 5140 | 7197 |

## Levantar Localmente

```bash
# 1. PostgreSQL (asegúrate que existe la DB con schema)
psql -U postgres -d essence_mvp-<date> -f db/essence_mvp_schema.sql

# 2. Restaurar dependencias y correr
cd src/EssenceMvp.Mvc
dotnet run
```

Abre `https://localhost:7197`

## Autenticación

- **Esquema**: Cookie-based (ASP.NET Core Identity Alternative)
- **Expiración**: 8 horas con sliding window
- **Flujo**: Login → claim principal → cookie → autorización en controllers

## NuGet Packages

```
Microsoft.AspNetCore.Authentication.Cookies
Npgsql.EntityFrameworkCore.PostgreSQL
Microsoft.EntityFrameworkCore.Design
```

## Git Branching

| Rama | Propósito |
|------|-----------|
| `main` | Producción (merge vía PR) |
| `develop` | Integración (base para features) |
| `feature/*` | Nueva funcionalidad |
| `fix/*` | Bug fixes |
| `db/*` | Cambios schema/seed |
| `chore/*` | Config, refactor sin lógica |

## Convención de Commits

```
feat: descripción corta      ← nueva feature
fix: descripción corta       ← bug fix
db: descripción corta        ← schema/seed changes
chore: descripción corta     ← config, sin impacto
refactor: descripción corta  ← sin cambio de comportamiento
```

## Testing

Run:
```bash
dotnet test
```

Test coverage via `xunit` + `Moq` (en `tests/` si existen).

---

Made with ☕ for OMG Essence v1.2
