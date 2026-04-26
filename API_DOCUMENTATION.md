# API Essence MVP — Documentación

## Swagger

Acceder a documentación interactiva en **desarrollo**:
```
https://localhost:7153/swagger/index.html
```

## Autenticación

**Header requerido:**
```
Authorization: Bearer {accessToken}
```

**Obtener token:** `POST /auth/login`

---

## Endpoints

### Auth

#### Register
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "displayName": "User Name"
}
```

**Response:**
```json
{
  "message": "User registered successfully",
  "accessToken": "eyJhbGc...",
  "refreshToken": "..."
}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

#### Refresh Token
```http
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "..."
}
```

#### Logout
```http
POST /auth/logout
```

#### Get Current User
```http
GET /auth/me
Authorization: Bearer {token}
```

**Response:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

---

### Projects

#### List Projects
```http
GET /projects
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Mi Proyecto",
    "description": "Descripción",
    "phase": "Exploring",
    "createdAt": "2026-04-26T10:00:00Z"
  }
]
```

#### Get Project
```http
GET /projects/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "name": "Mi Proyecto",
  "description": "Descripción",
  "phase": "Exploring",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "alphaStatuses": [...],
  "createdAt": "2026-04-26T10:00:00Z"
}
```

#### Create Project
```http
POST /projects
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Nuevo Proyecto",
  "description": "Opcional",
  "phase": "Initiated"
}
```

**Phases válidas:** Initiated, Exploring, In Development, Stabilizing, Operational, Closed

#### Update Project
```http
PUT /projects/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Nombre actualizado",
  "description": "Descripción",
  "phase": "Stabilizing"
}
```

#### Delete Project
```http
DELETE /projects/{id}
Authorization: Bearer {token}
```

#### Get Project Health
```http
GET /projects/{id}/health
Authorization: Bearer {token}
```

**Response (SEMAT Essence):**
```json
{
  "projectId": 1,
  "healthScore": 72.5,
  "healthClassification": "ACEPTABLE",
  "averageProgress": 75.0,
  "progressDispersion": 25.0,
  "penalization": 5.0,
  "alphaDetails": [
    {
      "alphaId": 1,
      "alphaName": "Stakeholders",
      "currentStateNumber": 2,
      "currentStateName": "Recognized",
      "progress": 66.67
    }
  ]
}
```

#### Save Checklist Responses
```http
POST /projects/{id}/checklist-responses
Authorization: Bearer {token}
Content-Type: application/json

{
  "projectId": 1,
  "alphaId": 3,
  "answers": [
    {
      "stateChecklistId": 1001,
      "isAchieved": true,
      "notes": "Completado"
    }
  ]
}
```

#### Create Snapshot
```http
POST /projects/{id}/snapshots
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "projectId": 1,
  "healthScore": 75.5,
  "healthClassification": "ACEPTABLE",
  "createdAt": "2026-04-26T14:30:00Z"
}
```

#### Get Snapshots
```http
GET /projects/{id}/snapshots
Authorization: Bearer {token}
```

#### Delete Snapshot
```http
DELETE /projects/{projectId}/snapshots/{snapshotId}
Authorization: Bearer {token}
```

---

### Alphas

#### List Alphas
```http
GET /alphas
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Stakeholders",
    "areaOfConcern": "Equity",
    "levelOfDetail": "Essential"
  }
]
```

#### Get Alpha States
```http
GET /alphas/{id}/states
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "stateNumber": 0,
    "stateName": "Not Started",
    "description": "..."
  },
  {
    "stateNumber": 1,
    "stateName": "Recognized",
    "description": "..."
  }
]
```

#### Get Alpha Checklist
```http
GET /alphas/{id}/states/{stateNumber}/checklist
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1001,
    "criterionText": "Criterion description",
    "stateNumber": 1
  }
]
```

---

## Códigos HTTP

| Código | Significado |
|--------|------------|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Errores

**Formato estándar:**
```json
{
  "error": "Descripción del error",
  "details": "..."
}
```

---

## CORS

API usa credenciales por defecto. MVC frontend en `https://localhost:7089` tiene acceso.

---

## Versioning

API v1 (sin versioning explícito en URLs).

---

## Rate Limiting

No implementado en MVP. Considerar para producción.

---

## Notas

- Todos los timestamps en UTC ISO 8601
- IDs son integers (excepto userId = GUID)
- Passwords: min 8 chars, 1 mayúscula, 1 número, 1 especial
