// Features/FeatureFlag/UpdateFlagStatus.cs

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public static class UpdateFlagStatus
{
    public record RouteParameter(string Name);
    // Update the Command to handle both State and IsEnabled
    public record Command(string? State, bool IsEnabled, string UserType);

    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IFeatureFlagCache _cache;

        public Handler(AppDbContext db, IFeatureFlagCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Results<Ok<FeatureFlagDto>, NotFound>> Handle(RouteParameter routeParameter, Command command, CancellationToken ct)
        {
            var flag = await _db.FeatureFlags
                .FirstOrDefaultAsync(f => f.Name == routeParameter.Name && f.UserType == command.UserType, ct);

            if (flag is null)
            {
                return TypedResults.NotFound();
            }

            // Update both properties
            flag.State = command.State;
            flag.IsEnabled = command.IsEnabled;
            await _db.SaveChangesAsync(ct);

            var cacheKey = $"featureFlag:{flag.Name}:{flag.UserType}";
            await _cache.RemoveFeatureFlagAsync(cacheKey, ct);

            var response = new FeatureFlagDto(flag.Name, flag.State, flag.IsEnabled, flag.UserType);
            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/flags")
            .WithTags("Feature Flags")
            .RequireAuthorization("AdminOnly");


        group.MapPut("/{name}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromBody] Command command,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, command, ct);
        })
        .WithName("UpdateFeatureFlag")
        .WithSummary("Update the state and enabled status of an existing feature flag.")
        .AddEndpointFilter(new FeatureFlagFilter("UpdateFlagStatusEnabled"))
        .Produces<FeatureFlagDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}