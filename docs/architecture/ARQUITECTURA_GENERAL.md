# Arquitectura General de EssenceMvp

## 🎯 Visión de Alto Nivel

**EssenceMvp** es una herramienta para evaluar la **salud de proyectos OMG Essence** (7 Alphas, checklist de estados). 

La aplicación sigue una **arquitectura en dos dominios**:

- **Backend**: API REST construida en ASP.NET Core 8 con **Clean Architecture** (4 capas)
- **Frontend**: SPA React 18 + TypeScript con arquitectura **feature-based**

Ambos se comunican vía REST + CORS, con **cookie-based authentication**.

```
┌─────────────────────────────────────┐
│   NAVEGADOR / CLIENTE                │
│   (React SPA en localhost:5173)      │
└──────────────┬──────────────────────┘
               │
            CORS ✓
         (withCredentials)
               │
┌──────────────▼──────────────────────┐
│  BACKEND ASP.NET CORE (API-only)    │
│  localhost:5070 / 7170 (HTTPS)      │
│                                       │
│  ┌─────────────────────────────────┐ │
│  │ Presentación (Controllers)       │ │
│  │ - ProjectHealthController        │ │
│  │ - ProjectEvaluationController    │ │
│  │ - ProjectSnapshotController      │ │
│  └─────────────────────────────────┘ │
│                                       │
│  ┌─────────────────────────────────┐ │
│  │ Aplicación (Business Services)  │ │
│  │ - ProjectService                │ │
│  │ - HealthCalculationService      │ │
│  │ - AlphaEvaluationService        │ │
│  └─────────────────────────────────┘ │
│                                       │
│  ┌─────────────────────────────────┐ │
│  │ Dominio (Essence Logic)         │ │
│  │ - Evaluación de Alphas          │ │
│  │ - Cálculo de salud              │ │
│  │ - Reglas SEMAT Essence          │ │
│  └─────────────────────────────────┘ │
│                                       │
│  ┌─────────────────────────────────┐ │
│  │ Infraestructura (EF Core)       │ │
│  │ - EssenceDbContext              │ │
│  │ - Entities, Migrations          │ │
│  └─────────────────────────────────┘ │
└──────────────┬──────────────────────┘
               │
    PostgreSQL 16
               │
        Base de datos
```

---

## 📦 Backend: Clean Architecture (4 Capas)

### 1️⃣ **Capa de Presentación** 
**Ubicación**: `src/EssenceMvp.Mvc/Controllers/`

**Responsabilidad**: Recibir petición HTTP → mapear a dominios → delegar a servicios → retornar respuesta JSON.

**Controllers API (solo 3)**:

| Controller | Endpoint | Método | Autenticación | Propósito |
|---|---|---|---|---|
| `ProjectHealthController` | `/evaluation/health/{projectId}` | GET | ❌ No | Calcula salud SEMAT del proyecto |
| `ProjectEvaluationController` | `/evaluation/alpha` | POST | ✅ Cookie | Evalúa respuestas de checklist |
| `ProjectSnapshotController` | `/snapshots` | GET/POST | ✅ Cookie | Historial de evaluaciones |

**Ejemplo real**:
```csharp
[Route("evaluation/health")]
public class ProjectHealthController : Controller
{
    private readonly IHealthCalculationService _healthCalcService;

    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project == null) return NotFound();

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);
        
        return Json(new HealthResult
        {
            HealthScore = healthScore.HealthScore,
            Classification = healthScore.Classification,
            AlphaDetails = healthScore.AlphaProgresses
        });
    }
}
```

---

### 2️⃣ **Capa de Aplicación**
**Ubicación**: `src/EssenceMvp.Application/Services/`

**Responsabilidad**: Orquestación de lógica, coordinación entre capas.

**Servicios principales**:

| Servicio | Interfaz | Propósito |
|---|---|---|
| `ProjectService` | `IProjectService` | CRUD de proyectos, acceso DB |
| `HealthCalculationService` | `IHealthCalculationService` | Cálculo agregado de salud |
| `AlphaEvaluationService` | `IAlphaEvaluationService` | **Evaluación de estados SEMAT** |
| `SnapshotService` | `ISnapshotService` | Gestión de snapshots históricos |
| `AuthService` | `IAuthService` | Autenticación de usuarios |

