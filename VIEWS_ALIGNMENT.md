# Vistas Alineadas con SEMAT Essence

## Cambios Realizados

### 1. ViewModels Actualizados

**ProjectDetailViewModel:**
- ❌ Removido: `AlphaHealth` por Alpha (NO existe en Essence)
- ✅ Agregado: `HealthScore` (decimal)
- ✅ Agregado: `HealthClassification` (string: SALUDABLE/ACEPTABLE/EN RIESGO/CRÍTICO)
- ✅ Agregado: `AverageProgress` (decimal)
- ✅ Agregado: `ProgressDispersion` (decimal)

**AlphaProgressDto:**
- ❌ Removido: `AlphaHealth` - NO hay salud individual por Alpha en Essence
- ❌ Removido: `CompletionPercent` - reemplazado por `Progress` (cálculo: currentState/maxStates*100)
- ❌ Renombrado: `TotalStates` → `MaxStateNumber` (mejor claridad)
- ✅ Agregado: `MaxStateNumber` (short)
- ✅ Agregado: `Progress` (decimal)

---

### 2. Servicios Actualizados

**AlphaService.GetProjectAlphaProgressAsync():**
```csharp
// ANTES (Heurístico):
var health = completionPercent >= 70 ? HealthStatus.Green
    : completionPercent >= 40 ? HealthStatus.Yellow
    : HealthStatus.Red;

// AHORA (Essence):
var progress = maxStates == 0 ? 0m : ((currentState / maxStates) * 100);
// NO calcula salud por Alpha, solo progreso
```

**ProjectsController.Detail():**
- ✅ Inyecta `IHealthCalculationService`
- ✅ Obtiene `healthScore` completo (score, classification, dispersion, alphas)
- ✅ Pasa datos a ViewModel para mostrar en vista

---

### 3. Vistas Actualizadas

**Projects/Detail.cshtml:**

**OLD (Incorrecto):**
```html
<!-- Muestra semáforo por Alpha (NO es Essence) -->
@foreach (var alpha in Model.AlphaProgress)
{
    var (borderClass, barClass, badgeBg) = alpha.AlphaHealth switch { ... };
    <!-- Color por Alpha: border-success, bg-success, etc -->
}
```

**NEW (Essence Puro):**
```html
<!-- Muestra SOLO progreso del proyecto a nivel global -->
<div style="padding: 1rem; background: #f8f9fa;">
    <div style="font-size: 2.5rem;">@Model.HealthScore.ToString("F1")</div>
    <small>@Model.HealthClassification</small>
    <!-- Semáforo único = mapeo visual del score -->
</div>

<!-- Info Essence -->
<div class="alert alert-info">
    Promedio: @Model.AverageProgress%
    • Dispersión: @Model.ProgressDispersion%
    • Penalización: @(ProgressDispersion * 0.2)
</div>

<!-- Alphas sin semáforo individual, SOLO progreso -->
@foreach (var alpha in Model.AlphaProgress)
{
    <!-- Card sin colores por Alpha -->
    <span class="badge bg-info text-dark">
        @alpha.CurrentStateNumber / @alpha.MaxStateNumber
    </span>
    
    <div class="progress-bar bg-primary" 
         style="width: @alpha.Progress%">
    </div>
    
    <span>Estado: @alpha.CurrentStateName 
        • Progreso: @alpha.Progress.ToString("F1")%</span>
}
```

---

### 4. Flujo de Datos Essence

```
Checklist Responses (ChecklistResponse)
    ↓
AlphaEvaluationService.CalculateAsync()
    → CurrentStateNumber (secuencial)
    → MaxStateNumber (total estados)
    ↓
AlphaService.GetProjectAlphaProgressAsync()
    → Progress = (currentState / maxStates) * 100
    ↓
ProjectDetailViewModel
    ↓
Detail.cshtml muestra:
    - HealthScore global (avg - dispersion * 0.2)
    - Classification (SALUDABLE/ACEPTABLE/EN RIESGO/CRÍTICO)
    - Cada Alpha: estado actual, max estados, progreso %
    - NO: salud individual por Alpha
    - NO: heurísticas de semáforo
```

---

### 5. Lo Que NO Cambió (Correcto)

**_AlphaEvaluationModal.cshtml:**
- ✅ Sigue siendo correcto (muestra checklists, binarios TRUE/FALSE)
- ✅ NO tiene lógica de salud
- ✅ Puro Essence

**ProjectHealthController:**
- ✅ Endpoint GET /evaluation/health/{projectId}
- ✅ Devuelve HealthResult con score + classification + dispersion
- ✅ Endpoint puro Essence

**Projects/Index.cshtml:**
- ✅ Muestra OverallHealth (enum: semáforo)
- ✅ Es mapeo visual de HealthScore → traffc light
- ✅ Compatible con Essence

---

### 6. Validación Visual

| Elemento | Antes | Ahora | Essence? |
|---|---|---|---|
| Salud por Alpha | ✅ Rojo/Amarillo/Verde | ❌ Removido | ✅ |
| Score global | ❌ String (Red/Yellow) | ✅ Decimal 0-100 | ✅ |
| Clasificación | ❌ Semáforo | ✅ SALUDABLE/... | ✅ |
| Progreso Alpha | ❌ % heurístico | ✅ currentState/maxStates*100 | ✅ |
| Dispersión | ❌ No mostrada | ✅ max - min | ✅ |
| Penalización | ❌ No mostrada | ✅ dispersion * 0.2 | ✅ |

---

## Resumen

✅ Cero lógica heurística en vistas
✅ Essence puro: solo datos del cálculo
✅ Salud a nivel de PROYECTO, no por Alpha
✅ Progreso por Alpha clara: estado actual / máximo
✅ Semáforo es mapeo visual, no clasificación Essence
✅ Build compila sin errores
