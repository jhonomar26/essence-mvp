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
        public ValueTask WriteAsync(ProblemDetailsContext context) => ValueTask.CompletedTask;
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context) => ValueTask.FromResult(true);
    }

    private static async Task<(int Status, CapturingLogger Log)> HandleAsync(Exception error)
    {
        var log = new CapturingLogger();
        var handler = new ApiExceptionHandler(new NoopProblemDetails(), log);
        var context = new DefaultHttpContext();
        var handled = await handler.TryHandleAsync(context, error, CancellationToken.None);
        Assert.True(handled);
        return (context.Response.StatusCode, log);
    }

    [Fact]
    public async Task Unexpected_error_is_500_and_logged()
    {
        var error = new InvalidOperationException("boom");
        var (status, log) = await HandleAsync(error);

        Assert.Equal(500, status);
        Assert.Contains(log.Entries, e => e.Level == LogLevel.Error && e.Error == error);
    }

    [Fact]
    public async Task Not_found_is_404_and_not_logged_as_error()
    {
        var (status, log) = await HandleAsync(new NotFoundException("x"));

        Assert.Equal(404, status);
        Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Error);
    }
}