**Patrón Interfaces-First**:
```csharp
// Interfaz (contrato)
public interface IHealthCalculationService
{
    Task<HealthScore> CalculateProjectHealthAsync(int projectId);
}

// Implementación (puede cambiar sin afectar a quien la usa)
public class HealthCalculationService : IHealthCalculationService
{
    private readonly IProjectRepository _projects;
    private readonly IAlphaEvaluationService _alphaEval;

    public async Task<HealthScore> CalculateProjectHealthAsync(int projectId)
    {
        var alphaProgresses = new List<AlphaProgress>();
        
        // Calcula progreso para cada uno de los 7 alphas
        for (int i = 1; i <= 7; i++)
        {
            var result = await _alphaEval.CalculateAsync(projectId, i);
            alphaProgresses.Add(new AlphaProgress
            {
                AlphaId = i,
                CurrentStateNumber = result.CurrentStateNumber,
                MaxStateNumber = 6, // SEMAT Essence estándar
                Progress = (result.CurrentStateNumber * 100) / 6
            });
        }

        return new HealthScore
        {
            HealthScore = (int)alphaProgresses.Average(x => x.Progress),
            AlphaProgresses = alphaProgresses
        };
    }
}
```

---

### 3️⃣ **Capa de Dominio**
**Ubicación**: Embebida en `Application/Services/` (específicamente `AlphaEvaluationService`)

**Responsabilidad**: Reglas de negocio SEMAT Essence, sin conocer HTTP ni DB.

**Lógica de evaluación de Alpha** (el corazón de la app):

```csharp
public class AlphaEvaluationService : IAlphaEvaluationService
{
    public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
    {
        // REGLA SEMAT: Un Alpha está en Estado N si y solo si
        // TODOS los checklists de estados 1..N están completados.
        
        var alpha = await _db.Alphas
            .Include(a => a.States.OrderBy(s => s.StateNumber))
            .ThenInclude(s => s.Checklists)
            .FirstOrDefaultAsync(a => a.Id == alphaId);

        short currentState = 0;

        foreach (var state in alpha.States)
        {
            // Para cada estado, obtén sus checklists
            var checklistIds = state.Checklists.Select(c => c.Id).ToList();
            var responses = await _db.ChecklistResponses
                .Where(r => r.ProjectId == projectId && checklistIds.Contains(r.StateChecklistId))
                .ToListAsync();

            // ¿Todos los checklists completados?
            bool stateComplete = state.Checklists.All(c =>
                responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved)
            );

            if (!stateComplete) break; // Detén en primer incompleto
            currentState = state.StateNumber;
        }

        return new AlphaStateResult
        {
            AlphaId = alphaId,
            CurrentStateNumber = currentState,
            MaxStateNumber = 6,
            Status = currentState switch
            {
                0 => HealthStatus.Red,
                1 or 2 => HealthStatus.Red,
                3 => HealthStatus.Yellow,
                4 or 5 => HealthStatus.Yellow,
                6 => HealthStatus.Green,
                _ => HealthStatus.Red
            }
        };
    }
}
```

**Características**:
- ✅ No depende de ASP.NET, HTTP, ni controladores
- ✅ Puede testearse sin base de datos (mockeando repositories)
- ✅ Cambios en reglas SEMAT → solo toca este servicio

---

### 4️⃣ **Capa de Infraestructura**
**Ubicación**: `src/EssenceMvp.Infrastructure/`

**Responsabilidad**: Acceso a datos, persistencia, abstracciones de frameworks.

**Componentes**:

| Componente | Propósito |
|---|---|
| `EssenceDbContext` | DbContext de Entity Framework |
| `Entities/` | Mapeos C# → Tablas PostgreSQL |
| `Repositories/` | Acceso a datos (si aplica) |
| `Migrations/` | Versionado de schema |

**Entities principales**:

```
AppUser
  ├── Projects (1:N)
  │    ├── ProjectAlphaStatus (1:N)
  │    ├── ChecklistResponses (1:N)
  │    └── HealthReports (1:N)
  │
Alpha (7 fijos, seeded)
  ├── AlphaStates (6 por alpha)
  │    └── StateChecklists (múltiples)
  │
ChecklistResponse
  └── Mapeo: ProjectId + StateChecklistId + IsAchieved
```

