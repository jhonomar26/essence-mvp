# ARQUITECTURA DEL SISTEMA - Explicación Técnica

## 📐 Decisión Fundamental: Clean Architecture

Aplicamos Clean Architecture porque:
- **Separación clara de responsabilidades**: cada capa tiene un único propósito
- **Testabilidad aislada**: lógica de negocio sin dependencias de frameworks
- **Flexibilidad**: cambiar DB o framework no toca lógica de negocio
- **Mantenibilidad**: nuevo desarrollador entiende flujo inmediatamente

---

## 🏗️ Las Cuatro Capas (De arriba a abajo)

### 1️⃣ CAPA DE PRESENTACIÓN (User-Facing)
**Ubicación**: `src/EssenceMvp.Mvc/Controllers/`

**Archivos clave**:
- `ProjectsController.cs` - Gestión de proyectos
- `AccountController.cs` - Autenticación y sesión
- `ProjectEvaluationController.cs` - Evaluación de Alphas
- `ProjectHealthController.cs` - Cálculo de salud
- `ProjectSnapshotController.cs` - Snapshots
- `HomeController.cs` - Landing

**Responsabilidad única**: Recibir petición HTTP → delegar → devolver respuesta

```csharp
// Ejemplo real:
public async Task<IActionResult> Index()
{
    var projects = await _projects.GetUserProjectsAsync(UserId);
    var summaries = new List<ProjectSummary>();
    foreach (var p in projects)
    {
        var health = await _health.CalculateHealthAsync(p.Id, UserId);
        summaries.Add(new ProjectSummary { ... });
    }
    return View(summaries);  // ← Delega a Service, retorna View
}
```

**ViewModels asociados** (`src/EssenceMvp.Mvc/Models/`):
- `CreateProjectViewModel.cs`
- `EvaluateChecklistViewModel.cs`
- `ChecklistAnswerViewModel.cs`
- Transportan datos entre Controller y Vista

---

### 2️⃣ CAPA DE APLICACIÓN (Business Logic)
**Ubicación**: `src/EssenceMvp.Mvc/Application/Services/`

**Interfaces (contratos)**:
- `IAuthService` - Autenticación
- `IProjectService` - Gestión de proyectos
- `IAlphaService` - Operaciones sobre Alphas
- `IAlphaEvaluationService` - **Evaluación de estados**
- `IHealthService` - Cálculo de salud
- `IHealthCalculationService` - Lógica de salud
- `IProjectDetailComposerService` - Composición de detalles
- `ISnapshotService` - Gestión de snapshots

**Patrón Interfaces-First**: Definimos contrato antes de implementación

```csharp
public interface IProjectService
{
    Task<List<Project>> GetUserProjectsAsync(int userId);
    Task<Project> CreateProjectAsync(int userId, string name, string? description, string? phase);
    Task<Project?> GetProjectDetailAsync(int projectId, int userId);
    // ... más métodos
}
```

**Implementaciones concretas**:
- `ProjectService.cs`
- `AuthService.cs`
- `AlphaService.cs`
- `AlphaEvaluationService.cs`
- `HealthService.cs`

**Característica clave**: No dependen de Controllers ni de vistas. Solo de Entities e Infrastructure.

---

### 3️⃣ CAPA DE DOMINIO (Domain Logic - Essence-Specific)
**Ubicación**: Embebida en `Application/Services/`

**Lógica de SEMAT Essence**:
```csharp
// AlphaEvaluationService.cs - Núcleo del dominio
public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
{
    // SEMAT Essence: Alpha en estado N solo si TODOS los checklists
    // de estados 1..N están satisfechos
    var alpha = await _db.Alphas
        .Include(a => a.States)
        .ThenInclude(s => s.Checklists)
        .FirstOrDefaultAsync(a => a.Id == alphaId);

    var states = alpha.States.OrderBy(s => s.StateNumber).ToList();
    short current = 0;

    // Iteración ordenada. Se detiene en primer estado incompleto
    foreach (var state in states)
    {
        var checklistIds = state.Checklists.Select(c => c.Id).ToList();
        var responses = await _db.ChecklistResponses
            .Where(r => r.ProjectId == projectId && checklistIds.Contains(r.StateChecklistId))
            .ToListAsync();

        bool completed = state.Checklists.All(c =>
            responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved));

        if (!completed) break;
        current = state.StateNumber;
    }
    return new AlphaStateResult { CurrentStateNumber = current, ... };
}
```

**Reglas de negocio puras**:
- Lógica de cálculo de estados (verde/amarillo/rojo)
- Evaluación de checklists
- Cálculo de salud del proyecto
- Reglas SEMAT Essence

---

### 4️⃣ CAPA DE INFRAESTRUCTURA (Data Access)
**Ubicación**: `src/EssenceMvp.Mvc/Infrastructure/`

**EF Core + Npgsql**:
- `EssenceDbContext.cs` - Context de Entity Framework

