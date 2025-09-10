using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.GetAllProducts;

namespace VerticalSliceArchitecture.Features.Products
{
    public static class GetAllProducts
    {
        public record Query : IRequest<IEnumerable<Response>>;
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

        public class Handler(AppDbContext context) : IRequestHandler<Query, IEnumerable<Response>>
        {
            public async Task<IEnumerable<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var products = await context.Products.ToListAsync(cancellationToken);
                return products.Select(p => new Response(p.Id, p.Name, p.Price, p.CategoryId));
            }
        }
    }

    [ApiController]
    [Route("api/products")]
    public class GetAllProductsController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Response>>> GetAllProducts()
        {
            var response = await sender.Send(new Query());
            return Ok(response);
        }
    }
}
