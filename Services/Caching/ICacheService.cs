using System.Text.Json;

namespace VerticalSliceArchitecture.Services.Caching;

public interface ICacheService<T> where T : class
{
    Task<T?> GetAsync(string key, CancellationToken ct = default);

    Task SetAsync(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);

    Task RemoveAsync(string key, CancellationToken ct = default);
}
