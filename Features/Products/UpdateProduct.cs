namespace VerticalSliceArchitecture.Features.Products
{
    public class UpdateProduct
    {
        public record Request(string Name, decimal Price, int? CategoryId);
        public record Response(int Id, string Name, decimal Price, int? CategoryId);
    }
}