**Configuración PostgreSQL**:
```csharp
public class EssenceDbContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Alpha> Alphas { get; set; }
    public DbSet<AlphaState> AlphaStates { get; set; }
    public DbSet<StateChecklist> StateChecklists { get; set; }
    public DbSet<ChecklistResponse> ChecklistResponses { get; set; }
    public DbSet<ProjectAlphaStatus> ProjectAlphaStatuses { get; set; }
    public DbSet<HealthReport> HealthReports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Mapeos, seeding, enums, etc.
        builder.Entity<AppUser>().HasMany(u => u.Projects)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        // Seed data para los 7 alphas + estados + checklists
        SeedData(builder);
    }
}
```

---

## 🎨 Frontend: React Feature-Based

**Ubicación**: `frontend/`

**Stack**:
- React 18 + TypeScript
- Vite (build/dev server)
- TanStack Query v5 (server state)
- Zustand (client state)
- Axios (HTTP client con interceptors)
- react-router-dom v6 (routing)
- Tailwind CSS v4 (styling)

### Estructura de Carpetas

```
frontend/src/
│
├── app/                           ← Arranque de la app
│   ├── providers/AppProviders.tsx (QueryClientProvider, RouterProvider)
│   ├── router/
│   │   ├── router.tsx             (definición de rutas)
│   │   └── paths.ts               (constantes de paths)
│   └── layout/
│       ├── MainLayout.tsx         (para rutas autenticadas)
│       └── AuthLayout.tsx         (para login/register)
│
├── features/                      ← Dominio por feature
│   ├── auth/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── services/
│   │   ├── types/
│   │   └── index.ts
│   │
│   ├── projects/
│   │   ├── components/ProjectList.tsx
│   │   ├── components/ProjectCard.tsx
│   │   ├── services/projectApi.ts
│   │   ├── types/project.ts
│   │   └── hooks/useProjects.ts
│   │
│   ├── health/
│   │   ├── components/HealthDashboard.tsx
│   │   ├── services/healthApi.ts  (← WIRED: GET /evaluation/health/{id})
│   │   ├── types/health.ts
│   │   └── hooks/useProjectHealth.ts
│   │
│   ├── evaluation/
│   │   ├── components/EvaluationForm.tsx
│   │   ├── services/evaluationApi.ts
│   │   ├── types/evaluation.ts
│   │   └── hooks/useEvaluation.ts
│   │
│   └── snapshots/
│       ├── components/SnapshotList.tsx
│       ├── services/snapshotApi.ts
│       └── types/snapshot.ts
│
├── shared/                        ← Código reutilizable
│   ├── api/
│   │   └── client.ts              (Axios instance configurada)
│   ├── components/
│   │   ├── ui/                    (Buttons, Forms, Modals, etc.)
│   │   └── feedback/              (Loading, Error, Empty states)
│   ├── lib/
│   │   └── queryClient.ts         (TanStack Query config)
│   ├── hooks/                     (useApi, useFetch custom)
│   └── types/
│       └── api.ts                 (tipos compartidos)
│
├── stores/                        ← Estado global (Zustand)
│   ├── useAuthStore.ts            (auth state, logout, etc.)
│   └── useUIStore.ts              (theme, sidebar collapsed, etc.)
│
├── App.tsx                        ← Componente raíz
├── main.tsx                       ← Entry point
└── index.css                      (@import "tailwindcss";)
```

### Client HTTP (Axios)

```typescript
// shared/api/client.ts
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5070',
  withCredentials: true, // ← Crucial para cookies de auth
});

// Interceptor para manejar 302/401 de login
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401 || error.response?.status === 302) {
      // Redirige a login o limpia auth
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Ejemplo: Hook de Datos (TanStack Query)

```typescript
// features/health/hooks/useProjectHealth.ts
import { useQuery } from '@tanstack/react-query';
import { getProjectHealth } from '../services/healthApi';

export function useProjectHealth(projectId: number) {
  return useQuery({
    queryKey: ['health', projectId],
    queryFn: () => getProjectHealth(projectId),
    staleTime: 5 * 60 * 1000, // 5 min cache
  });
}

// features/health/services/healthApi.ts
import apiClient from '../../../shared/api/client';
import type { HealthResult } from '../types/health';

export async function getProjectHealth(projectId: number): Promise<HealthResult> {
  const response = await apiClient.get<HealthResult>(
    `/evaluation/health/${projectId}`
  );
  return response.data;
}
```

### Componente que lo usa

```typescript
// features/health/components/HealthDashboard.tsx
import { useProjectHealth } from '../hooks/useProjectHealth';

