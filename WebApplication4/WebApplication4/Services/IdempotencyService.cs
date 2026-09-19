using System.Collections.Concurrent;

namespace WebApplication4.Services;

/// <summary>
/// Idempotency-Key для POST/PATCH (из конспекта).
/// Повторный запрос с тем же ключом возвращает сохранённый ответ,
/// а не выполняет операцию заново. TTL — 24 часа.
/// </summary>
public sealed class IdempotencyService
{
    private sealed record Entry(DateTime ExpiresUtc, int StatusCode, string Body, string? Location);

    private readonly ConcurrentDictionary<string, Entry> _store = new();
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    public bool TryGet(string key, out (int StatusCode, string Body, string? Location) result)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresUtc > DateTime.UtcNow)
            {
                result = (entry.StatusCode, entry.Body, entry.Location);
                return true;
            }
            _store.TryRemove(key, out _); // просрочен — чистим лениво
        }
        result = default;
        return false;
    }

    public void Save(string key, int statusCode, string body, string? location, TimeSpan? ttl = null)
    {
        _store[key] = new Entry(DateTime.UtcNow + (ttl ?? DefaultTtl), statusCode, body, location);
    }
}
