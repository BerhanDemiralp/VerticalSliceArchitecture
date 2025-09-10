// Features/Products/CreateProduct.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Common;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class CreateProduct
    {
        public record Request(string Name, decimal Price, int? CategoryId);
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                              .WithTags("Products");

            group.MapPost("/", async (Request req, AppDbContext db, CancellationToken ct) =>
            {
                var product = new Product
                {
                    Name = req.Name,
                    Price = req.Price,
                    CategoryId = req.CategoryId
                };

                db.Products.Add(product);
                await db.SaveChangesAsync(ct);

                var resp = new Response(product.Id, product.Name, product.Price, product.CategoryId);

                return Results.Created($"/api/products/{resp.Id}", resp);
            })
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product with Name, Price, and CategoryId.")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
