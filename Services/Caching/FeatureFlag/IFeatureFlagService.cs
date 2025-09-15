// Services/FeatureFlag/IFeatureFlagService.cs

namespace VerticalSliceArchitecture.Services.FeatureFlag;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagName, string userType, CancellationToken ct = default);
}