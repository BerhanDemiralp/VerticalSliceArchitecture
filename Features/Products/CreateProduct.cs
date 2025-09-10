using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.CreateProduct;

namespace VerticalSliceArchitecture.Features.Products
{
    public class CreateProduct
    {
        public record Command(string Name, decimal Price, int? CategoryId) : IRequest<Response>;
        public record Response(int Id, string Name, decimal Price, int? CategoryId);
        public class Handler(AppDbContext context) : IRequestHandler<Command, Response>
        {
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = new Product
                {
                    Name = request.Name,
                    Price = request.Price,
                    CategoryId = request.CategoryId
                };
                context.Products.Add(product);
                await context.SaveChangesAsync(cancellationToken);
                return new Response(product.Id,product.Name,product.Price,product.CategoryId);
            }
        }
    }

    [ApiController]
    [Route("api/products")]
    public class CreateProductController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Response>> CreateProduct(Command command)
        {
            var response = await sender.Send(command);
            return Ok(response);
        }
    }
}
