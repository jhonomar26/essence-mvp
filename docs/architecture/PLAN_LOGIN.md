# Plan: Implementar Login (backend + frontend)

## Contexto

La rama actual es `feature/login-inicio-sesion`. El backend ya fue refactorizado a Clean Architecture (Domain/Application/Infrastructure/Mvc) y el frontend fue scaffoldeado con Vite+React, pero **el login nunca se conectó de punta a punta**:

- **Backend**: ya existen `AppUser` (entidad), `IAuthService`/`AuthService` (`AuthenticateAsync`, `RegisterAsync` con `PasswordHasher<AppUser>`), `IAppUserRepository`/`AppUserRepository`, y todo está registrado en DI. **Pero no existe ningún controller** que exponga esto por HTTP — no hay `POST /auth/login` en ningún lado.
- **Modelo de sesión ya definido en la base de datos**: `db/essence_mvp_schema.sql` define `user_session` (`app_user_id`, `refresh_token_hash`, `expires_at`, `revoked_at`, `replaced_by_token_hash`) y el dominio ya tiene la entidad `UserSession` mapeada (`UserSessionConfiguration`). **Esta tabla es la fuente de verdad de las sesiones activas** — el login debe crear una fila ahí y cada request autenticado debe validarse contra ella (para poder revocar/hacer logout de verdad). Por eso se descarta el enfoque anterior de `AddCookie()` de ASP.NET Core: esa cookie es autocontenida (se valida desencriptándola, sin tocar la DB) y dejaría `user_session` sin usar, que es exactamente lo que ya estaba pasando.
- **Frontend**: `features/auth/*` son carpetas vacías (`.gitkeep`), `useAuthStore` es un placeholder sin acciones, y el router (`react-router-dom` ya instalado) solo tiene la ruta `/`. `App.tsx` ni siquiera usa el `RouterProvider` todavía.
- **Bug preexistente detectado**: `ProjectHealthController.cs` importa `EssenceMvp.Mvc.Models` y usa `HealthResult`, pero esa carpeta/clase no existe — el backend probablemente no compila hoy. Se corrige como parte de este trabajo porque bloquea probar cualquier cosa en runtime.

Decisiones de alcance confirmadas con el usuario:
- **Sesión respaldada por `user_session`, no por cookies de ASP.NET Core.** El token de sesión viaja como header `Authorization: Bearer <token>` (no cookie), y cada request autenticado se valida contra la tabla.
- Modelo simple: **un solo token de sesión por login** (sin rotación de refresh token todavía; `replaced_by_token_hash` queda sin usar por ahora — es para una fase futura de refresh rotation).
- Se arregla el build roto de `ProjectHealthController`/`HealthResult`.
- Se incluye login **y** registro (register ya tiene lógica en `AuthService`, solo falta exponerlo).

## Backend

### 1. Arreglar build roto
Crear `src/EssenceMvp.Mvc/Models/HealthResult.cs` con la clase `HealthResult` (propiedades `HealthScore`, `Classification`, `AverageProgress`, `ProgressDispersion`, `AlphaDetails` con `AlphaDetail` anidado: `AlphaId`, `AlphaName`, `CurrentStateNumber`, `MaxStateNumber`, `Progress`), inferido de cómo lo usa `ProjectHealthController.cs`.

### 2. Repositorio de sesiones
Nuevo `src/EssenceMvp.Application/Abstractions/IUserSessionRepository.cs` + `src/EssenceMvp.Infrastructure/Repositories/UserSessionRepository.cs` (mismo patrón que `IAppUserRepository`/`AppUserRepository`), con:
- `Task<UserSession> CreateAsync(UserSession session)`
- `Task<UserSession?> GetByTokenHashAsync(string tokenHash)` — trae también el `AppUser` relacionado (`Include`) para no hacer dos queries.
- `Task RevokeAsync(UserSession session)` — setea `RevokedAt = DateTime.UtcNow` y guarda.

Registrar en `Infrastructure/DependencyInjection.cs` igual que `IAppUserRepository`.

