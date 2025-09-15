namespace VerticalSliceArchitecture.Features.Category
{
    public static class Contracts
    {
        public record CategoryDto(int Id, string Name);
        public record CategoryListDto(IReadOnlyList<CategoryDto> Items);
    }
}
