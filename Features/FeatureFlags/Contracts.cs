// Features/FeatureFlags/Contracts.cs

namespace VerticalSliceArchitecture.Features.FeatureFlags
{
    public class Contracts 
    {
        public record FeatureFlagDto(string Name, bool IsEnabled, string UserType = "default");
        public record FeatureFlagListDto(List<FeatureFlagDto> FeatureFlags);
    }

}