export function HealthDashboard({ projectId }: { projectId: number }) {
  const { data: health, isLoading, error } = useProjectHealth(projectId);

  if (isLoading) return <div>Cargando...</div>;
  if (error) return <div>Error: {error.message}</div>;
  if (!health) return <div>Sin datos</div>;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold">Salud del Proyecto</h2>
      <div className="grid grid-cols-7 gap-4 mt-4">
        {health.alphaDetails.map(alpha => (
          <div key={alpha.alphaId} className="p-4 border rounded">
            <div className="font-semibold">{alpha.alphaName}</div>
            <div className="text-sm text-gray-600">
              {alpha.currentStateNumber}/{alpha.maxStateNumber}
            </div>
            <div className="w-full bg-gray-200 rounded mt-2 h-2">
              <div
                className="bg-green-500 h-2 rounded"
                style={{ width: `${alpha.progress}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## 🔄 Flujo End-to-End: Desde React hasta PostgreSQL

### Caso de uso: Usuario abre dashboard de salud

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. USUARIO en React (Frontend)                                   │
│    • Navega a /projects/123/health                               │
│    • Se renderiza <HealthDashboard projectId={123} />           │
└──────────────────────┬──────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 2. Hook React: useProjectHealth(123)                             │
│    • TanStack Query ejecuta queryFn                              │
│    • Llama: getProjectHealth(123)                                │
└──────────────────────┬──────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 3. Axios Client Request                                          │
│    • GET http://localhost:5070/evaluation/health/123             │
│    • Headers: { withCredentials: true, ... }                     │
│    • Incluye cookie de sesión del usuario                        │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                  CORS Check
         ✓ Origin: http://localhost:5173
         ✓ Credenciales permitidas
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 4. ASP.NET Core - Middleware                                     │
│    • CORS Middleware: valida origen (OK)                         │
│    • Auth Middleware: extrae cookie → claimsPrincipal            │
│    • Routing: mapea a ProjectHealthController.Get(123)           │
└──────────────────────┬──────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 5. Presentación - ProjectHealthController                        │
│    public async Task<IActionResult> Get(int projectId)           │
│    {                                                              │
│        var project = await _projects.GetByIdAsync(projectId);   │
│        var healthScore = await _healthCalcService               │
│            .CalculateProjectHealthAsync(projectId);              │
│        return Json(healthScore);                                 │
│    }                                                              │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 6. Aplicación - HealthCalculationService                         │
│    public async Task<HealthScore> CalculateProjectHealthAsync   │
│    {                                                              │
│        for (int i = 1; i <= 7; i++)                             │
│        {                                                          │
│            var result = await _alphaEval.CalculateAsync(pId, i);│
│            // Construye resumen de cada alpha                    │
│        }                                                          │
│        return new HealthScore { ... };                           │
│    }                                                              │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 7. Dominio - AlphaEvaluationService (LÓGICA PURA)               │
│                                                                   │
│    Para cada Alpha i:                                            │
│    • Obtén todos sus AlphaStates (1..6)                          │
│    • Para cada State, obtén sus StateChecklists                  │
│    • Consigue las ChecklistResponses del usuario                 │
│    • Verifica: ¿Todos los checklists del state completados?     │
│    • Retorna: currentStateNumber                                 │
│                                                                   │
│    REGLA SEMAT: currentState = min(5) para status=Green          │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 8. Infraestructura - EF Core → PostgreSQL                        │
│                                                                   │
│    SELECT * FROM alphas WHERE id = i;                            │
│    SELECT * FROM alpha_states WHERE alpha_id = i ORDER BY num;  │
│    SELECT * FROM state_checklists WHERE state_id = s;           │
│    SELECT * FROM checklist_responses                             │
│        WHERE project_id = 123 AND state_checklist_id IN (...);  │
│                                                                   │
│    ← EF Core traduce LINQ a SQL                                  │
│    ← Npgsql ejecuta en PostgreSQL                                │
│    ← Mapea resultados a entities C#                              │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 9. VUELTA - Construcción de Response                             │
│                                                                   │
│    Dominio retorna AlphaStateResult[] a Aplicación               │
│    Aplicación agrega metadata (progress %, classification)       │
│    Controller serializa a JSON: HealthResult                     │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 10. Respuesta HTTP                                               │
│                                                                   │
│    200 OK                                                         │
│    Content-Type: application/json                                │
│    {                                                              │
│      "healthScore": 72,                                          │
│      "classification": "Green",                                  │
│      "alphaDetails": [                                           │
│        {                                                          │
│          "alphaId": 1,                                           │
│          "alphaName": "Stakeholders",                            │
│          "currentStateNumber": 5,                                │
│          "maxStateNumber": 6,                                    │
│          "progress": 83                                          │
│        },                                                         │
│        ...                                                        │
│      ]                                                            │
│    }                                                              │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 11. React - Axios Response Handler                               │
│                                                                   │
│    apiClient interceptor procesa respuesta                       │
│    TanStack Query cachea en memoria                              │
│    Component re-renderiza con data                               │
└──────────────┬──────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────────┐
│ 12. UI Renderizado                                               │
│                                                                   │
│    <HealthDashboard /> recibe { data: HealthResult }             │
│    Renderiza 7 cards con barras de progreso                      │
│    Usuario ve: Stakeholders (83%), ...                           │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🔐 Autenticación y Seguridad

### Flujo de Login

```
React Login Form
  ↓
POST /api/auth/login { email, password }
  ↓
ASP.NET Core - AccountController.Login()
  ↓
IAuthService.AuthenticateAsync()
  ↓
EF Core busca usuario en DB
  ↓
Verifica password (hash)
  ↓
Crea ClaimsPrincipal + Cookie
  ↓
200 OK + Set-Cookie: .AspNetCore.Cookies=...
  ↓
React recibe cookie (automático con withCredentials: true)
  ↓
Siguientes requests incluyen cookie automáticamente
```

**Cookie config** (en `Program.cs`):
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
```

**CORS permitido solo para FrontendDev**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // ← Crucial para cookies
    });
});
```

---

## 📊 Stack Tecnológico Completo

### Backend

| Capa | Tecnología | Propósito |
|---|---|---|
| **Web Framework** | ASP.NET Core 8 | HTTP server, routing, middleware |
| **Lenguaje** | C# | OOP, LINQ, async/await |
| **ORM** | Entity Framework Core 8 | Mapeo relacional |
| **Driver DB** | Npgsql | Conexión PostgreSQL |
| **Database** | PostgreSQL 16 | RDBMS |
| **Authentication** | Cookies | Session management |
| **Testing** | xUnit + Moq | Unit tests (futuro) |

### Frontend

| Capa | Tecnología | Propósito |
|---|---|---|
| **Framework** | React 18 | UI components |
| **Lenguaje** | TypeScript | Type safety |
| **Build Tool** | Vite | Bundling, dev server |
| **HTTP Client** | Axios | REST calls |
| **Server State** | TanStack Query v5 | Caching, sync |
| **Client State** | Zustand | Global state |
| **Routing** | react-router-dom v6 | SPA navigation |
| **Styling** | Tailwind CSS v4 | Utility CSS |
| **Package Manager** | pnpm | Faster than npm |

---

## ✅ Ventajas de Esta Arquitectura

| Aspecto | Beneficio |
|---|---|
| **Separación Backend/Frontend** | Fácil evolucionar cada lado independientemente |
| **Clean Architecture** | Testeable, mantenible, agnóstico a frameworks |
| **Feature-Based Frontend** | Escalable, organizado por dominio, no por tecnología |
| **REST API** | Compatible con cualquier cliente (móvil, web, escritorio) |
| **TanStack Query** | Sincronización automática, caching, optimístico |
| **TypeScript both sides** | Type safety end-to-end |

---

## 🚀 Próximos Pasos

1. **Conectar features de React a APIs backend**:
   - `features/auth` → POST `/api/auth/login`
   - `features/projects` → GET/POST `/api/projects`
   - `features/evaluation` → POST `/api/evaluation/alpha`

2. **Implementar componentes UI**:
   - Forms con React Hook Form + Zod
   - Modales, dropdowns, validación

3. **State management con Zustand**:
   - Auth store (user, isLoading, error)
   - UI store (sidebar open, theme)

4. **Testing**:
   - Backend: xUnit + Moq
   - Frontend: Vitest + React Testing Library

5. **Deployment**:
   - Backend: Docker + Azure/AWS
   - Frontend: Vercel/Netlify o mismo servidor

---

## 📚 Documentación Complementaria

- **Backend detallado**: Revisar `ARQUITECTURA_EXPLICACION.md`
- **Plan de migración**: `FRONTEND_MIGRATION_PLAN.md`
- **Configuración DB**: `db/essence_mvp_schema.sql`
