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
        public record Response(int Id, string Name, decimal Price, int? CategoryId);
    }
}
