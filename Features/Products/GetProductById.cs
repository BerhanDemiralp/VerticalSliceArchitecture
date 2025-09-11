using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class GetProductById
    {
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                              .WithTags("Products");

            group.MapGet("/{id}", async (int id, AppDbContext db, CancellationToken ct) =>
            {
                var response = await db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new Response(p.Id, p.Name, p.Price, p.CategoryId))
                    .SingleOrDefaultAsync(ct);

                if (response is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(response);
            })
            .WithName("GetProductById")
            .WithSummary("Get an existing product.")
            .WithDescription("Get a product by its unique ID.")
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
        }
    }
}
