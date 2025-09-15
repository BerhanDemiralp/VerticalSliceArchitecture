using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Category.Contracts;

namespace VerticalSliceArchitecture.Features.Categories;

public static class CreateCategory
{
    public record Command(string Name);
    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly ICategoryCache _cache;

        public Handler(AppDbContext db, ICategoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // İşlemi gerçekleştiren ana metot. Command nesnesini alır.
        public async Task<Results<Created<CategoryDto>, BadRequest>> Handle(Command command, CancellationToken ct)
        {
            var category = new Domain.Category
            {
                Name = command.Name
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync(ct);

            var response = new CategoryDto(category.Id, category.Name);
            await _cache.RemoveAllCategoriesAsync(ct);

            // Başarılı bir şekilde oluşturulduğunu belirtmek için Created tipini kullanın.
            return TypedResults.Created($"/api/categories/{response.Id}", response);
        }
    }

    // 3. EndPoint Tanımı
    // Bu metot, sadece gelen isteği alır ve iş mantığını içeren Handler'ı çağırır.
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapPost("/", async (
            [FromBody] Command command,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            // Handler'ı çağır ve sonucu döndür.
            return await handler.Handle(command, ct);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Creates a new category with Name.")
        .Produces<CategoryDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}