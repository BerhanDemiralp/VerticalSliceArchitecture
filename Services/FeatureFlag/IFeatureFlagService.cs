// Services/FeatureFlag/IFeatureFlagService.cs

using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

namespace VerticalSliceArchitecture.Services.FeatureFlag;

public interface IFeatureFlagService
{
    Task<FeatureFlagDto?> GetFlagAsync(string flagName, string userType, CancellationToken ct = default);
}