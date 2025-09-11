using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class GetAllProducts
    {
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                              .WithTags("Products");

            group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            {
                var response = await db.Products
                    .AsNoTracking()
                    .Select(p => new Response(p.Id, p.Name, p.Price, p.CategoryId))
                    .ToListAsync(ct);

                return Results.Ok(response);
            })
            .WithName("GetAllProducts")
            .WithSummary("List all products")
            .WithDescription("Returns all products.")
            .Produces<List<Response>>(StatusCodes.Status200OK);
        }
    }
}