### 3. Emisión y validación de tokens
Nuevo `src/EssenceMvp.Application/Services/ISessionTokenService.cs` / `SessionTokenService.cs`:
- `GenerateToken()` → bytes aleatorios criptográficamente seguros (`RandomNumberGenerator`, 32 bytes) codificados en Base64Url → el token "crudo" que se le da al cliente.
- `Hash(string rawToken)` → SHA-256 del token crudo, el único valor que se guarda en `user_session.refresh_token_hash` (igual que ya se hace con el password: nunca se persiste el valor en texto plano que viaja al cliente).

Este servicio lo usa un nuevo método en `IAuthService`/`AuthService`, p.ej. `Task<(AppUser user, string token)> CreateSessionAsync(AppUser user)`, que:
1. Genera el token crudo + su hash.
2. Crea una fila `UserSession` (`AppUserId`, `RefreshTokenHash = hash`, `ExpiresAt = UtcNow.AddHours(8)`).
3. Devuelve el token crudo (para la respuesta HTTP) — el hash es lo único que queda en DB.

### 4. Esquema de autenticación custom (reemplaza `AddCookie`)
En vez de `AddAuthentication().AddCookie(...)`, se implementa un `AuthenticationHandler` propio:

`src/EssenceMvp.Mvc/Auth/SessionTokenAuthenticationHandler.cs` (hereda de `AuthenticationHandler<AuthenticationSchemeOptions>`):
- Lee el header `Authorization: Bearer <token>`.
- Si no hay header → `AuthenticateResult.NoResult()`.
- Si hay token: lo hashea (mismo `SessionTokenService.Hash`), busca en `IUserSessionRepository.GetByTokenHashAsync`.
- Si no existe, o `RevokedAt != null`, o `ExpiresAt <= UtcNow` → `AuthenticateResult.Fail(...)` (esto ya produce un `401` limpio para `[Authorize]`, sin redirects — se resuelve solo el problema de "302 en vez de 401" que tenía el plan anterior, sin necesitar los `Events.OnRedirectToLogin` de cookies).
- Si es válida: construye `ClaimsPrincipal` con `ClaimTypes.NameIdentifier = session.AppUserId` (mismo claim que ya espera `ProjectSnapshotController.UserId`) y `AuthenticateResult.Success(...)`.

En `Program.cs`:
```csharp
builder.Services.AddAuthentication("SessionToken")
    .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>("SessionToken", null);
builder.Services.AddAuthorization();
```
Se elimina `using Microsoft.AspNetCore.Authentication.Cookies;` y el bloque `.AddCookie(...)`.

CORS: ya no hace falta `AllowCredentials()` (no hay cookies cruzando origen); se ajusta la policy `FrontendDev` para permitir el header `Authorization` (`AllowAnyHeader()` ya lo cubre).

### 5. DTOs de request/response
`src/EssenceMvp.Application/DTOs/AuthDtos.cs`:
- `LoginRequest(string Email, string Password)`
- `RegisterRequest(string Email, string Password, string? DisplayName)`
- `UserResponse(int Id, string Email, string? DisplayName)` — nunca se expone `PasswordHash`.
- `AuthResponse(string Token, UserResponse User)` — lo que devuelven login/register.

### 6. `AccountController`
Nuevo `src/EssenceMvp.Mvc/Controllers/AccountController.cs`, patrón de `ProjectSnapshotController.cs`. Ruta base `auth` (consistente con `evaluation/health`, `projects/{id}/snapshots` que no usan prefijo `/api`).

- `POST auth/login` — `AuthenticateAsync(email, password)`; si `null` → `401`. Si ok → `CreateSessionAsync(user)` → `200 OK` con `AuthResponse`.
- `POST auth/register` — `RegisterAsync(...)`; si `error != null` → `409 Conflict`. Si ok → `CreateSessionAsync(user)` (login automático) → `201 Created` con `AuthResponse`.
- `POST auth/logout` — `[Authorize(AuthenticationSchemes = "SessionToken")]`. Vuelve a leer el header `Authorization`, lo hashea, busca la `UserSession` y la revoca (`IUserSessionRepository.RevokeAsync`). `200 OK`.
- `GET auth/me` — `[Authorize]`. Lee `ClaimTypes.NameIdentifier`, usa `IAppUserRepository.GetByIdAsync` (método nuevo — el repo hoy solo tiene `GetByEmailAsync`; añadirlo con el mismo patrón). Devuelve `UserResponse`.

