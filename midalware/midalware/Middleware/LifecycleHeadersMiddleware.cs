namespace midalware.Middleware;

/// <summary>
/// ЭТАП ЖИЗНЕННОГО ЦИКЛА ЗАПРОСА №4 — Маркер жизненного цикла.
/// Добавляет заголовки, по которым видно порядок прохождения конвейера.
/// Удобно показывать преподавателю во вкладке Network (F12).
/// </summary>
public class LifecycleHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _stage;

    public LifecycleHeadersMiddleware(RequestDelegate next, string stage)
    {
        _next = next;
        _stage = stage;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"[4-HEAD:{_stage}] проход '{_stage}'");
        context.Items[$"stage.{_stage}"] = DateTime.Now.ToString("HH:mm:ss.fff");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Lifecycle-Stage"] = _stage;
            context.Response.Headers["X-App"] = "midalware-lifecycle-demo";
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
