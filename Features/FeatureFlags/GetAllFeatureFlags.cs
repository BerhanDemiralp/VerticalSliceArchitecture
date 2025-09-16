// Features/FeatureFlags/GetAllFeatureFlags.cs

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.FeatureFlags.Contracts;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public static class GetAllFeatureFlags
{
    // Update the DTO to include both State and IsEnabled
    public record FeatureFlagDto(string Name, string UserType, string? State, bool IsEnabled);

    public class Handler
    {
        private readonly AppDbContext _db;

        public Handler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Results<Ok<List<FeatureFlagDto>>, NotFound>> Handle(CancellationToken ct)
        {
            var flags = await _db.FeatureFlags
                .AsNoTracking()
                // Select both State and IsEnabled
                .Select(f => new FeatureFlagDto(f.Name, f.UserType, f.State, f.IsEnabled))
                .ToListAsync(ct);

            if (flags is null || !flags.Any())
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(flags);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/flags")
            .WithTags("Feature Flags");

        group.MapGet("/", async (
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(ct);
        })
        .WithName("GetAllFeatureFlags")
        .WithSummary("Gets the state and enabled status of all feature flags.")
        .WithDescription("Retrieves all feature flags and their current status for all user types.")
        .AddEndpointFilter(new FeatureFlagFilter("GetAllFeatureFlagsEnabled"))
        .Produces<List<FeatureFlagDto>>(StatusCodes.Status200OK)
        .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}