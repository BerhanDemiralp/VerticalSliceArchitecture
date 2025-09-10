namespace VerticalSliceArchitecture.Domain
{
    public class Product
    {
        public int Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public decimal Price { get; init; }
        public int? CategoryId { get; init; }
    }
}
