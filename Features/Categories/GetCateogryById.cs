using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Category.Contracts;

namespace VerticalSliceArchitecture.Features.Categories;

public static class GetCategoryById
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

        // İşlemi gerçekleştiren ana metot. Query'yi parametre olarak alır.
        public async Task<Results<Ok<CategoryDto>, NotFound>> Handle(RouteParameter routeParameter, CancellationToken ct)
        {
            var cachedCategory = await _cache.GetCategoryAsync(routeParameter.Id, ct);
            if (cachedCategory != null)
            {
                // Önbellekte varsa, doğrudan döndür.
                return TypedResults.Ok(cachedCategory);
            }

            // Önbellekte yoksa, veritabanından getir.
            var response = await _db.Categories
                .AsNoTracking()
                .Where(c => c.Id == routeParameter.Id)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .SingleOrDefaultAsync(ct);

            if (response is null)
            {
                return TypedResults.NotFound();
            }

            // Veritabanından gelen veriyi önbelleğe al.
            await _cache.SetCategoryAsync(response, TimeSpan.FromMinutes(5), ct);

            return TypedResults.Ok(response);
        }
    }

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapGet("/{id}", async (
            [AsParameters] RouteParameter routeParameter,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            return await handler.Handle(routeParameter, ct);
        })
        .WithName("GetCategoryById")
        .WithSummary("Get an existing category")
        .WithDescription("Get a category by its unique ID.")
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}