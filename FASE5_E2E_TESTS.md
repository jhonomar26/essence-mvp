# Fase 5: Tests E2E con Playwright

## Resumen

Implementados tests end-to-end para validar flujos críticos del MVP:
- Registro y login
- Creación de proyectos
- Evaluación de Alphas
- Creación de snapshots

## Estructura

```
tests/EssenceMvp.Tests.E2E/
├── EssenceMvpE2ETests.cs       Tests usando fixtures de Playwright
├── PlaywrightFixture.cs         Fixture para browser management
├── EssenceMvp.Tests.E2E.csproj  .NET 8 xUnit + Playwright
└── appsettings.json             Configuración de URLs
```

## Tests Implementados

| Test | Flujo |
|------|-------|
| FullFlow_RegisterLoginCreateProjectEvaluateSnapshot | Completo: registro → login → crear → evaluar → snapshot |
| LoginWithInvalidCredentials | Validar rechazo de credenciales incorrectas |
| CreateProjectTest | Crear proyecto solo (autenticado) |
| EvaluateAlpha | Evaluar un Alpha y verificar cambios |
| CreateAndViewSnapshot | Crear snapshot y verificar en historial |

## Ejecución

### Requisitos
```bash
# Instalar CLI
dotnet tool install -g Microsoft.Playwright.CLI

# Instalar navegadores (una sola vez)
playwright install chromium --with-deps
```

### Ejecutar tests
```bash
cd tests/EssenceMvp.Tests.E2E
dotnet test --logger "console;verbosity=detailed"
```

### Ejecutar test específico
```bash
dotnet test --filter "FullFlow"
```

### Modo headful (ver navegador)
Editar `PlaywrightFixture.cs`:
```csharp
_browser = await _playwright.Chromium.LaunchAsync(
    new BrowserTypeLaunchOptions { Headless = false }  // Ver navegador
);
```

## Configuración

**URL Base:** Configurada en `PlaywrightFixture.cs`
```csharp
public string BaseUrl { get; } = 
    Environment.GetEnvironmentVariable("MVC_BASE_URL") 
    ?? "https://localhost:7089";
```

Cambiar vía variable de entorno:
```bash
set MVC_BASE_URL=http://localhost:5000
dotnet test
```

## Notas de Implementación

- **Timeouts:** 5 segundos por defecto (WaitForURL, WaitForSelector)
- **Headless:** true (modo CI/CD, sin interfaz gráfica)
- **Browsers:** Chromium únicamente
- **Data:** Tests crean usuarios y proyectos nuevos con GUID en email

## Próximos Pasos (Opcional)

- [ ] Tests de API (xUnit + HttpClient)
- [ ] Cobertura de casos de error (validaciones, 403, 500)
- [ ] Performance tests (carga de proyectos grandes)
- [ ] Tests de accesibilidad (WCAG)
