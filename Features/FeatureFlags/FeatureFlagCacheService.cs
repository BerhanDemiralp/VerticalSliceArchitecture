using VerticalSliceArchitecture.Services.Caching;
using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public class FeatureFlagCacheService : IFeatureFlagCache
{
    private readonly ICacheService<FeatureFlagDto> _cache;

    public FeatureFlagCacheService(ICacheService<FeatureFlagDto> cache)
    {
        _cache = cache;
    }

    public Task<FeatureFlagDto?> GetFeatureFlagAsync(string key, CancellationToken ct)
    {
        return _cache.GetAsync(key, ct);
    }

    public Task SetFeatureFlagAsync(FeatureFlagDto flag, TimeSpan duration, CancellationToken ct)
    {
        string cacheKey = $"featureFlag:{flag.Name}:{flag.UserType}";
        return _cache.SetAsync(cacheKey, flag, duration, ct);
    }

    public Task RemoveFeatureFlagAsync(string key, CancellationToken ct)
    {
        return _cache.RemoveAsync(key, ct);
    }
}