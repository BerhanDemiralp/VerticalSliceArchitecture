using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Category.Contracts;

namespace VerticalSliceArchitecture.Features.Categories;

public static class GetAllCategories
{
    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly ICategoryCache _cache;

        public Handler(AppDbContext db, ICategoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Results<Ok<CategoryListDto>, NotFound>> Handle(CancellationToken ct)
        {

            var cachedCategories = await _cache.GetAllCategoriesAsync(ct);

            if (cachedCategories != null)
            {
                return TypedResults.Ok(cachedCategories);
            }

            var categoriesFromDb = await _db.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync(ct);

            if (categoriesFromDb is null)
            {
                return TypedResults.NotFound();
            }
            var response = new CategoryListDto(categoriesFromDb);
            await _cache.SetAllCategoriesAsync(response, TimeSpan.FromMinutes(10), ct);
            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
                         .WithTags("Categories");

        group.MapGet("/", async (
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(ct);
        })
        .WithName("GetAllCategories")
        .WithSummary("Get all categories")
        .Produces<CategoryListDto>(StatusCodes.Status200OK)
        .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}