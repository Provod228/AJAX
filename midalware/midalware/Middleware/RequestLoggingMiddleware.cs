namespace midalware.Middleware;

/// <summary>
/// ЭТАП ЖИЗНЕННОГО ЦИКЛА ЗАПРОСА №1 — Логирование.
/// Первый middleware в конвейере. Видит ВСЕ запросы (и к форме, и к /submit).
/// Демонстрирует: вход в конвейер, передача дальше через _next, выход из конвейера.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // --- ВХОД: запрос только пришёл в конвейер ---
        var startTime = DateTime.Now;
        Console.WriteLine($"[1-LOG] >>> ВХОД: {context.Request.Method} {context.Request.Path} в {startTime:HH:mm:ss.fff}");

        _logger.LogInformation("Входящий запрос: {Method} {Path} от {IP}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        // Добавляем заголовок-маркер, что запрос прошёл этот middleware
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Logged-By"] = "RequestLoggingMiddleware";
            return Task.CompletedTask;
        });

        // Передаём управление СЛЕДУЮЩЕМУ middleware (ключевой момент жизненного цикла!)
        await _next(context);

        // --- ВЫХОД: ответ уже сформирован, идём обратно по цепочке ---
        var elapsed = DateTime.Now - startTime;
        Console.WriteLine($"[1-LOG] <<< ВЫХОД: {context.Request.Path} -> {context.Response.StatusCode} за {elapsed.TotalMilliseconds:F1} мс");
    }
}
