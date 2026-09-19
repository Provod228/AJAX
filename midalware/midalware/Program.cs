using midalware.Middleware;
using midalware.Services;

namespace midalware;

public class Program
{
    public static void Main(string[] args)
    {
        // ============================================================
        // ЭТАП 1. СОЗДАНИЕ ХОСТА (Host Creation)
        // WebApplication.CreateBuilder загружает:
        //  - конфигурацию (appsettings.json, переменные окружения, secrets)
        //  - логирование (ILogger)
        //  - DI-контейнер (IServiceCollection)
        // Создано из ПУСТОГО шаблона: dotnet new web -n midalware
        // ============================================================
        var builder = WebApplication.CreateBuilder(args);
        Console.WriteLine("[LIFECYCLE 1/6] CreateBuilder: хост создан, конфигурация загружена");

        // ============================================================
        // ЭТАП 2. РЕГИСТРАЦИЯ СЕРВИСОВ (ConfigureServices / DI)
        // Здесь приложение "узнаёт" о своих зависимостях.
        // Порядок: всё, что добавлено в builder.Services, станет доступно
        // через конструкторы (например ILogger<T> в middleware).
        // ============================================================
        builder.Services.AddHostedService<AppLifecycleService>(); // показывает старт/стоп хоста
        builder.Services.AddDirectoryBrowser(); // чтобы было видно файлы (не обязательно)
        Console.WriteLine("[LIFECYCLE 2/6] ConfigureServices: сервисы зарегистрированы");

        // ============================================================
        // ЭТАП 3. СБОРКА ПРИЛОЖЕНИЯ (Build)
        // builder.Build() создаёт WebApplication: финализирует DI,
        // собирает конфигурацию и готовит конвейер к настройке.
        // После Build() добавлять сервисы уже НЕЛЬЗЯ.
        // ============================================================
        var app = builder.Build();
        Console.WriteLine("[LIFECYCLE 3/6] Build: WebApplication собран");

        // Показываем окружение — часть жизненного цикла (Development / Production)
        Console.WriteLine($"[ENV] Environment: {app.Environment.EnvironmentName}");
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); // подробные ошибки только в Development
        }

        // ============================================================
        // ЭТАП 4. КОНФИГУРАЦИЯ КОНВЕЙЕРА (Configure Middleware Pipeline)
        // !!! САМЫЙ ВАЖНЫЙ ЭТАП ДЛЯ ДЗ !!!
        // Каждый Use* добавляет "слой луковицы". Запрос идёт сверху вниз,
        // ответ — снизу вверх. Порядок Use* = порядок выполнения!
        //
        // Схема нашего конвейера:
        //   Request -> [1-LOG] -> [2-TIME] -> [FORM-VALIDATION] -> [STAGE] -> Endpoint
        //   Response <- [1-LOG] <- [2-TIME] <- (short-circuit?)       <- Endpoint
        // ============================================================
        Console.WriteLine("[LIFECYCLE 4/6] Configure: строим middleware-конвейер");

        // Статика для формы (wwwroot/index.html, css). Должна быть раньше остального,
        // чтобы index.html отдавался без лишней обработки.
        app.UseDefaultFiles(); // ищет index.html автоматически
        app.UseStaticFiles();

        app.UseRequestLogging();    // №1: логирует вход/выход КАЖДОГО запроса
        app.UseRequestTiming();     // №2: замеряет время выполнения
        app.UseFormValidation();    // №3: ПРИВЯЗАН К ФОРМЕ (POST /submit), может остановить конвейер
        app.UseLifecycleStage("pre-endpoint"); // №4: маркер-заголовки для Network-вкладки

        // Встроенный терминальный пример: просто пишет в консоль (необязательный слой)
        app.Use(async (context, next) =>
        {
            Console.WriteLine($"[INLINE] {context.Request.Method} {context.Request.Path} проходит inline-middleware");
            await next(context);
            Console.WriteLine($"[INLINE] {context.Request.Method} {context.Request.Path} возвращается обратно");
        });

        // ============================================================
        // ЭТАП 5. ENDPOINTS (конечные точки — терминальные middleware)
        // Endpoint НЕ вызывает next — он формирует окончательный ответ.
        // Если FormValidationMiddleware сделал short-circuit, эти endpoint'ы
        // для POST /submit вызваны НЕ будут!
        // ============================================================

        // GET / — отдаём форму (дублирует StaticFiles, но гарантирует ответ даже без wwwroot)
        app.MapGet("/", () => Results.Redirect("/index.html"));

        // GET /lifecycle — страница-схема жизненного цикла (для скринов в ДЗ)
        app.MapGet("/lifecycle", () => Results.Redirect("/lifecycle.html"));

        // GET /health — проверка, что приложение живо (удобно для скрина)
        app.MapGet("/health", () => Results.Json(new
        {
            status = "OK",
            app = "midalware",
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            lifecycle = "CreateBuilder -> Services -> Build -> Pipeline -> Run -> Request pipeline"
        }));

        // POST /submit — приём ФОРМЫ. Сюда запрос попадает ТОЛЬКО если
        // FormValidationMiddleware пропустил его (валидация пройдена).
        app.MapPost("/submit", (HttpContext context) =>
        {
            Console.WriteLine("[ENDPOINT] >>> POST /submit ДОШЁЛ до endpoint (валидация пройдена)");

            var name = context.Items["form.name"]?.ToString() ?? "(нет)";
            var email = context.Items["form.email"]?.ToString() ?? "(нет)";
            var message = context.Items["form.message"]?.ToString() ?? "(нет)";

            // Экранируем, чтобы не было XSS
            Func<string?, string?> safe = System.Net.WebUtility.HtmlEncode;
            return Results.Content($"""
                <!DOCTYPE html>
                <html lang="ru"><head><meta charset="utf-8"><title>Успешно</title>
                <link rel="stylesheet" href="/css/style.css"></head>
                <body><div class="card success">
                <h2>✅ Форма прошла весь конвейер!</h2>
                <table>
                  <tr><td>Имя:</td><td><b>{safe(name)}</b></td></tr>
                  <tr><td>E-mail:</td><td><b>{safe(email)}</b></td></tr>
                  <tr><td>Сообщение:</td><td>{safe(message)}</td></tr>
                </table>
                <h3>Пройденный жизненный цикл запроса:</h3>
                <ol>
                  <li>RequestLoggingMiddleware — залогировал вход/выход</li>
                  <li>RequestTimingMiddleware — замерил время (см. заголовок X-Response-Time-ms)</li>
                  <li>FormValidationMiddleware — провалидировал и пропустил дальше</li>
                  <li>LifecycleHeadersMiddleware — добавил X-Lifecycle-Stage</li>
                  <li>Endpoint POST /submit — сформировал эту страницу</li>
                </ol>
                <a class="btn" href="/">← Назад к форме</a>
                <a class="btn secondary" href="/lifecycle.html">Схема жизненного цикла</a>
                </div></body></html>
                """, "text/html; charset=utf-8");
        });

        // ============================================================
        // ЭТАП 6. ЗАПУСК (Run — Kestrel слушает порт)
        // app.Run() блокирует поток, запускает сервер и
        // вызывает IHostedService.StartAsync (наш AppLifecycleService).
        // При Ctrl+C вызывается StopAsync — конец жизненного цикла.
        // ============================================================
        Console.WriteLine("[LIFECYCLE 5/6] Endpoints зарегистрированы (/, /submit, /health, /lifecycle)");
        Console.WriteLine("[LIFECYCLE 6/6] Run: сервер запускается. Откройте http://localhost:5076");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("ДЛЯ СКРИНОВ ДЗ:");
        Console.WriteLine("  1. Форма:            http://localhost:5076/");
        Console.WriteLine("  2. Схема lifecycle:  http://localhost:5076/lifecycle.html");
        Console.WriteLine("  3. Health-check:     http://localhost:5076/health");
        Console.WriteLine("--------------------------------------------------");

        app.Run();
    }
}