**Entities (mapeo DB → C#)**:
- `Project.cs`
- `Alpha.cs`
- `AlphaState.cs`
- `StateChecklist.cs`
- `ChecklistResponse.cs`
- `HealthReport.cs`
- `ProjectAlphaStatus.cs`
- `AppUser.cs`
- `UserSession.cs`

**PostgreSQL 16 (Backend)**:
- Tablas relacionales
- Índices de rendimiento
- Constraints de integridad

**Ventaja de aislamiento**: Mañana cambiamos a SQL Server o MongoDB → solo toca Infrastructure.

---

## 🔄 Flujo de una Petición (End-to-End)

```
┌─────────────────────────────────────────────────────────────┐
│ 1. NAVEGADOR                                                 │
│    GET /Projects/Index                                       │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 2. PRESENTACIÓN (ProjectsController)                         │
│    ├─ Recibe HttpContext                                     │
│    ├─ Extrae UserId del ClaimsPrincipal                      │
│    ├─ Inyección de dependencia: IProjectService              │
│    └─ Llama → _projects.GetUserProjectsAsync(UserId)        │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 3. APLICACIÓN (ProjectService)                               │
│    ├─ Recibe parámetro: UserId                               │
│    ├─ Inyección: EssenceDbContext                            │
│    ├─ Consulta LINQ                                          │
│    └─ Retorna: List<Project>                                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 4. INFRAESTRUCTURA (EF Core + PostgreSQL)                   │
│    ├─ EssenceDbContext traduce LINQ → SQL                   │
│    ├─ Npgsql ejecuta en PostgreSQL 16                        │
│    ├─ Retorna DataReader                                     │
│    └─ EF mapea a List<Project>                               │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 5. APLICACIÓN (VUELTA)                                       │
│    Para cada Project:                                        │
│    ├─ Calcula Health vía IHealthService                      │
│    └─ Construye ProjectSummary                               │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 6. PRESENTACIÓN (VUELTA)                                     │
│    ├─ Instancia ViewModel: List<ProjectSummary>              │
│    ├─ Selecciona View: ~/Projects/Index.cshtml               │
│    └─ Razor renderiza HTML                                   │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│ 7. NAVEGADOR                                                 │
│    Recibe HTML renderizado → Displayea al usuario            │
└─────────────────────────────────────────────────────────────┘
```

---

## 💉 Inyección de Dependencias (por qué importa)

En `Program.cs`:
```csharp
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IAlphaEvaluationService, AlphaEvaluationService>();
// ... resto de servicios
```

**Beneficio**:
- Controllers **no crean** servicios (loose coupling)
- Services **no crean** DbContext (responsabilidad delegada)
- Tests pueden inyectar **mocks** sin tocar producción

---

## 🎯 Características de Clean Architecture que vemos aquí

| Aspecto | Implementación |
|--------|-----------------|
| **Inversión de control** | Interfaces inyectadas, no new ServiceImpl() |
| **Separación de capas** | Controllers ≠ Services ≠ Infrastructure |
| **Reglas de negocio puras** | AlphaEvaluationService sin dependencias web |
| **Testabilidad** | Services testean con mocks sin DB real |
| **Independencia de frameworks** | Lógica de Essence no conoce ASP.NET |

---

## 🚨 Decisiones Específicas del Proyecto

### Enums y Mapping
- **AlphaState**: Verde (3), Amarillo (2), Rojo (1), Gris (0)
- Seed IDs hardcodeados en migraciones
- StateNumber = orden secuencial SEMAT

### Integridad de Datos
- FK: ProjectAlphaStatus → Project + Alpha
- Constraint: ResponseList completitud para evaluación

### Tolerancia al Cambio
- Si PostgreSQL → MongoDB: **solo Infrastructure cambia**
- Si reglas SEMAT → nuevas reglas: **AlphaEvaluationService actualiza**
- Si UI rediseñada: **Controllers + ViewModels actualizan**

---

## 📊 Diagrama Mental

```
┌─────────────────────────────────────────────┐
│   PRESENTACIÓN (ASP.NET Core MVC)           │
│   Controllers → ViewModels → Razor Views    │
├─────────────────────────────────────────────┤
│   APLICACIÓN (Business Services)            │
│   IProjectService, IHealthService, etc.     │
│   ← Lógica de orquestación                  │
├─────────────────────────────────────────────┤
│   DOMINIO (Essence-Specific)                │
│   AlphaEvaluationService, HealthCalc        │
│   ← Reglas SEMAT puras                      │
├─────────────────────────────────────────────┤
│   INFRAESTRUCTURA (EF Core + Npgsql)        │
│   EssenceDbContext, Entities, Migrations    │
└─────────────────────────────────────────────┘
         ↓
    PostgreSQL 16
```

---

## ✅ Conclusión para la Presentación

**El mensaje clave**: "No es un monolito. Cada capa tiene responsabilidad clara. Eso nos permite:
- Testear servicios sin tocar DB
- Cambiar DB sin reescribir lógica de negocio
- Onboardear desarrolladores rápido
- Escalar sin spaghetti code"
