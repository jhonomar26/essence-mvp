# SEMAT Essence Implementation - Refactoring Notes

## Overview
Refactored health calculation and alpha state evaluation to follow SEMAT Essence v1.2 standard (OMG). Removed heuristic semaphore logic (Red/Yellow/Green thresholds). Implemented pure Essence sequential state calculation and dispersion-based health scoring.

---

## Key Changes

### 1. Alpha State Calculation (Essence Sequential Logic)
**Implemented in:** 
- `AlphaEvaluationService.CalculateAsync(projectId, alphaId)` (MVC & API)
- `ProjectService.GetAlphaCurrentStateAsync(projectId, alphaId)` (MVC)

**Logic:**
- Iterates Alpha states ordered by StateNumber (1, 2, 3, ...)
- Stops at first incomplete state (not all checklists = TRUE)
- Current state = highest state where ALL checklists 1..N are satisfied
- **No percentages, no jumps, strict sequential**

**Updated DTOs:**
- `AlphaStateResult` now includes `MaxStateNumber` (needed for progress calculation)

---

### 2. Project Health Score (Essence Formula)
**Implemented in:**
- `HealthCalculationService.CalculateProjectHealthAsync(projectId)` (MVC & API)

**Formula:**
```
For each Alpha:
  progress = (currentState / maxStates) * 100

Then:
  averageProgress = mean(all progress values)
  dispersion = max(progress) - min(progress)
  penalty = dispersion * 0.2
  healthScore = averageProgress - penalty
  
Classification:
  80-100    → SALUDABLE
  60-79     → ACEPTABLE
  40-59     → EN RIESGO
  0-39      → CRÍTICO
```

**Why dispersion penalty?**
- Pure average ignores imbalance
- Penalizes projects where some Alphas lag far behind others
- Encourages balanced progress (key Essence concept)

---

### 3. Updated DTOs
**New Models:**
- `ProjectHealthScore` (MVC) / `ProjectHealthScoreDto` (API)
- `AlphaProgressDetail` (MVC) / `AlphaProgressDetailDto` (API)
- `HealthResponseDto` (API)

**Retired:**
- Old `HealthDto` with string `GlobalStatus`

**Updated:**
- `HealthResult` (MVC) - now contains score, classification, dispersion
- `AlphaStateResult` - added `MaxStateNumber`

---

### 4. Traffic Light Mapping (for Views)
**In:** `HealthService.CalculateHealthAsync(projectId, userId)`

Maps decimal health scores to enum for backward compatibility:
- SALUDABLE (80-100) → Green
- ACEPTABLE (60-79) → Yellow  
- EN RIESGO (40-59) → Yellow
- CRÍTICO (0-39) → Red

---

### 5. Endpoints Updated

**MVC:**
- `GET /evaluation/health/{projectId}` - returns ProjectHealthScore JSON

**API:**
- `GET /projects/{id}/health` - returns HealthResponseDto with Essence metrics

Both now use `CalculateProjectHealthAsync()` instead of `CalculateAsync()`.

---

## Files Modified

### MVC Layer
- `Services/IHealthCalculationService.cs` - signature changed
- `Services/HealthCalculationService.cs` - Essence formula implementation
- `Services/AlphaEvaluationService.cs` - added MaxStateNumber
- `Services/IProjectService.cs` - added GetAlphaCurrentStateAsync
- `Services/ProjectService.cs` - implemented GetAlphaCurrentStateAsync
- `Services/HealthService.cs` - uses new health calculation
- `Models/AlphaStateResult.cs` - added MaxStateNumber
- `Models/ProjectHealthScore.cs` - NEW DTO
- `Models/HealthResult.cs` - restructured for Essence
- `Controllers/ProjectHealthController.cs` - uses new service

### API Layer
- `Services/IHealthCalculationService.cs` - signature changed
- `Services/HealthCalculationService.cs` - Essence formula implementation
- `Services/AlphaEvaluationService.cs` - added MaxStateNumber  
- `Services/IAlphaEvaluationService.cs` - record updated
- `Features/AlphaDtos.cs` - added ProjectHealthScoreDto, AlphaProgressDetailDto
- `Features/ProjectDtos.cs` - replaced HealthDto with HealthResponseDto
- `Features/ProjectEndpoints.cs` - updated health endpoint

---

## Testing Recommendations

1. **State Calculation**
   - Create project with Alpha (3 states)
   - Mark checklists for state 1 only → currentState=1
   - Mark checklists for states 1,2 → currentState=2
   - Mark only state 2 (skip state 1) → currentState=0 (sequential!)

2. **Health Scoring**
   - All Alphas at 100% → score ≈ 100 (SALUDABLE)
   - All Alphas at 50% → score ≈ 50 (EN RIESGO)
   - 3 Alphas at 100%, 1 at 0% → dispersion=100, penalty=20, score≈70 (ACEPTABLE due to imbalance)

3. **Backward Compatibility**
   - Check dashboard still shows traffic light colors
   - Verify all views render without errors

---

## Database
No schema changes required. Uses existing tables:
- `alpha`, `alpha_state`, `state_checklist`
- `project_alpha_status`, `checklist_response`
- `health_report` (snapshots - optional)

---

## References
- SEMAT Essence v1.2 (OMG)
- Previous implementation notes in comments within each method
