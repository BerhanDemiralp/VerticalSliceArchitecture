// Services/FeatureFlag/FeatureFlagService.cs

using Microsoft.EntityFrameworkCore;
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

    public async Task<FeatureFlagDto?> GetFlagAsync(string flagName, string userType, CancellationToken ct = default)
    {
        var cacheKey = $"featureFlag:{flagName}:{userType}";
        var cachedFlag = await _cache.GetFeatureFlagAsync(cacheKey, ct);
        if (cachedFlag != null)
        {
            return cachedFlag;
        }

        var dbFlag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Name == flagName && f.UserType == userType, ct);
        if (dbFlag != null)
        {
            var flagDto = new FeatureFlagDto(dbFlag.Name, dbFlag.State, dbFlag.IsEnabled, dbFlag.UserType);
            await _cache.SetFeatureFlagAsync(flagDto, TimeSpan.FromMinutes(15), ct);
            return flagDto;
        }

        var defaultFlag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Name == flagName && f.UserType == "default", ct);
        if (defaultFlag != null)
        {
            var flagDto = new FeatureFlagDto(defaultFlag.Name, defaultFlag.State, defaultFlag.IsEnabled, defaultFlag.UserType);
            await _cache.SetFeatureFlagAsync(flagDto, TimeSpan.FromMinutes(15), ct);
            return flagDto;
        }

        // Return null if flag is not found anywhere
        return null;
    }
}