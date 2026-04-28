# SLIDE 5 — ARQUITECTURA DEL SISTEMA
## Script para Presentador

---

## 📌 APERTURA (15 seg)
> "Ahora que vieron el software funcionando, hablemos de cómo está construido por dentro. La decisión más importante que tomamos fue aplicar **Clean Architecture**."

---

## 🏗️ LAS CAPAS (90 seg)

### 1. PRESENTACIÓN (15 seg)
> "La capa de PRESENTACIÓN es donde vive el usuario. Tenemos **controllers MVC** y vistas **Razor en ASP.NET Core 8**. Los controllers son deliberadamente **delgados** — su único trabajo es recibir la petición y delegar."

**Archivos**: `ProjectsController`, `AccountController`, `ProjectEvaluationController`, etc.

---

### 2. APLICACIÓN (30 seg)
> "La capa de APLICACIÓN tiene nuestros **Services**: AuthService, ProjectService, AlphaService, HealthService, SnapshotService. Aquí vive toda la lógica de negocio. Y son **interfaces primero** — IAuthService, IProjectService — lo que nos permite testearlos de forma aislada."

**Interfaces**: `IProjectService`, `IAlphaService`, `IHealthService`, etc.  
**Implementaciones**: `ProjectService`, `AlphaService`, `HealthService`, etc.

---

### 3. DOMINIO (30 seg)
> "La capa de DOMINIO tiene los **evaluadores específicos de Essence**. La lógica de cómo se calcula si un **Alpha está verde, amarillo o rojo**."

**Núcleo**: `AlphaEvaluationService.CalculateAsync()`
```csharp
// Alpha solo en estado N si TODOS checklists 1..N están completos
foreach (var state in states)
{
    bool completed = state.Checklists.All(c => 
        responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved));
    if (!completed) break;
    current = state.StateNumber;
}
```

---

### 4. INFRAESTRUCTURA (15 seg)
> "La capa de INFRAESTRUCTURA es donde vive **EF Core y Npgsql**. El **EssenceDbContext**, las **entities**, las **migraciones**. Está **completamente aislada** — si mañana cambiamos de base de datos, solo tocamos esta capa."

**Archivos**: `EssenceDbContext.cs`, `Entities/Project.cs`, `Alpha.cs`, etc.

---

## 💾 TRANSICIÓN
> "Y en la base está **PostgreSQL 16** — con decisiones técnicas que mi compañero explica en siguiente slide."

---

## 🎯 PUNTOS CLAVE
1. Separación clara: cada capa, un trabajo
2. Interfaces primero: testeable sin BD real
3. Flexibilidad: cambiar DB es cambio de infraestructura
4. No hay magia: Controllers → Services → Dominio → Data

---

## 💡 TIPS
✅ Señalar capas en slide de arriba a abajo mientras hablas
✅ Mostrar flujo: "Petición baja, resultado sube"
✅ No entres en detalles de EF Core — eso es siguiente slide
✅ Timing: 15 + 90 + 15 = **120 seg (2 min)**
