using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Products;
public static class UpdateProduct
{
    public record Command(string Name, decimal Price, int? CategoryId);
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

        // İşlemi gerçekleştiren ana metot.
        public async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> Handle(RouteParameter routeParameter, Command command, CancellationToken ct)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == routeParameter.Id, ct);
            if (product is null)
            {
                return TypedResults.NotFound(new ErrorResponse("No product with the specified ID was found."));
            }

            product.Name = command.Name;
            product.Price = command.Price;
            product.CategoryId = command.CategoryId;

            await _db.SaveChangesAsync(ct);

            var response = new ProductDto(product.Id, product.Name, product.Price, product.CategoryId);

            // Veritabanı işlemi başarılı olduktan sonra önbelleği temizle.
            // Hem tekil ürünü hem de tüm ürünler listesini temizlemek önemlidir.
            await _cache.RemoveProductAsync(routeParameter.Id, ct);
            await _cache.RemoveAllProductsAsync(ct);

            return TypedResults.Ok(response);
        }
    }

    // 3. Endpoint Tanımı
    // Bu metot, sadece gelen isteği Handler'a yönlendirir.
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPut("/{id:int}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromBody] Command command,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, command, ct);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product.")
        .WithDescription("Updates a product by its unique ID, with cache invalidation.")
        .AddEndpointFilter(new FeatureFlagFilter("UpdateProductEnabled"))
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }
}
