namespace midalware.Middleware;

/// <summary>
/// ЭТАП ЖИЗНЕННОГО ЦИКЛА ЗАПРОСА №3 — Привязка к ФОРМЕ.
/// Этот middleware перехватывает ТОЛЬКО POST /submit (отправка формы),
/// валидирует поля и может КОРОТКО ЗАМКНУТЬ конвейер (не вызвать _next).
///
/// Это главная демонстрация для ДЗ: "форма + привязанный middleware".
/// </summary>
public class FormValidationMiddleware
{
    private readonly RequestDelegate _next;

    public FormValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Работаем только с отправкой формы, остальные запросы пропускаем дальше
        if (context.Request.Method == "POST" &&
            context.Request.Path.Equals("/submit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[3-FORM] >>> Перехвачена отправка формы /submit");

            var form = await context.Request.ReadFormAsync();
            var name = form["name"].ToString().Trim();
            var email = form["email"].ToString().Trim();
            var message = form["message"].ToString().Trim();

            Console.WriteLine($"[3-FORM] Данные: name='{name}', email='{email}', message_len={message.Length}");

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                errors.Add("Имя должно содержать минимум 2 символа.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                errors.Add("Введите корректный e-mail (должен содержать @).");
            if (string.IsNullOrWhiteSpace(message) || message.Length < 5)
                errors.Add("Сообщение должно содержать минимум 5 символов.");

            if (errors.Count > 0)
            {
                // КОРОТКОЕ ЗАМЫКАНИЕ: дальше по конвейеру НЕ идём, сами формируем ответ.
                // Endpoint /submit вызван НЕ будет — это и есть short-circuit в жизненном цикле.
                Console.WriteLine("[3-FORM] !!! Валидация НЕ пройдена — короткое замыкание конвейера");
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/html; charset=utf-8";

                var errorItems = string.Join("", errors.Select(e => $"<li>{System.Net.WebUtility.HtmlEncode(e)}</li>"));
                await context.Response.WriteAsync($"""
                    <!DOCTYPE html>
                    <html lang="ru"><head><meta charset="utf-8"><title>Ошибка формы</title>
                    <link rel="stylesheet" href="/css/style.css"></head>
                    <body><div class="card error">
                    <h2>❌ Форма отклонена middleware</h2>
                    <p><b>FormValidationMiddleware</b> остановил конвейер (short-circuit):</p>
                    <ul>{errorItems}</ul>
                    <a class="btn" href="/">← Вернуться к форме</a>
                    <p class="hint">Проверьте заголовки ответа: запрос НЕ дошёл до endpoint.</p>
                    </div></body></html>
                    """);
                return;
            }

            // Валидация пройдена — кладём данные в Items, чтобы endpoint мог их забрать.
            // HttpContext.Items живёт только в пределах ОДНОГО запроса (часть жизненного цикла запроса).
            Console.WriteLine("[3-FORM] Валидация пройдена — передаём дальше по конвейеру");
            context.Items["form.name"] = name;
            context.Items["form.email"] = email;
            context.Items["form.message"] = message;
            context.Items["form.validated"] = true;
        }

        await _next(context);
    }
}
