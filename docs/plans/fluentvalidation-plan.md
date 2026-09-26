# FluentValidation en EssenceMvp

## Context

FluentValidation 12.1.1 ya quedó instalado en `EssenceMvp.Application` en el paso anterior. El usuario quiere aplicarlo "donde sea necesario y donde sea conveniente" — está aprendiendo el tema y quiere que quede bien integrado, no un caso aislado.

Hoy la solución no tiene NINGUNA validación de input: ni DataAnnotations, ni checks manuales, ni ModelState. Hay exactamente 3 inputs de usuario (bodies POST) en toda la app:

- `LoginRequest` / `RegisterRequest` (`src/EssenceMvp.Application/Dtos/AuthDtos.cs`) — usados por `AccountController` (`src/EssenceMvp.Mvc/Controllers/AccountController.cs`), que SÍ tiene `[ApiController]`.
- `EvaluateChecklistViewModel` (`src/EssenceMvp.Mvc/Models/EvaluateChecklistViewModel.cs`) — usado por `AlphaEvaluationController` (clase real, vive en el archivo `src/EssenceMvp.Mvc/Controllers/ProjectEvaluationController.cs` — mismatch de nombre, no se toca, fuera de alcance), que NO tiene `[ApiController]` y por tanto no tiene auto-400 de ModelState.

`FluentValidation.AspNetCore` está deprecado, así que no se usa. En su lugar: un único `IAsyncActionFilter` global que resuelve `IValidator<T>` por reflexión para cada argumento de action — patrón documentado oficialmente por FluentValidation como reemplazo. Esto cubre los 3 inputs de hoy con cero boilerplate en los controllers, y cualquier DTO futuro queda cubierto automáticamente con solo agregar su `Validator` (si no hay validator registrado para un tipo, el filtro no hace nada — no rompe nada existente).

No se usa MediatR ni pipeline behaviors (no existen en el proyecto) — se descarta esa vía por ser una abstracción no pedida.

## Cambios

### 1. Validators

**`src/EssenceMvp.Application/Validators/LoginRequestValidator.cs`** (nuevo):
```csharp
using FluentValidation;
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
```
Solo exige presencia en login — la política de longitud pertenece al registro, no a la verificación de credenciales existentes.

**`src/EssenceMvp.Application/Validators/RegisterRequestValidator.cs`** (nuevo):
```csharp
using FluentValidation;
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.DisplayName).MaximumLength(100).When(x => x.DisplayName is not null);
    }
}
```
Mínimo 8 caracteres (piso razonable para MVP educativo, sin regex de complejidad). La unicidad de email sigue en `AuthService.RegisterAsync` (regla de negocio con acceso a datos — no es responsabilidad de FluentValidation).

**`src/EssenceMvp.Mvc/Validators/EvaluateChecklistViewModelValidator.cs`** (nuevo — va en Mvc porque el DTO vive en `Mvc.Models`; Application no debe depender de tipos de Mvc):
```csharp
using FluentValidation;
using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Validators;

public class EvaluateChecklistViewModelValidator : AbstractValidator<EvaluateChecklistViewModel>
{
    public EvaluateChecklistViewModelValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.AlphaId).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty();
        RuleForEach(x => x.Answers).SetValidator(new ChecklistAnswerValidator());
    }

    private class ChecklistAnswerValidator : AbstractValidator<EvaluateChecklistViewModel.ChecklistAnswer>
    {
        public ChecklistAnswerValidator()
        {
            RuleFor(x => x.StateChecklistId).GreaterThan(0);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}
```

### 2. Filtro global de validación

**`src/EssenceMvp.Mvc/Filters/FluentValidationActionFilter.cs`** (nuevo):
```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EssenceMvp.Mvc.Filters;

public class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!context.ModelState.IsValid)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            context.Result = new BadRequestObjectResult(
                factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState));
            return;
        }

        await next();
    }
}
```
Cubre `AccountController` (con `[ApiController]`) y `AlphaEvaluationController` (sin `[ApiController]`) de forma idéntica. Respuesta de error: `ValidationProblemDetails` estándar (RFC 7807) vía `ProblemDetailsFactory` — mismo shape que ASP.NET Core produce automáticamente, sin construir nada custom.

### 3. Registro DI

**`src/EssenceMvp.Application/DependencyInjection.cs`** — agregar dentro de `AddApplication`, junto a los `AddScoped` existentes:
```csharp
services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
```

**`src/EssenceMvp.Mvc/Program.cs`** — reemplazar `builder.Services.AddControllers();` por:
```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationActionFilter>();
});
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateChecklistViewModelValidator>();
```

### 4. .csproj

**`src/EssenceMvp.Mvc/EssenceMvp.Mvc.csproj`** — agregar referencia directa (hoy solo llega transitivo vía `Application`; Mvc usa `FluentValidation` directamente en el filtro y en el validator del ViewModel):
```
dotnet add src/EssenceMvp.Mvc/EssenceMvp.Mvc.csproj package FluentValidation
```

## No se toca

- `AccountController.cs`, `ProjectEvaluationController.cs` (clase `AlphaEvaluationController`) — cero boilerplate agregado en las actions, ese es el punto del filtro global.
- No se agrega `FluentValidation.AspNetCore` (deprecado), ni MediatR/pipeline behaviors, ni interfaces de abstracción extra sobre el filtro.
- `AuthService` — la validación de unicidad de email queda igual (regla de negocio, no de formato).

## Verificación

```bash
dotnet build src/EssenceMvp.Mvc/EssenceMvp.Mvc.csproj
```

Con la app corriendo (`dotnet run --project src/EssenceMvp.Mvc`), probar con curl:

```bash
# Login inválido -> 400 ValidationProblemDetails (errors.Email, errors.Password)
curl -i -X POST http://localhost:PORT/auth/login -H "Content-Type: application/json" -d '{"email":"not-an-email","password":""}'

# Register con password corta -> 400
curl -i -X POST http://localhost:PORT/auth/register -H "Content-Type: application/json" -d '{"email":"new@example.com","password":"123"}'

# Register válido -> 201
curl -i -X POST http://localhost:PORT/auth/register -H "Content-Type: application/json" -d '{"email":"new@example.com","password":"correcthorse123","displayName":"X"}'

# Evaluate con ProjectId=0 y Answers vacío -> 400
curl -i -X POST http://localhost:PORT/evaluation/alpha -H "Content-Type: application/json" -d '{"projectId":0,"alphaId":1,"answers":[]}'
```

Confirmar 400 con body `errors` en los casos inválidos, y que los casos válidos no fallen por validación (pueden dar 401/409/200/201 según lógica de negocio preexistente).
