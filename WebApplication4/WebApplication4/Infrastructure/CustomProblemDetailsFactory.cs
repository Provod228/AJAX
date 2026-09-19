using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication4.Infrastructure;

/// <summary>
/// Кастомизация ProblemDetails из конспекта: к каждой ошибке
/// добавляем traceId и timestamp (в Development — ещё и окружение).
/// </summary>
public sealed class CustomProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly IHostEnvironment _env;

    public CustomProblemDetailsFactory(IHostEnvironment env) => _env = env;

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext, int? statusCode = null,
        string? title = null, string? type = null,
        string? detail = null, string? instance = null)
    {
        var pd = new ProblemDetails
        {
            Status = statusCode ?? 500,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path.Value
        };
        Enrich(pd, httpContext);
        return pd;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext, ModelStateDictionary modelStateDictionary,
        int? statusCode = null, string? title = null, string? type = null,
        string? detail = null, string? instance = null)
    {
        var vpd = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode ?? 400,
            Title = title ?? "Validation failed",
            Type = type,
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path.Value
        };
        Enrich(vpd, httpContext);
        return vpd;
    }

    private void Enrich(ProblemDetails pd, HttpContext ctx)
    {
        pd.Extensions["traceId"] = ctx.TraceIdentifier;
        pd.Extensions["timestamp"] = DateTime.UtcNow;
        if (_env.IsDevelopment())
            pd.Extensions["environment"] = "Development";
    }
}