### 7. Verificación backend
- `dotnet build` (si el SDK está disponible en este entorno; si no, compilar/probar manualmente).
- `curl`/Postman: registrar usuario → recibir `token` → llamar `auth/me` con `Authorization: Bearer <token>` → confirmar `200`. Llamar `auth/logout` → confirmar que el mismo token ya no sirve (`401` en la siguiente llamada a `auth/me`) y que en la tabla `user_session` quedó `revoked_at` seteado.

## Frontend

### 8. Cliente HTTP sin cookies
`frontend/src/shared/api/client.ts`: quitar `withCredentials: true`. Añadir un interceptor de request que lea el token desde el store de auth (o desde donde se persista) y lo mande como `Authorization: Bearer <token>`; interceptor de response que, ante `401`, limpie el store de auth y redirija a `/login`.

### 9. Activar el router
`AppProviders.tsx`/`App.tsx` no usan `RouterProvider` todavía. Envolver con `RouterProvider client={router}`; mover la demo de health a su propia ruta (p.ej. `/health-demo`) en vez de perderla.

### 10. Rutas de login/registro
`paths.ts`: añadir `login: '/login'`, `register: '/register'`. `router.tsx`: rutas con `AuthLayout` para login/register, y una ruta protegida de ejemplo con `MainLayout` usando el guard del punto 13.

### 11. Tipos y servicio de auth
- `features/auth/types/auth.ts`: `LoginPayload`, `RegisterPayload`, `AuthUser` (`id`, `email`, `displayName`), `AuthResponse` (`token`, `user`).
- `features/auth/services/authApi.ts` (patrón de `features/health/services/healthApi.ts`): `login`, `register`, `logout`, `getMe` contra `auth/login`, `auth/register`, `auth/logout`, `auth/me`.

### 12. Store de auth con persistencia del token
`stores/useAuthStore.ts`, con el middleware `persist` de Zustand (guarda en `localStorage`) para que el token sobreviva a un refresh de página:
```ts
type AuthState = {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  setSession: (token: string, user: AuthUser) => void;
  clear: () => void;
};
```

### 13. Hooks, formularios y guard
- Instalar `react-hook-form` + `zod` + `@hookform/resolvers` para los formularios de login/registro.
- `features/auth/hooks/useLogin.ts` / `useRegister.ts` — `useMutation` de TanStack Query; en `onSuccess` llaman `setSession(token, user)` y navegan a la ruta protegida.
- `features/auth/components/LoginForm.tsx` / `RegisterForm.tsx` — RHF+Zod.
- `features/auth/index.ts` — exports públicos del feature.
- `app/router/ProtectedRoute.tsx` (nuevo) — lee `isAuthenticated` de `useAuthStore` y redirige a `/login` si es falso.

Con el token persistido en `localStorage` vía Zustand `persist`, no hace falta rehidratar contra `auth/me` al montar la app para mantener la sesión visualmente — pero sí conviene una llamada a `auth/me` en el arranque para confirmar que el token persistido todavía es válido en el servidor (por si fue revocado o expiró), y limpiar el store si no lo es.

## Verificación end-to-end

1. Backend compilando y corriendo (`dotnet run` en `EssenceMvp.Mvc`).
2. Frontend corriendo (`pnpm dev`), navegar a `/register`, crear un usuario, confirmar que queda autenticado y se guarda el token en `localStorage`.
3. Recargar la página en una ruta protegida y confirmar que sigue autenticado (token persistido + validación contra `auth/me`).
4. Cerrar sesión desde el frontend, confirmar redirect a `/login`, y confirmar en la tabla `user_session` que la fila quedó con `revoked_at` seteado.
5. Confirmar que reusar el token viejo tras logout da `401` en `auth/me`.
6. Confirmar que golpear un endpoint protegido sin `Authorization` header da `401` limpio (JSON), no un redirect HTML.
