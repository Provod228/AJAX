namespace midalware.Services;

/// <summary>
/// ЖИЗНЕННЫЙ ЦИКЛ ХОСТА (приложения):
/// IHostedService.StartAsync вызывается при app.Run() (старт),
/// StopAsync — при остановке (Ctrl+C / остановка в VS).
/// Это первая и последняя строчки жизненного цикла — удобно показать на скрине консоли.
/// </summary>
public class AppLifecycleService : IHostedService
{
    private readonly ILogger<AppLifecycleService> _logger;

    public AppLifecycleService(ILogger<AppLifecycleService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("[HOST] >>> Приложение СТАРТУЕТ (IHostedService.StartAsync)");
        Console.WriteLine("[HOST] Жизненный цикл: CreateBuilder -> Services -> Build -> Pipeline -> Run");
        Console.WriteLine("==================================================");
        _logger.LogInformation("AppLifecycleService запущен");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("[HOST] <<< Приложение ОСТАНАВЛИВАЕТСЯ (IHostedService.StopAsync)");
        Console.WriteLine("==================================================");
        _logger.LogInformation("AppLifecycleService остановлен");
        return Task.CompletedTask;
    }
}
