using System;

namespace TodoApiV2.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Logica pre-elaborazione
        _logger.LogInformation("Request iniziata: {context.Request.Path}", context.Request.Path);
        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Chiamata al middleware successivo nella pipeline
            await _next(context);
        }
        finally
        {
            watch.Stop();
            _logger.LogInformation("Request completata in {watch.ElapsedMilliseconds}ms", watch.ElapsedMilliseconds);
        }
    }
}