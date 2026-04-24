# Fase 3: Motor de Cálculo de Estado — MVC

## Resumen
Implementación de evaluación de checklists, recálculo automático de estado de Alphas y semáforo global de salud del proyecto.

---

## 1. Servicios

### IProjectService — Métodos nuevos

```csharp
Task SaveChecklistResponsesAsync(int projectId, List<(int stateChecklistId, bool isAchieved, string? notes)> responses);
```
- Guarda/actualiza respuestas de checklist en BD
- Upsert: si existe respuesta anterior, actualiza; si no, crea nueva
- Ejecutado después de cada evaluación de Alpha

```csharp
Task<List<(int Id, string CriterionText, bool IsAchieved)>> GetAlphaChecklistsAsync(int projectId, int alphaId);
```
- Obtiene todos los criterios de un Alpha ordenados por estado
- Incluye estado actual (IsAchieved) de cada criterio
- Usado para poblar modal de evaluación

### AlphaEvaluationService

Lógica acumulativa: Alpha está en estado N solo si todos los checkpoints de estados 1..N están satisfechos.

```csharp
public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
```
- Itera estados ordenados 1..max
- Para cada estado, verifica que TODOS sus criterios tengan `IsAchieved=true`
- Si falta 1 criterio, detiene y retorna último estado completado
- Retorna: `(CurrentStateNumber: short, CurrentStateName: string)`

**Variables tipos:**
- `current: short` — último estado completado
- `name: string` — nombre del estado

### HealthCalculationService

Semáforo global basado en atrasos de Alphas:
- **Red**: 1+ Alpha con atraso ≥2 estados OR 3+ Alphas con atraso 1
- **Yellow**: 1+ Alpha con atraso 1 (y menos de 3)
- **Green**: todos en nivel esperado

```csharp
int diff = 3 - s.CurrentStateNumber;  // ¿cuántos estados le faltan para llegar a 3?
if (diff >= 2) red++;      // falta 2+ → rojo
else if (diff == 1) yellow++;  // falta 1 → amarillo
```

---

## 2. Controladores

### AlphaEvaluationController

**POST `/evaluation/alpha`**

Flujo completo de evaluación:

```
1. Recibe EvaluateChecklistViewModel
   └─ ProjectId, AlphaId, List<ChecklistAnswerViewModel>

2. Guarda respuestas en BD
   └─ await _projectService.SaveChecklistResponsesAsync(...)

3. Recalcula estado del Alpha
   └─ var stateResult = await _alphaEvalService.CalculateAsync(projectId, alphaId)

4. Actualiza ProjectAlphaStatus
   └─ projAlpha.CurrentStateNumber = stateResult.CurrentStateNumber
   └─ projAlpha.UpdatedAt = DateTimeOffset.UtcNow
   └─ await _db.SaveChangesAsync()

5. Retorna resultado como JSON
   └─ return Json(stateResult)  // { CurrentStateNumber, CurrentStateName }
```

### ProjectHealthController

**GET `/evaluation/health/{projectId}`**

Retorna salud completa del proyecto:

```
1. Obtiene proyecto con todos sus Alphas
2. Calcula semáforo global usando HealthCalculationService
3. Para cada Alpha:
   └─ calcula estado actual con AlphaEvaluationService
   └─ agrega a lista de detalles
4. Retorna HealthResult con:
   └─ GlobalStatus: "Green|Yellow|Red"
   └─ AlphaDetails: List<(AlphaId, AlphaName, CurrentStateNumber, CurrentStateName)>
```

---

## 3. Modelos

### AlphaStateResult
```csharp
public short CurrentStateNumber { get; set; }      // 0..N
public string CurrentStateName { get; set; }       // "Not Started", "Inception", etc.
```
**Nota**: `short` coincide con BD (`state_number` = smallint)

### HealthResult
```csharp
public string GlobalStatus { get; set; }           // "Green|Yellow|Red"
public List<AlphaDetail> AlphaDetails { get; set; }

public class AlphaDetail
{
    public int AlphaId { get; set; }
    public string AlphaName { get; set; }
    public short CurrentStateNumber { get; set; }
    public string CurrentStateName { get; set; }
}
```

### EvaluateChecklistViewModel
```csharp
public int ProjectId { get; set; }
public int AlphaId { get; set; }
public List<ChecklistAnswerViewModel> Answers { get; set; }
```

Donde `ChecklistAnswerViewModel`:
```csharp
public int StateChecklistId { get; set; }
public bool IsAchieved { get; set; }
public string? Notes { get; set; }
```

---

## 4. Vistas

### Detail.cshtml — Cambios

**Agregar botón "Evaluar" en cada Alpha**
```html
<button type="button" class="btn btn-sm btn-outline-primary" 
        data-bs-toggle="modal" 
        data-bs-target="#evaluateAlphaModal_@alpha.AlphaId">
    <i class="bi bi-pencil-square"></i> Evaluar
</button>
```

**Renderizar modales**
```razor
@{
    var alphaChecklists = (Dictionary<int, List<(int, string, bool)>>)ViewBag.AlphaChecklists;
    if (alphaChecklists != null)
    {
        foreach (var alpha in Model.AlphaProgress)
        {
            if (alphaChecklists.TryGetValue(alpha.AlphaId, out var checklists))
            {
                await Html.RenderPartialAsync("_AlphaEvaluationModal", 
                    (Model.Id, alpha.AlphaId, alpha.AlphaName, checklists));
            }
        }
    }
}
```

### _AlphaEvaluationModal.cshtml (Nuevo)

Modal para evaluar criterios de un Alpha:

