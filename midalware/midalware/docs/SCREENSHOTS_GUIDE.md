# Гайд по скринам для ДЗ (папка docs/screenshots/)

Положите сюда 5 PNG-скриншотов. Имена файлов — строго такие:

## Обязательные скрины

1. `01-form.png`
   - Страница http://localhost:5076/ — форма (пустые поля видны).
2. `02-lifecycle-page.png`
   - Страница http://localhost:5076/lifecycle.html — схема жизненного цикла.
3. `03-console-startup.png`
   - Консоль/терминал VS с запуском: строки `[LIFECYCLE 1/6]…[6/6]` и `[HOST] >>> Приложение СТАРТУЕТ`.
   - Как снять: запустите `dotnet run`, дождитесь строк, сделайте скрин окна Output/Terminal.
4. `04-post-valid.png`
   - Успешная отправка валидной формы (name=Иван, email с @, сообщение длинное).
   - Страница «✅ Форма прошла весь конвейер!» + в консоли `[ENDPOINT] >>> POST /submit ДОШЁЛ`.
5. `05-post-invalid-and-headers.png` (можно 2 скрина: `05a-...`, `05b-...`)
   - a) Невалидная форма → страница «❌ Форма отклонена middleware» (400).
   - b) F12 → Network → клик на `submit` → вкладка Headers с `X-Logged-By`, `X-Response-Time-ms`, `X-Lifecycle-Stage`.

## Как снимать (Windows + VS)

- Запуск: `dotnet run` в папке проекта или F5 в Visual Studio (профиль `http`).
- Браузер: Chrome/Edge, адрес `http://localhost:5076/`.
- Консоль сервера: окно «Вывод» / внешний терминал, где видны `[1-LOG]`, `[2-TIME]`, `[3-FORM]`.
- Скрины: `Win + Shift + S` → сохранить PNG в эту папку.
- Проверка перед отправкой ДЗ: в папке должно быть минимум `01..05` PNG.

Пример лога для сверки — `docs/example-console-log.txt`.
