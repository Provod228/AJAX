namespace midalware.Middleware;

/// <summary>
/// Extension-методы для красивого подключения в Program.cs:
/// app.UseRequestLogging(); и т.д.
/// Так принято оформлять middleware в реальных проектах.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();

    public static IApplicationBuilder UseFormValidation(this IApplicationBuilder app)
        => app.UseMiddleware<FormValidationMiddleware>();

    public static IApplicationBuilder UseLifecycleStage(this IApplicationBuilder app, string stage)
        => app.UseMiddleware<LifecycleHeadersMiddleware>(stage);
}
