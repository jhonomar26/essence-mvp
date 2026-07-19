# Scaffolding de frontend React (feature-based) para EssenceMvp

## Contexto

EssenceMvp es hoy una app ASP.NET Core 8 MVC que renderiza todo con Razor Views. El objetivo es migrar el frontend a React, de forma gradual e iterativa, usando una arquitectura **feature-based** (cada funcionalidad con sus propios `components/hooks/services/types`, como en apps tipo "tickets"). Se pidió explícitamente usar el stack más popular y probado de la industria, sin inventar nada experimental.

Este documento cubre **solo el andamiaje inicial**: crear el proyecto React, la estructura de carpetas vacía, y dejar una única llamada de prueba funcionando contra el backend real. No migra ninguna vista Razor ni lógica de negocio todavía — eso vendrá en iteraciones futuras.

## Hallazgos clave del backend (ya verificados)

- Puertos reales (`Properties/launchSettings.json`): **http `localhost:5070`**, **https `localhost:7170`** (el README tiene el dato viejo, 5140/7197 — no usar esos).
- Pipeline actual (`Program.cs:46-52`): `UseHttpsRedirection → UseStaticFiles → UseRouting → UseAuthentication → UseAuthorization → MapControllerRoute`. **No hay `AddCors`/`UseCors` en ningún lado.**
- Auth: cookie (`.AspNetCore.Cookies`, sliding, 8h). `LoginPath`/`AccessDeniedPath` apuntan a `/Account/Login` y devuelven **302 a HTML**, no 401 — rompe fetch/axios si se llama a un endpoint protegido sin ajustar esto (se deja como deuda documentada, no se toca ahora).
- Controllers que YA devuelven JSON: `ProjectSnapshotController` (`[ApiController]`, `[Authorize]` — requiere cookie), `ProjectHealthController` (`GET /evaluation/health/{projectId}`, **sin `[Authorize]`** → es el candidato ideal para la primera petición de prueba, no requiere resolver el tema de auth), `AlphaEvaluationController`/`ProjectEvaluationController` (JSON, sin `[ApiController]` formal).
- `AccountController`, `HomeController`, `ProjectsController` devuelven Razor Views — quedan fuera de alcance en este plan.
- No existe ningún proyecto frontend, `package.json`, ni carpeta `/frontend` — punto de partida limpio.

## Stack recomendado (decidido, probado en producción, sin alternativas abiertas)

| Pieza | Elección | Por qué |
|---|---|---|
| Bundler/scaffolding | **Vite** (`react-ts` template) | Estándar de facto para SPAs React; usado por defecto en docs oficiales de React, TanStack, Zustand. |
| Lenguaje | **TypeScript** | Ya existen DTOs tipados en C# (`ProjectSummary`, `HealthResult`, etc.); TS replica ese contrato en el cliente. |
| Data fetching (server state) | **TanStack Query v5** | Adopción muy superior a SWR, ecosistema más grande (devtools, mutations), es el estándar en boilerplates feature-based con mutaciones complejas. |
| Estado global (client state) | **Zustand** | Librería de estado más adoptada tras Redux, sin boilerplate de providers. Se usa junto a TanStack Query separando "server state" (Query) de "client state" (Zustand: sesión, UI). |
| Cliente HTTP | **Axios** | Necesita interceptors para fijar `withCredentials: true` centralizado y manejar el 302/401 de la cookie de auth — fetch nativo no trae interceptors. |
| Routing | **react-router-dom v6** | API estable, años en producción, sin la complejidad de "framework mode" de v7/Remix que no se necesita para una SPA que solo consume una API. |
| Estilos | **Tailwind CSS v4** (`@tailwindcss/vite`) | Framework de utilidades más popular; v4 se integra nativamente con Vite sin config extra. |
| Formularios | **React Hook Form + Zod** — decisión tomada, instalación **diferida** | Se define ahora para no re-discutirlo, pero no se instala hasta migrar el primer formulario real (candidato: login). |
| Testing (futuro, no ahora) | Vitest + React Testing Library | Pairing estándar de Vite, se deja como nota. |
| Linting | ESLint + typescript-eslint del template de Vite | No se reinventa nada. |

