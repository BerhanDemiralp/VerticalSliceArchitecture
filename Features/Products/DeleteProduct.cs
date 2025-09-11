using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class DeleteProduct
    {

        public static void Map(IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/products")
                              .WithTags("Products");

            group.MapDelete("/{id}", async (int id, AppDbContext db, CancellationToken ct) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product is null)
                {
                    return Results.NotFound();
                }

                db.Products.Remove(product);
                await db.SaveChangesAsync(ct);

                return Results.NoContent();
            })
            .WithName("DeleteProduct")
            .WithSummary("Deletes an existing product")
            .WithDescription("Deletes a product by its unique ID.")
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
        }
    }
}
