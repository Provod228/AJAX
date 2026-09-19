using System.Security.Cryptography;
using System.Text;
using WebApplication4.Models;

namespace WebApplication4.Infrastructure;

/// <summary>
/// Хелперы ETag из конспекта: версия ресурса → ETag "v{N}".
/// </summary>
public static class EntityTags
{
    public static string For(Product p) => $"\"v{p.Version}\"";

    public static string ForList(IEnumerable<Product> items)
    {
        var raw = string.Join(";", items.Select(p => $"{p.Id}:{p.Version}"));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)))[..16];
        return $"\"list-{hash.ToLowerInvariant()}\"";
    }

    /// <summary>
    /// Сравнение If-None-Match / If-Match с текущим ETag (включая "*" и списки).
    /// </summary>
    public static bool Matches(string? headerValue, string etag)
    {
        if (string.IsNullOrWhiteSpace(headerValue))
            return false;
        if (headerValue.Trim() == "*")
            return true;
        return headerValue.Split(',').Any(t =>
        {
            t = t.Trim();
            if (t.StartsWith("W/", StringComparison.Ordinal))
                t = t[2..];
            return string.Equals(t, etag, StringComparison.Ordinal);
        });
    }
}
