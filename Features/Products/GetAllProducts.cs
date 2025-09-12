using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using Microsoft.AspNetCore.Mvc; // [FromServices] için bu namespace'i eklemelisiniz

namespace VerticalSliceArchitecture.Features.Products
{
    public static class GetAllProducts
    {
        public record Response(List<Product> Products);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                             .WithTags("Products");

            group.MapGet("/", async (AppDbContext db, [FromServices] IProductCache cache, CancellationToken ct) =>
            {
                var cacheDuration = TimeSpan.FromMinutes(10);

                // 1. Önbelleği kontrol et
                var response = await cache.GetAllProductsAsync(ct);

                if (response is not null)
                {
                    // 2. Önbellekte veri varsa, direkt döndür.
                    return Results.Ok(response);
                }

                // 3. Önbellekte veri yoksa, veritabanından getir.
                var productsFromDb = await db.Products
                    .AsNoTracking()
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        CategoryId = p.CategoryId
                    })
                    .ToListAsync(ct);

                // 4. Gelen veriyi GetAllProducts.Response tipine atama
                var allProductsResponse = new GetAllProducts.Response(productsFromDb);

                // 5. Yeni veriyi önbelleğe ekle
                await cache.SetAllProductsAsync(allProductsResponse, cacheDuration, ct);

                // 6. Sonucu döndür
                return Results.Ok(allProductsResponse);
            });
        }
    }
}
