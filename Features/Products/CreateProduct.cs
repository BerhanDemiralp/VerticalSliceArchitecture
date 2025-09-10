using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;
using static VerticalSliceArchitecture.Features.Products.CreateProduct;

namespace VerticalSliceArchitecture.Features.Products
{
    public class CreateProduct
    {
        public record Request(string Name, decimal Price, int? CategoryId);
        public record Response(int Id, string Name, decimal Price, int? CategoryId);

    }
}
