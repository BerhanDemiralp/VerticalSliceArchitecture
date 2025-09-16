// Features/FeatureFlag/FeatureFlagFilter.cs

using Microsoft.AspNetCore.Http;
using VerticalSliceArchitecture.Services.FeatureFlag;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public class FeatureFlagFilter : IEndpointFilter
{
    private readonly string _flagName;
    private readonly string _requiredState;

    public FeatureFlagFilter(string flagName, string requiredState = "")
    {
        _flagName = flagName;
        _requiredState = requiredState;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var featureFlagService = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagService>();

        string userType = context.HttpContext.Request.Headers["User-Type"].FirstOrDefault()?.ToLowerInvariant() ?? "default";

        var flag = await featureFlagService.GetFlagAsync(_flagName, userType, context.HttpContext.RequestAborted);

        // Check if the flag is not null, is enabled, and if a required state is specified, check that too.
        if (flag is null || !flag.IsEnabled || (!string.IsNullOrEmpty(_requiredState) && flag.State != _requiredState))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}