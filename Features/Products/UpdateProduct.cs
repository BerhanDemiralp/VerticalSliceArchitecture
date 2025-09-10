// Features/Products/UpdateProduct.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class UpdateProduct
    {
        public record Request(string Name, decimal Price, int? CategoryId);
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                              .WithTags("Products");

            group.MapPut("/{id:int}", async (int id, Request req, AppDbContext db, CancellationToken ct) =>
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
                if (product is null)
                    return Results.NotFound();

                product.Name = req.Name;
                product.Price = req.Price;
                product.CategoryId = req.CategoryId;

                await db.SaveChangesAsync(ct);

                var resp = new Response(product.Id, product.Name, product.Price, product.CategoryId);
                return Results.Ok(resp);
            })
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .WithDescription("Updates product fields (Name, Price, CategoryId)")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
