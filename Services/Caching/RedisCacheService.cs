using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace VerticalSliceArchitecture.Services.Caching;

// Generic implementation of the ICacheService using IDistributedCache (e.g., Redis).
public class CacheService<T> : ICacheService<T> where T : class
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync(string key, CancellationToken ct = default)
    {
        string? cachedValueJson = await _cache.GetStringAsync(key, ct);
        if (string.IsNullOrEmpty(cachedValueJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(cachedValueJson);
    }

    public async Task SetAsync(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        string jsonString = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonString, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }
}
