var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(configure => configure.AddConsole());

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Успешный запуск", DateTime.Now);
});

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Начинаю остановку");
});

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Остановись");
});

app.Use(async(context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);

    await next();

    stopwatch.Stop();
    logger.LogInformation("{Method} {Path} {time}", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/slow", async () =>
{
    await Task.Delay(5000);
    return "slow answer";
});

app.MapGet("/heavy", async () =>
{
    await Task.Delay(5000);
    app.StopAsync();
    return "Ready task";
});

app.Run();
