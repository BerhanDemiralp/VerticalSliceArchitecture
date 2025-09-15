using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products;

public static class DeleteProduct
{
    public record RouteParameter(int Id);

    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IProductCache _cache; // Önbellek servisi eklendi

        public Handler(AppDbContext db, IProductCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Results<NoContent, NotFound>> Handle(RouteParameter routeParameter, CancellationToken ct)
        {
            var product = await _db.Products.FindAsync(routeParameter.Id);
            if (product is null)
            {
                return TypedResults.NotFound();
            }

            // Önce veritabanından ürünü kaldır
            _db.Products.Remove(product);
            await _db.SaveChangesAsync(ct);

            // Veritabanı işlemi başarılı olduktan sonra önbelleği temizle
            // Bu, tutarlılık için çok önemlidir.
            await _cache.RemoveProductAsync(routeParameter.Id, ct);
            await _cache.RemoveAllProductsAsync(ct);

            return TypedResults.NoContent();
        }
    }

    // 3. Endpoint Tanımı
    // Bu metot, sadece gelen isteği Handler'a yönlendirir.
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
                          .WithTags("Products");

        group.MapDelete("/{id}", async (
            // URL'den gelen 'id' otomatik olarak Command'in 'Id' özelliğine atanır.
            [AsParameters] RouteParameter routeParameter,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, ct);
        })
        .WithName("DeleteProduct")
        .WithSummary("Deletes an existing product.")
        .WithDescription("Deletes a product by its unique ID, and clears it from cache.")
        .AddEndpointFilter(new FeatureFlagFilter("DeleteProductEnabled"))
        .Produces<NoContent>(StatusCodes.Status204NoContent)
        .Produces<NotFound>(StatusCodes.Status404NotFound);

    }
}
