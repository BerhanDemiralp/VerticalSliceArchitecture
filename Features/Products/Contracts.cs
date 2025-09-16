namespace VerticalSliceArchitecture.Features.Products
{
    public static class Contracts
    {
        public record ProductDto(int Id, string Name, decimal Price, int? CategoryId);
        public record ProductListDto(IReadOnlyList<ProductDto> Items);
        public record ErrorResponse(string Error);

    }
}
