using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Products;

public static class GetAllProducts
{
    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IProductCache _cache;

        public Handler(AppDbContext db, IProductCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Results<Ok<ProductListDto>, NotFound>> Handle( CancellationToken ct)
        {

            var cachedProducts = await _cache.GetAllProductsAsync(ct);

            if (cachedProducts != null)
            {
                return TypedResults.Ok(cachedProducts);
            }

            var productsFromDb = await _db.Products
                .AsNoTracking()
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.CategoryId))
                .ToListAsync(ct);

            if (productsFromDb is null)
            {
                return TypedResults.NotFound();
            }
            var response = new ProductListDto(productsFromDb);
            await _cache.SetAllProductsAsync(response, TimeSpan.FromMinutes(10), ct);
            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
                         .WithTags("Products");

        group.MapGet("/", async (
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(ct);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products.")
        .Produces<ProductListDto>(StatusCodes.Status200OK)
        .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}