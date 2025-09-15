// Features/FeatureFlags/IFeatureFlagCache.cs

using VerticalSliceArchitecture.Features.FeatureFlags;
using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

public interface IFeatureFlagCache
{
    Task<FeatureFlagDto?> GetFeatureFlagAsync(string key, CancellationToken ct);
    Task SetFeatureFlagAsync(FeatureFlagDto flag, TimeSpan duration, CancellationToken ct);
    Task RemoveFeatureFlagAsync(string key, CancellationToken ct);
}