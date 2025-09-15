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
    public record Command(bool IsEnabled, string UserType);

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

            flag.IsEnabled = command.IsEnabled;
            await _db.SaveChangesAsync(ct);

            // Invalidate the cache for this specific flag
            var cacheKey = $"featureFlag:{flag.Name}:{flag.UserType}";
            await _cache.RemoveFeatureFlagAsync(cacheKey, ct);

            var response = new FeatureFlagDto(flag.Name, flag.IsEnabled, flag.UserType);
            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/flags")
            .WithTags("Feature Flags");

        group.MapPut("/{name}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromBody] Command command,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, command, ct);
        })
        .WithName("UpdateFeatureFlag")
        .WithSummary("Update the status of an existing feature flag.")
        .AddEndpointFilter(new FeatureFlagFilter("UpdateFlagStatusEnabled"))
        .Produces<FeatureFlagDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}