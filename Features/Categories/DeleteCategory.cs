using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Categories;

public static class DeleteCategory
{
    public record RouteParameter(int Id);

    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly ICategoryCache _cache; 

        public Handler(AppDbContext db, ICategoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Results<NoContent, NotFound>> Handle(RouteParameter routeParameter, CancellationToken ct)
        {
            var category = await _db.Categories.FindAsync(routeParameter.Id);
            if (category is null)
            {
                return TypedResults.NotFound();
            }

            // Önce veritabanından ürünü kaldır
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync(ct);

            // Veritabanı işlemi başarılı olduktan sonra önbelleği temizle
            await _cache.RemoveCategoryAsync(routeParameter.Id, ct);
            await _cache.RemoveAllCategoriesAsync(ct);

            return TypedResults.NoContent();
        }
    }

    // 3. Endpoint Tanımı
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
                          .WithTags("Categories");

        group.MapDelete("/{id}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, ct);
        })
        .WithName("DeleteCategory")
        .WithSummary("Deletes an existing category")
        .WithDescription("Deletes a category by its unique ID.")
        .Produces<NoContent>(StatusCodes.Status204NoContent)
        .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}