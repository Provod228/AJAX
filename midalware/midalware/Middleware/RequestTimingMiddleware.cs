using System.Diagnostics;

namespace midalware.Middleware;

/// <summary>
/// ЭТАП ЖИЗНЕННОГО ЦИКЛА ЗАПРОСА №2 — Замер времени.
/// Демонстрирует работу с HttpContext.Response после вызова _next
/// и добавление заголовка X-Response-Time-ms.
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("[2-TIME] Таймер запущен");

        // Регистрируем callback ДО вызова _next: он сработает перед отправкой
        // заголовков (т.е. после endpoint, но пока заголовки ещё можно менять).
        // Так X-Response-Time-ms гарантированно попадёт в ответ.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-ms"] = sw.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });

        // Вызываем следующий компонент конвейера
        await _next(context);

        sw.Stop();
        Console.WriteLine($"[2-TIME] Запрос {context.Request.Path} выполнен за {sw.ElapsedMilliseconds} мс");
    }
}
