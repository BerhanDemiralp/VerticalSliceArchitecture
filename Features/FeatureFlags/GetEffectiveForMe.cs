using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.FeatureFlags;

public static class GetEffectiveForMe
{
    public record EffectiveFeature(string Name, bool IsEnabled, string? State);

    public sealed class Handler
    {
        private readonly AppDbContext _db;
        public Handler(AppDbContext db) => _db = db;

        public async Task<IResult> Handle(HttpContext ctx, CancellationToken ct)
        {
            // User-Type şimdilik header’dan geliyor (UI zaten gönderiyor)
            var userType = ctx.Request.Headers["User-Type"].FirstOrDefault() ?? "default";

            var all = await _db.FeatureFlags
                .AsNoTracking()
                .Select(f => new { f.Name, f.UserType, f.IsEnabled, f.State })
                .ToListAsync(ct);

            // default baz + userType override
            var dict = new Dictionary<string, EffectiveFeature>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in all.Where(x => x.UserType == "default"))
                dict[f.Name] = new EffectiveFeature(f.Name, f.IsEnabled, f.State);
            foreach (var f in all.Where(x => x.UserType == userType))
                dict[f.Name] = new EffectiveFeature(f.Name, f.IsEnabled, f.State);

            return Results.Ok(dict.Values);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/me/features")
                          .WithTags("Feature Flags (Me)")
                          .RequireAuthorization();

        group.MapGet("/", async (Handler h, HttpContext ctx, CancellationToken ct)
            => await h.Handle(ctx, ct));
    }
}
