// Features/FeatureFlag/FeatureFlagFilter.cs

using Microsoft.AspNetCore.Http;
using VerticalSliceArchitecture.Services.FeatureFlag;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public class FeatureFlagFilter : IEndpointFilter
{
    private readonly string _flagName;

    public FeatureFlagFilter(string flagName)
    {
        _flagName = flagName;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Get the IFeatureFlagService from the request services
        var featureFlagService = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagService>();

        // Extract user type from a request header
        string userType = context.HttpContext.Request.Headers["User-Type"].FirstOrDefault()?.ToLowerInvariant() ?? "default";

        var isEnabled = await featureFlagService.IsEnabledAsync(_flagName, userType, context.HttpContext.RequestAborted);

        if (!isEnabled)
        {
            return Results.BadRequest($"The '{_flagName}' feature is currently disabled for user type '{userType}'.");
        }

        return await next(context);
    }
}