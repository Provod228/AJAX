# Жизненный цикл приложения midalware (для ДЗ)

## 0. Как создан проект (пустой шаблон)

```bash
dotnet new web -n midalware   # именно пустой шаблон, без MVC/Razor
dotnet run
```

Пустой шаблон даёт только `Program.cs` + Kestrel. Всё остальное (форма, middleware) дописано вручную —
это и требуется по заданию.

---

## 1. Жизненный цикл ПРИЛОЖЕНИЯ (хоста) — 6 этапов

| № | Этап | Код (Program.cs) | Что происходит |
|---|------|------------------|----------------|
| 1 | CreateBuilder | `WebApplication.CreateBuilder(args)` | Загрузка `appsettings.json`, env-переменных, логгера, DI-контейнера |
| 2 | ConfigureServices | `builder.Services.AddHostedService<AppLifecycleService>()` | Регистрация зависимостей. После Build добавлять нельзя |
| 3 | Build | `var app = builder.Build()` | Финализация DI, создание `WebApplication` |
| 4 | Configure Pipeline | `app.UseRequestLogging(); app.UseRequestTiming(); app.UseFormValidation(); ...` | Сборка «луковицы» middleware. **Порядок Use = порядок выполнения** |
| 5 | Endpoints | `app.MapGet("/", ...)`, `app.MapPost("/submit", ...)` | Терминальные точки. Формируют ответ, `next` не вызывают |
| 6 | Run / Stop | `app.Run()` → `StartAsync`; Ctrl+C → `StopAsync` | Kestrel слушает порт (5076/7045). `AppLifecycleService` печатает старт/стоп |

Консоль при старте (что скринить):

```
[LIFECYCLE 1/6] CreateBuilder: хост создан, конфигурация загружена
[LIFECYCLE 2/6] ConfigureServices: сервисы зарегистрированы
[LIFECYCLE 3/6] Build: WebApplication собран
[HOST] >>> Приложение СТАРТУЕТ (IHostedService.StartAsync)
[LIFECYCLE 4/6] Configure: строим middleware-конвейер
[LIFECYCLE 5/6] Endpoints зарегистрированы (/, /submit, /health, /lifecycle)
[LIFECYCLE 6/6] Run: сервер запускается. Откройте http://localhost:5076
```

---

## 2. Жизненный цикл ЗАПРОСА (конвейер) — 5 слоёв

```
Request (GET / или POST /submit)
  → [1] RequestLoggingMiddleware  (вход-лог → next → выход-лог + X-Logged-By)
  → [2] RequestTimingMiddleware   (Stopwatch → next → X-Response-Time-ms)
  → [3] FormValidationMiddleware  (только POST /submit: валидация;
  │                                 ошибка → 400 short-circuit;
  │                                 ок → Items + next)
  → [4] LifecycleHeadersMiddleware (X-Lifecycle-Stage, X-App)
  → [5] Endpoint (StaticFiles / MapPost /submit)
  ← Response идёт обратно вверх → браузер
```

### Код конвейера (Program.cs, этап 4):

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRequestLogging();                 // №1
app.UseRequestTiming();                  // №2
app.UseFormValidation();                 // №3 — привязан к форме
app.UseLifecycleStage("pre-endpoint");   // №4

app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapGet("/health", () => Results.Json(new { status = "OK" }));
app.MapPost("/submit", (HttpContext context) => { /* ... */ });
```

### Привязка middleware к форме ( FormValidationMiddleware.cs ):

```csharp
if (context.Request.Method == "POST" && context.Request.Path == "/submit")
{
    var form = await context.Request.ReadFormAsync();
    // проверка name/email/message...
    if (errors.Count > 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("...ошибка...");
        return; // ← SHORT-CIRCUIT: _next НЕ вызывается, endpoint не выполнится
    }
    context.Items["form.name"] = name; // передача данных дальше по запросу
}
await _next(context);
```

### Endpoint приёма формы (вызывается ТОЛЬКО после валидации):

```csharp
app.MapPost("/submit", (HttpContext context) =>
{
    var name = context.Items["form.name"]?.ToString();
    // ...формируем HTML успеха...
    return Results.Content(html, "text/html; charset=utf-8");
});
```

---

## 3. Short-circuit — главный термин для защиты

> Middleware может **не вызвать** `_next` и само сформировать ответ.
> Тогда остаток конвейера (включая endpoint) **не выполняется**.

В проекте это делает `FormValidationMiddleware` при невалидной форме (ответ 400).
Преподавателю показать: отправка плохой формы → страница «Форма отклонена middleware»,
в консоли строка `[3-FORM] !!! Валидация НЕ пройдена — короткое замыкание конвейера`.

---

## 4. Какие файлы — какой этап показывают

- `Program.cs` — все 6 этапов хоста + регистрация конвейера и endpoints.
- `Services/AppLifecycleService.cs` — Start/Stop хоста (начало и конец жизни).
- `Middleware/RequestLoggingMiddleware.cs` — вход/выход запроса («луковица»).
- `Middleware/RequestTimingMiddleware.cs` — пост-обработка ответа.
- `Middleware/FormValidationMiddleware.cs` — привязка к форме + short-circuit.
- `wwwroot/index.html` — форма (`POST /submit`: name, email, message).
- `wwwroot/lifecycle.html` — визуальная схема (скрин для ДЗ).

## 5. Заголовки-маркеры (доказательство прохождения конвейера)

| Заголовок | Кто ставит | Как проверить |
|-----------|------------|---------------|
| `X-Logged-By: RequestLoggingMiddleware` | №1 | F12 → Network → /submit → Headers |
| `X-Response-Time-ms: 3` | №2 | там же |
| `X-Lifecycle-Stage: pre-endpoint` | №4 | там же |
| `X-App: midalware-lifecycle-demo` | №4 | там же |
