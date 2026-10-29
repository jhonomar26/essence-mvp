using EssenceMvp.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Errors;

// Punto único donde las excepciones se traducen a respuestas HTTP (ADR-0001).
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public ApiExceptionHandler(IProblemDetailsService problemDetails) => _problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception is NotFoundException
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                // El mensaje interno solo se expone para "no encontrado"; un 500 va genérico.
                Title = status == StatusCodes.Status404NotFound ? exception.Message : "An unexpected error occurred."
            }
        });
    }
}