**Referencia de arquitectura probada**: [bulletproof-react](https://github.com/alan2207/bulletproof-react) — repo de referencia de la comunidad (miles de estrellas) que documenta exactamente el patrón feature-based con `shared/` transversal. La estructura de abajo está inspirada directamente en él.

## Estructura de carpetas

Nuevo proyecto `frontend/` en la raíz del repo, junto a `src/`:

```
essence-mvp/
└── frontend/
    ├── index.html
    ├── package.json
    ├── vite.config.ts
    ├── tsconfig.json
    ├── .env.development          # VITE_API_BASE_URL=http://localhost:5070
    ├── .env.example
    └── src/
        ├── main.tsx
        ├── App.tsx
        ├── index.css                       # @import "tailwindcss";
        │
        ├── app/                             # arranque/composición, sin lógica de negocio
        │   ├── providers/AppProviders.tsx   # QueryClientProvider + RouterProvider
        │   ├── router/{router.tsx,paths.ts}
        │   └── layout/{MainLayout.tsx,AuthLayout.tsx}
        │
        ├── features/                        # una carpeta por feature, mapeada a un controller
        │   ├── auth/          -> AccountController (futuro)
        │   ├── projects/      -> ProjectsController (futuro)
        │   ├── evaluation/    -> ProjectEvaluationController / AlphaEvaluationController (futuro)
        │   ├── health/        -> ProjectHealthController (PRIMERA llamada real, sin auth)
        │   └── snapshots/     -> ProjectSnapshotController (futuro, requiere auth)
        │       └── (cada feature: components/ hooks/ services/ types/ index.ts)
        │
        ├── shared/
        │   ├── api/client.ts                # instancia Axios, baseURL desde env, withCredentials: true
        │   ├── components/{ui,feedback}
        │   ├── lib/queryClient.ts
        │   └── types/api.ts
        │
        └── stores/
            └── useAuthStore.ts               # placeholder Zustand, sin lógica real todavía
```

Solo `features/health/services/healthApi.ts` tendrá una llamada real (`GET /evaluation/health/{projectId}`), porque es el único endpoint JSON sin `[Authorize]`. El resto de features arrancan con placeholders (`.gitkeep`, `export {}`).

## Pasos de implementación

1. **Scaffold Vite**: `npm create vite@latest frontend -- --template react-ts` en la raíz del repo, luego `npm install`.
2. **Dependencias de producción**: `axios@^1`, `@tanstack/react-query@^5`, `zustand@^5`, `react-router-dom@^6`.
3. **Tailwind v4**: `npm install -D tailwindcss@^4 @tailwindcss/vite`; agregar el plugin a `vite.config.ts` y reemplazar `src/index.css` por `@import "tailwindcss";`.
4. **Variables de entorno**: crear `.env.development` y `.env.example` con `VITE_API_BASE_URL=http://localhost:5070`.
5. **Crear árbol de carpetas** descrito arriba con placeholders mínimos (`index.ts` con `export {};`, `.gitkeep` en carpetas vacías).
6. **`shared/api/client.ts`**: instancia de Axios con `baseURL: import.meta.env.VITE_API_BASE_URL` y `withCredentials: true`.
7. **`shared/lib/queryClient.ts`** + **`app/providers/AppProviders.tsx`**: envolver `App` con `QueryClientProvider`.
8. **`features/health/services/healthApi.ts`**: función `getProjectHealth(projectId)` que llama a `GET /evaluation/health/{projectId}` vía el cliente Axios — usada desde `App.tsx` o una página placeholder solo para verificar la conexión end-to-end.
9. **Backend — CORS** (`src/EssenceMvp.Mvc/Program.cs`): agregar política nombrada con origen exacto del dev server de Vite (`http://localhost:5173`), `AllowAnyHeader()`, `AllowAnyMethod()`, `AllowCredentials()` (obligatorio por la cookie; incompatible con `AllowAnyOrigin()`). Insertar `app.UseCors("FrontendDev")` entre `app.UseRouting()` y `app.UseAuthentication()` (línea 48-49 actual).
10. **Verificar**: correr el backend (`dotnet run` en `src/EssenceMvp.Mvc`) y el frontend (`npm run dev` en `frontend/`), confirmar que la llamada de prueba a `/evaluation/health/{id}` responde 200 con datos reales (usando un `projectId` existente en la BD) sin error de CORS en consola.

## Qué NO hacer todavía

- No migrar ninguna Razor View a componentes React.
- No tocar `AccountController`, `HomeController`, `ProjectsController` (HTML) — fuera de alcance.
- No instalar React Hook Form / Zod ni construir formularios reales.
- No resolver el 302-vs-401 de `OnRedirectToLogin` — queda documentado como deuda para cuando se consuma un endpoint con `[Authorize]`.
- No conectar `projects`, `evaluation`, `auth`, `snapshots` a llamadas reales — solo `health`.

## Verificación

- `npm run dev` en `frontend/` levanta en `http://localhost:5173` sin errores.
- `dotnet run` en `src/EssenceMvp.Mvc` levanta en `http://localhost:5070`.
- La llamada de prueba desde React a `GET /evaluation/health/{projectId}` (con un ID real de la BD) devuelve 200 y los datos se ven en consola/UI, sin bloqueo de CORS.
- La estructura de carpetas de `features/` y `shared/` existe completa, con placeholders, y el proyecto compila (`npm run build`) sin errores de TypeScript.
