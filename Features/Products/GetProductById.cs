using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Products;

public static class GetProductById
{
    public record RouteParameter(int Id);
    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IProductCache _cache;

        public Handler(AppDbContext db, IProductCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // İşlemi gerçekleştiren ana metot. Query'yi parametre olarak alır.
        public async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> Handle(RouteParameter routeParameter, CancellationToken ct)
        {
            var cachedProduct = await _cache.GetProductAsync(routeParameter.Id, ct);
            if (cachedProduct != null)
            {
                // Önbellekte varsa, doğrudan döndür.
                return TypedResults.Ok(cachedProduct);
            }

            // Önbellekte yoksa, veritabanından getir.
            var response = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id == routeParameter.Id)
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.CategoryId))
                .SingleOrDefaultAsync(ct);

            if (response is null)
            {
                return TypedResults.NotFound(new ErrorResponse("No product with the specified ID was found."));
            }

            // Veritabanından gelen veriyi önbelleğe al.
            await _cache.SetProductAsync(response, TimeSpan.FromMinutes(5), ct);

            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/{id}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, ct);
        })
        .WithName("GetProductById")
        .WithSummary("Get an existing product.")
        .WithDescription("Get a product by its unique ID, with caching.")
        .AddEndpointFilter(new FeatureFlagFilter("GetProductByIdEnabled"))
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

    }
}