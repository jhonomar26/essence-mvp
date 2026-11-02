using EssenceMvp.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Errors;

// Punto único donde las excepciones se traducen a respuestas HTTP (ADR-0001).
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception is NotFoundException
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = status;

        var written = await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status == StatusCodes.Status404NotFound ? exception.Message : "An unexpected error occurred."
            }
        });

   
        if (written && status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

        return written;
    }
}
