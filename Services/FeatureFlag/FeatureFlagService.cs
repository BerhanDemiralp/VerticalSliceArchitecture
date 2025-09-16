// Services/FeatureFlag/FeatureFlagService.cs

using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

namespace VerticalSliceArchitecture.Services.FeatureFlag;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagCache _cache;
    private readonly AppDbContext _db;

    public FeatureFlagService(IFeatureFlagCache cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public async Task<bool> IsEnabledAsync(string flagName, string userType, CancellationToken ct = default)
    {
        // Use a composite key like "featureFlag:Name:UserType" for uniqueness
        var cacheKey = $"featureFlag:{flagName}:{userType}";
        var cachedFlag = await _cache.GetFeatureFlagAsync(cacheKey, ct);
        if (cachedFlag != null)
        {
            return cachedFlag.IsEnabled;
        }

        // Look for a user-specific flag in the database
        var dbFlag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Name == flagName && f.UserType == userType, ct);
        if (dbFlag != null)
        {
            await _cache.SetFeatureFlagAsync(new FeatureFlagDto(dbFlag.Name, dbFlag.IsEnabled, dbFlag.UserType), TimeSpan.FromMinutes(15), ct);
            return dbFlag.IsEnabled;
        }

        // Fallback to the default flag if no user-specific one is found
        var defaultFlag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Name == flagName && f.UserType == "default", ct);
        if (defaultFlag != null)
        {
            await _cache.SetFeatureFlagAsync(new FeatureFlagDto(defaultFlag.Name, defaultFlag.IsEnabled, defaultFlag.UserType), TimeSpan.FromMinutes(15), ct);
            return defaultFlag.IsEnabled;
        }

        return false;
    }
}