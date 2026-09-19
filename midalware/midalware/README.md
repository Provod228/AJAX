# midalware — Форма + Middleware + Жизненный цикл (Пустой шаблон)

Учебный проект ASP.NET Core (.NET 8), созданный из **пустого шаблона** (`dotnet new web`).
Демонстрирует: форму, привязанный к ней middleware и **весь жизненный цикл приложения и запроса**.

## 🚀 Быстрый старт

```bash
dotnet restore
dotnet run
```

Открыть:
- Форма: http://localhost:5076/
- Схема жизненного цикла: http://localhost:5076/lifecycle.html
- Health-check: http://localhost:5076/health

## 📁 Структура

```
midalware/
├── Program.cs                  # Этапы 1-6 жизненного цикла (CreateBuilder → Run)
├── Middleware/
│   ├── RequestLoggingMiddleware.cs   # №1 — лог входа/выхода, X-Logged-By
│   ├── RequestTimingMiddleware.cs    # №2 — Stopwatch, X-Response-Time-ms
│   ├── FormValidationMiddleware.cs   # №3 — ПРИВЯЗАН К ФОРМЕ (POST /submit)
│   ├── LifecycleHeadersMiddleware.cs # №4 — X-Lifecycle-Stage
│   └── MiddlewareExtensions.cs       # Use*() расширения
├── Models/FeedbackForm.cs
├── Services/AppLifecycleService.cs   # IHostedService: старт/стоп хоста
├── wwwroot/
│   ├── index.html              # ФОРМА
│   ├── lifecycle.html          # Страница-схема lifecycle (для скрина)
│   └── css/style.css
├── LIFECYCLE.md                # Полное описание жизненного цикла + коды
├── docs/
│   ├── SCREENSHOTS_GUIDE.md    # Какие скрины положить в ДЗ
│   └── screenshots/            # ← сюда положить PNG скрины
└── appsettings.json
```

## 🧪 Сценарии для защиты ДЗ

1. **Невалидная форма** (пустое имя / email без @ / короткое сообщение) →
   `400` от `FormValidationMiddleware`, конвейер **коротко замкнут**, endpoint не вызван.
2. **Валидная форма** → проходит все 4 middleware → `POST /submit` возвращает страницу успеха.
3. **F12 → Network** → заголовки `X-Logged-By`, `X-Response-Time-ms`, `X-Lifecycle-Stage`.
4. **Консоль сервера** → строки `[LIFECYCLE 1/6]…[6/6]`, `[1-LOG]`, `[2-TIME]`, `[3-FORM]`, `[ENDPOINT]`.

Подробно: см. `LIFECYCLE.md`.
