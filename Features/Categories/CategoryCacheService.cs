using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Services.Caching;
using static VerticalSliceArchitecture.Features.Category.Contracts;

namespace VerticalSliceArchitecture.Features.Category
{
    public class CategoryCacheService : ICategoryCache
    {
        // Tekil ürünler için önbellek servisi
        private readonly ICacheService<CategoryDto> _categoryCache;
        // Tüm ürünler için önbellek servisi
        private readonly ICacheService<CategoryListDto> _allCategoriesCache;

        private const string CategoryCacheKeyPrefix = "category:";
        private const string AllCategoriesCacheKey = "categories:all";

        public CategoryCacheService(
            ICacheService<CategoryDto> categoryCache,
            ICacheService<CategoryListDto> allCategoriesCache)
        {
            _categoryCache = categoryCache;
            _allCategoriesCache = allCategoriesCache;
        }

        public Task<CategoryDto?> GetCategoryAsync(int categoryId, CancellationToken ct)
        {
            string cacheKey = $"{CategoryCacheKeyPrefix}{categoryId}";
            return _categoryCache.GetAsync(cacheKey, ct);
        }

        public Task SetCategoryAsync(CategoryDto category, TimeSpan duration, CancellationToken ct)
        {
            string cacheKey = $"{CategoryCacheKeyPrefix}{category.Id}";
            return _categoryCache.SetAsync(cacheKey, category, duration, ct);
        }

        public Task<CategoryListDto?> GetAllCategoriesAsync(CancellationToken ct)
        {
            return _allCategoriesCache.GetAsync(AllCategoriesCacheKey, ct);
        }

        public Task SetAllCategoriesAsync(CategoryListDto categories, TimeSpan duration, CancellationToken ct)
        {
            return _allCategoriesCache.SetAsync(AllCategoriesCacheKey, categories, duration, ct);
        }
        public Task RemoveCategoryAsync(int categoryId, CancellationToken ct)
        {
            string cacheKey = $"{CategoryCacheKeyPrefix}{categoryId}";
            return _categoryCache.RemoveAsync(cacheKey, ct);
        }

        public Task RemoveAllCategoriesAsync(CancellationToken ct)
        {
            return _allCategoriesCache.RemoveAsync(AllCategoriesCacheKey, ct);
        }
    }
}
