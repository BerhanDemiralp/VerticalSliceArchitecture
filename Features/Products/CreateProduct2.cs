using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Products;

public static class CreateProduct2
{
    public record Command(string Name, decimal Price, int? CategoryId);
    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IProductCache _cache;

        public Handler(AppDbContext db, IProductCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // İşlemi gerçekleştiren ana metot. Command nesnesini alır.
        public async Task<Results<Created<ProductDto>, BadRequest<ErrorResponse>>> Handle(Command command, CancellationToken ct)
        {
            if (command.Price < 10)
            {
                return TypedResults.BadRequest(new ErrorResponse("Price should grater than 10"));
            }

            var product = new Product
            {
                Name = command.Name,
                Price = command.Price,
                CategoryId = command.CategoryId
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);

            var response = new ProductDto(product.Id, product.Name, product.Price, product.CategoryId);
            await _cache.RemoveAllProductsAsync(ct);

            // Başarılı bir şekilde oluşturulduğunu belirtmek için Created tipini kullanın.
            return TypedResults.Created($"/api/products/{response.Id}", response);
        }
    }

    // 3. EndPoint Tanımı
    // Bu metot, sadece gelen isteği alır ve iş mantığını içeren Handler'ı çağırır.
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPost("/v2", async (
            [FromBody] Command command,
            [FromServices] Handler handler,
            CancellationToken ct) =>
        {
            // Handler'ı çağır ve sonucu döndür.
            return await handler.Handle(command, ct);
        })
        .WithName("CreateProductV2")
        .WithSummary("Create a new product V2")
        .WithDescription("Creates a new product with Name, Price, and CategoryId V2.")
        .AddEndpointFilter(new FeatureFlagFilter("CreateProductEnabled","v2"))
        .Produces<ProductDto>(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

    }
}