**Estructura:**
```
Modal Header
  └─ Título: "Evaluar: [AlphaName]"

Modal Body
  └─ Lista de checkboxes (uno por criterio)
  └─ Datos precargados con IsAchieved anterior

Modal Footer
  └─ Botón Cancelar
  └─ Botón Guardar
```

**JavaScript: `submitEvaluation(e, alphaId)`**
```js
1. Previene submit por defecto
2. Recopila estado de checkboxes
3. Construye payload JSON: { projectId, alphaId, answers: [...] }
4. POST a /evaluation/alpha
5. Si OK: cierra modal + reload página
6. Si error: alert con mensaje
```

---

## 5. Controlador ProjectsController — Cambios

**Method: Detail(int id)**

Agregar carga de checklists antes de renderizar:

```csharp
// Load checklists for each alpha to pass to modals
var alphaChecklists = new Dictionary<int, List<(int, string, bool)>>();
foreach (var alpha in alphaProgress)
{
    alphaChecklists[alpha.AlphaId] = 
        await _projects.GetAlphaChecklistsAsync(id, alpha.AlphaId);
}
ViewBag.AlphaChecklists = alphaChecklists;
```

Esto proporciona a la vista los criterios de cada Alpha para renderizar en modales.

---

## 6. Flujo Completo de Usuario

```
1. Usuario accede a /Projects/Detail/{projectId}
   ↓
2. ProjectsController.Detail() carga:
   - Datos de proyecto
   - HealthStatus global (usando HealthService)
   - Progreso de 7 Alphas (AlphaProgressDto)
   - Checklists de cada Alpha (para modales)
   ↓
3. Vista renderiza:
   - Semáforo grande en esquina
   - 7 cards de Alphas con progreso
   - Botón "Evaluar" en cada card
   - 7 modales ocultos (uno por Alpha)
   ↓
4. Usuario hace clic en "Evaluar" en Alpha X
   ↓
5. Modal se abre mostrando:
   - Título: "Evaluar: [Nombre del Alpha]"
   - Checkboxes de criterios (precargados con evaluación anterior)
   ↓
6. Usuario marca/desmarca checkboxes, clica "Guardar"
   ↓
7. submitEvaluation(e, alphaId) ejecuta:
   - POST /evaluation/alpha con payload JSON
   - Body: { projectId, alphaId, answers: [...] }
   ↓
8. AlphaEvaluationController.Evaluate():
   a) Guarda respuestas en ChecklistResponse
   b) Calcula nuevo estado: AlphaEvaluationService.CalculateAsync()
   c) Actualiza ProjectAlphaStatus.CurrentStateNumber
   d) Retorna AlphaStateResult como JSON
   ↓
9. JavaScript cierra modal + reload página
   ↓
10. Detail() recalcula salud global y vuelve a renderizar
    (nuevo semáforo + progreso actualizado)
```

---

## 7. Base de Datos — Entidades Usadas

```
ChecklistResponse
├─ ProjectId (FK)
├─ StateChecklistId (FK)
├─ IsAchieved (bool)
├─ Notes (string?)
└─ UpdatedAt (DateTimeOffset)

ProjectAlphaStatus
├─ ProjectId (FK)
├─ AlphaId (FK)
├─ CurrentStateNumber (short) ← ACTUALIZADO POR EVALUACIÓN
└─ UpdatedAt (DateTimeOffset)

Project
├─ UserId (FK)
├─ AlphaStatuses (nav)
└─ ChecklistResponses (nav)
```

---

## 8. Tipos — Conversiones Importantes

| Campo | Tipo | Razón |
|-------|------|-------|
| `StateNumber` (AlphaState) | `short` | BD: smallint |
| `CurrentStateNumber` (ProjectAlphaStatus) | `short` | BD: smallint |
| `CurrentStateNumber` (AlphaStateResult) | `short` | Coincide con BD |
| `CurrentStateNumber` (AlphaProgressDto) | `short` | Coincide con BD |

**Nota**: En JavaScript/JSON se envía como número entero. La conversión a `short` ocurre en EF Core.

---

## 9. Ejemplo: Payload POST

```json
{
  "projectId": 5,
  "alphaId": 3,
  "answers": [
    { "stateChecklistId": 10, "isAchieved": true, "notes": null },
    { "stateChecklistId": 11, "isAchieved": false, "notes": null },
    { "stateChecklistId": 12, "isAchieved": true, "notes": "Completado parcialmente" }
  ]
}
```

Respuesta esperada:
```json
{
  "currentStateNumber": 2,
  "currentStateName": "Worked"
}
```

---

## 10. Pruebas Manuales

1. **Crear proyecto** → /Projects/Create
2. **Abrir proyecto** → /Projects/Detail/[id]
3. **Evaluar Alpha** → Clic botón "Evaluar" → Marcar checkboxes → "Guardar"
4. **Verificar:**
   - Semáforo actualiza
   - Estado del Alpha avanza
   - Recarga sin errores
5. **Evaluar segundo Alpha** → Verificar semáforo global recalcula

---

## 11. Errores Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| Modal no abre | ViewBag.AlphaChecklists null | Verificar que Detail() cargue checklists |
| `CS0266: No se puede convertir int en short` | Tipo incorrecto | Cambiar `int` a `short` en variable |
| Checklist no se guarda | Falta SaveChangesAsync() | Verificar SaveChecklistResponsesAsync |
| Estado no recalcula | AlphaEvaluationService no llamado | Verificar AlphaEvaluationController |

---

## 12. Próximos Pasos (Opcional)

- [ ] Agregar validación de checklist vacío
- [ ] Historial de cambios de estado
- [ ] Exportar reporte de salud
- [ ] Webhooks para notificaciones cuando status cambia
- [ ] Caché de evaluación para rendimiento
