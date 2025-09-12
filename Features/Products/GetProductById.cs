using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class GetProductById
    {
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                             .WithTags("Products");

            // Add IDistributedCache as a parameter to the lambda function to access the cache service.
            group.MapGet("/{id}", async (int id, AppDbContext db, IDistributedCache cache, CancellationToken ct) =>
            {
                // 1. Define a unique cache key for the product.
                string cacheKey = $"product:{id}";

                // 2. Try to get the product from the Redis cache.
                string cachedProductJson = await cache.GetStringAsync(cacheKey, ct);

                if (cachedProductJson != null)
                {
                    // 3. If the product is found in the cache, deserialize and return it.
                    var cachedProduct = JsonSerializer.Deserialize<Response>(cachedProductJson);
                    return Results.Ok(cachedProduct);
                }

                // 4. If not found in the cache, fetch the product from the database.
                var response = await db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new Response(p.Id, p.Name, p.Price, p.CategoryId))
                    .SingleOrDefaultAsync(ct);

                if (response is null)
                {
                    return Results.NotFound();
                }

                // 5. Product was found in the database. Now, cache it for future requests.
                string jsonString = JsonSerializer.Serialize(response);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache for 5 minutes.
                };
                await cache.SetStringAsync(cacheKey, jsonString, cacheOptions, ct);

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
