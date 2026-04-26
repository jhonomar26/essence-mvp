# SEMAT Essence Implementation - Validation & Examples

## State Calculation (Sequential, Binary)

### Code Location
`ProjectService.GetAlphaCurrentStateAsync(projectId, alphaId)` — lines iterate ordered states, stops at first incomplete.

### Example
**Alpha "Customer" with 3 states:**
```
State 1: [Checklist A, Checklist B]
State 2: [Checklist C, Checklist D]
State 3: [Checklist E]
```

**Scenario 1: All of State 1 completed**
```
ChecklistResponses: A=TRUE, B=TRUE
Result: currentState=1 ✓ (can't advance to 2 because C,D not answered)
```

**Scenario 2: States 1 and 2 completed, State 3 not**
```
ChecklistResponses: A=TRUE, B=TRUE, C=TRUE, D=TRUE
Result: currentState=2 ✓ (E not answered, so stop)
```

**Scenario 3: WRONG - Skip state 1, answer state 2**
```
ChecklistResponses: C=TRUE, D=TRUE (A and B NOT answered)
Result: currentState=0 ✓ (stops at state 1 because A,B not TRUE)
```

---

## Health Score Calculation (Essence Formula)

### Code Location
`HealthCalculationService.CalculateProjectHealthAsync(projectId)` — implements exact formula.

### Formula
```
For each Alpha in project:
  progress_i = (currentState_i / maxStates_i) * 100

Example with 3 Alphas:
  Alpha1: state=2/3 → progress = (2/3)*100 = 66.67%
  Alpha2: state=1/3 → progress = (1/3)*100 = 33.33%
  Alpha3: state=3/3 → progress = (3/3)*100 = 100%

Calculate:
  avg = (66.67 + 33.33 + 100) / 3 = 66.67
  max = 100
  min = 33.33
  dispersion = 100 - 33.33 = 66.67
  penalty = 66.67 * 0.2 = 13.33
  
healthScore = avg - penalty
healthScore = 66.67 - 13.33 = 53.34

Classification: 53.34 falls in [40, 59] → "EN RIESGO"
```

---

## Why Dispersion Penalty?

### Problem: Ignoring Imbalance
```
Project A (WRONG classification if only average):
  Alpha1: 100%
  Alpha2: 0%
  Average = 50% → Red
  
Project B:
  Alpha1: 50%
  Alpha2: 50%
  Average = 50% → Red
  
Same average, but different risk!
```

### Solution: Dispersion Penalty
```
Project A:
  dispersion = 100 - 0 = 100
  penalty = 100 * 0.2 = 20
  health = 50 - 20 = 30 → CRÍTICO ✓ (correctly reflects imbalance)
  
Project B:
  dispersion = 50 - 50 = 0
  penalty = 0 * 0.2 = 0
  health = 50 - 0 = 50 → EN RIESGO ✓ (balanced, less risky)
```

**Essence Insight:** Imbalance = risk. A project where one Alpha is 100% and another is 0% is in worse shape than balanced 50/50.

---

## Classification Bands

| Health Score | Classification | Traffic Light | Meaning |
|---|---|---|---|
| 80-100 | SALUDABLE | Green | Project is healthy, all Alphas balanced and advanced |
| 60-79 | ACEPTABLE | Yellow | Project progressing well but has minor imbalances |
| 40-59 | EN RIESGO | Yellow | Project at risk, significant imbalance or slow progress |
| 0-39 | CRÍTICO | Red | Project in critical state, major delays or severe imbalance |

---

## Validation Checklist

- [ ] No percentages used to determine Alpha state (only boolean checklist completion)
- [ ] No "heuristic" rules like "if 1 Alpha is behind state 3 → Red"
- [ ] State calculation stops at first incomplete state (sequential, not cumulative percentage)
- [ ] Progress = (currentState / maxStates) * 100, not percentage of checklists
- [ ] Dispersion penalty = (max - min) * 0.2, not arbitrary thresholds
- [ ] healthScore = average - penalty, clamped to [0, 100]
- [ ] Classification uses exact ranges: 80+, 60-79, 40-59, <40
- [ ] All DTOs include MaxStateNumber for progress calculation
- [ ] Both MVC and API layers use identical Essence logic
- [ ] Backward compatibility: traffic light mapping works for views

---

## Before/After Comparison

### OLD (Heuristic, Incorrect)
```csharp
// BAD: Magic number 3, arbitrary thresholds
int diff = 3 - currentStateNumber;
if (diff >= 2) return "Red";
if (diff == 1) return "Yellow";
return "Green";
```

### NEW (Essence Pure)
```csharp
// GOOD: Essence formula
var progress_i = (currentState_i / maxStates_i) * 100;
var avg = mean(all progress);
var dispersion = max - min;
var penalty = dispersion * 0.2;
var healthScore = avg - penalty;

var classification = healthScore switch {
    >= 80 => "SALUDABLE",
    >= 60 => "ACEPTABLE",
    >= 40 => "EN RIESGO",
    _ => "CRÍTICO"
};
```

---

## Database
No migration needed. Schema already supports:
- `project_alpha_status.current_state_number` (int)
- `alpha_state.state_number` (smallint)
- `state_checklist.is_achieved` (boolean via checklist_response)

---

## Next Steps for You

1. **Test state calculation** - verify sequential logic with manual project
2. **Verify health scores** - create projects with different imbalances, check penalty
3. **Check UI rendering** - ensure HealthResult is displayed correctly in views
4. **API testing** - POST checklist-responses and GET /projects/{id}/health
5. **Performance** - profile if projects have 100+ Alphas (current N Alphas query should be O(1))
