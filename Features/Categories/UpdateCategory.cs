using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Category.Contracts;

namespace VerticalSliceArchitecture.Features.Categories
{
    public static class UpdateCategory
    {
        public record Command(string Name);
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

            // İşlemi gerçekleştiren ana metot.
            public async Task<Results<Ok<CategoryDto>, NotFound>> Handle(RouteParameter routeParameter, Command command, CancellationToken ct)
            {
                var category = await _db.Categories.FirstOrDefaultAsync(p => p.Id == routeParameter.Id, ct);
                if (category is null)
                {
                    return TypedResults.NotFound();
                }

                category.Name = command.Name;

                await _db.SaveChangesAsync(ct);

                var response = new CategoryDto(category.Id, category.Name);

                // Veritabanı işlemi başarılı olduktan sonra önbelleği temizle.
                // Hem tekil ürünü hem de tüm ürünler listesini temizlemek önemlidir.
                await _cache.RemoveCategoryAsync(routeParameter.Id, ct);
                await _cache.RemoveAllCategoriesAsync(ct);

                return TypedResults.Ok(response);
            }
        }

        // 3. Endpoint Tanımı
        // Bu metot, sadece gelen isteği Handler'a yönlendirir.
        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/categories")
                .WithTags("Categories");

            group.MapPut("/{id:int}", async (
                [AsParameters] RouteParameter routeParameter,
                [FromBody] Command command,
                [FromServices] Handler handler,
                CancellationToken ct) =>
            {
                return await handler.Handle(routeParameter, command, ct);
            })
            .WithName("UpdateCategory")
            .WithSummary("Update an existing categor")
            .WithDescription("Updates a category by its unique ID.")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}