using EssenceMvp.Application.Exceptions;
using EssenceMvp.Mvc.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EssenceMvp.Tests.Integration;

// Spec: migracion-net10 (H2). Desde ASP.NET Core 10 un handler que devuelve true ya no deja
// rastro en el log, así que los 500 se registran dentro del propio handler.
public class ApiExceptionHandlerTests
{
    private sealed class CapturingLogger : ILogger<ApiExceptionHandler>
    {
        public List<(LogLevel Level, Exception? Error)> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> fmt)
            => Entries.Add((level, error));
    }

    private sealed class NoopProblemDetails : Microsoft.AspNetCore.Http.IProblemDetailsService
    {
        private readonly bool _canWrite;
        public NoopProblemDetails(bool canWrite = true) => _canWrite = canWrite;
        public ValueTask WriteAsync(ProblemDetailsContext context) => ValueTask.CompletedTask;
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context) => ValueTask.FromResult(_canWrite);
    }

    private static async Task<(int Status, bool Handled, CapturingLogger Log)> HandleAsync(Exception error, bool canWrite = true)
    {
        var log = new CapturingLogger();
        var handler = new ApiExceptionHandler(new NoopProblemDetails(canWrite), log);
        var context = new DefaultHttpContext();
        var handled = await handler.TryHandleAsync(context, error, CancellationToken.None);
        return (context.Response.StatusCode, handled, log);
    }

    [Fact]
    public async Task Unexpected_error_is_500_and_logged()
    {
        var error = new InvalidOperationException("boom");
        var (status, _, log) = await HandleAsync(error);

        Assert.Equal(500, status);
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Error && e.Error == error);
    }

    [Fact]
    public async Task Not_found_is_404_and_not_logged_as_error()
    {
        var (status, _, log) = await HandleAsync(new NotFoundException("x"));

        Assert.Equal(404, status);
        Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task When_the_response_cannot_be_written_the_handler_does_not_log_and_defers_to_the_middleware()
    {
        // Devuelve false: el middleware registrará la excepción; si el handler también lo hiciera, quedaría duplicada.
        var (_, handled, log) = await HandleAsync(new InvalidOperationException("boom"), canWrite: false);

        Assert.False(handled);
        Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Error);
    }
}
