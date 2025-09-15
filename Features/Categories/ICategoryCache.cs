using static VerticalSliceArchitecture.Features.Category.Contracts;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Category
{
    public interface ICategoryCache
    {
        Task<CategoryDto?> GetCategoryAsync(int categoryId, CancellationToken ct);
        Task SetCategoryAsync(CategoryDto category, TimeSpan duration, CancellationToken ct);

        // GetAllProducts.Response
        Task<CategoryListDto?> GetAllCategoriesAsync(CancellationToken ct);
        Task SetAllCategoriesAsync(CategoryListDto products, TimeSpan duration, CancellationToken ct);

        Task RemoveCategoryAsync(int categoryId, CancellationToken ct);
        Task RemoveAllCategoriesAsync(CancellationToken ct);
    }
}
