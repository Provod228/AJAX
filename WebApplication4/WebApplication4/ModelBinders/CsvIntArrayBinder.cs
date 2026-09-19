using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication4.ModelBinders;

/// <summary>
/// Кастомный биндер из конспекта: ?ids=1,2,3 → int[].
/// Не число ("abc") → ошибка в ModelState → [ApiController] вернёт 400.
/// </summary>
public sealed class CsvIntArrayBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        var raw = ctx.ValueProvider.GetValue(ctx.ModelName).FirstValue;
        if (string.IsNullOrWhiteSpace(raw))
        {
            ctx.Result = ModelBindingResult.Success(Array.Empty<int>());
            return Task.CompletedTask;
        }

        var list = new List<int>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, out var value))
            {
                ctx.ModelState.TryAddModelError(ctx.ModelName, $"Не число: '{part}'");
                ctx.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            list.Add(value);
        }

        ctx.Result = ModelBindingResult.Success(list.ToArray());
        return Task.CompletedTask;
    }
}
