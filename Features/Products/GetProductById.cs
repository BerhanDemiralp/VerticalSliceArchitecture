using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
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

            group.MapGet("/{id}", async (int id, AppDbContext db, [FromServices] IProductCache cache, CancellationToken ct) =>
            {
                var cachedProduct = await cache.GetProductAsync(id, ct);
                if (cachedProduct != null)
                {
                    // If found, return it directly.
                    return Results.Ok(cachedProduct);
                }

                // If not found in the cache, fetch from the database.
                var response = await db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new Response(p.Id, p.Name, p.Price, p.CategoryId))
                    .SingleOrDefaultAsync(ct);

                if (response is null)
                {
                    return Results.NotFound();
                }

                // Cache the new product data using the caching service.
                await cache.SetProductAsync(response, TimeSpan.FromMinutes(5), ct);

                return Results.Ok(response);
            })
            .WithName("GetProductById")
            .WithSummary("Get an existing product.")
            .WithDescription("Get a product by its unique ID, with caching.")
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
        }
    }
}
