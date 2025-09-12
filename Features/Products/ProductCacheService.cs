using VerticalSliceArchitecture.Services.Caching;
using static VerticalSliceArchitecture.Features.Products.Contracts;

namespace VerticalSliceArchitecture.Features.Products;

public class ProductCacheService : IProductCache
{
    // Tekil ürünler için önbellek servisi
    private readonly ICacheService<ProductDto> _productCache;
    // Tüm ürünler için önbellek servisi
    private readonly ICacheService<ProductListDto> _allProductsCache;

    private const string ProductCacheKeyPrefix = "product:";
    private const string AllProductsCacheKey = "products:all";

    public ProductCacheService(
        ICacheService<ProductDto> productCache,
        ICacheService<ProductListDto> allProductsCache)
    {
        _productCache = productCache;
        _allProductsCache = allProductsCache;
    }

    public Task<ProductDto?> GetProductAsync(int productId, CancellationToken ct)
    {
        string cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        return _productCache.GetAsync(cacheKey, ct);
    }

    public Task SetProductAsync(ProductDto product, TimeSpan duration, CancellationToken ct)
    {
        string cacheKey = $"{ProductCacheKeyPrefix}{product.Id}";
        return _productCache.SetAsync(cacheKey, product, duration, ct);
    }

    public Task<ProductListDto?> GetAllProductsAsync(CancellationToken ct)
    {
        return _allProductsCache.GetAsync(AllProductsCacheKey, ct);
    }

    public Task SetAllProductsAsync(ProductListDto products, TimeSpan duration, CancellationToken ct)
    {
        return _allProductsCache.SetAsync(AllProductsCacheKey, products, duration, ct);
    }
    public Task RemoveProductAsync(int productId, CancellationToken ct)
    {
        string cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        return _productCache.RemoveAsync(cacheKey, ct);
    }

    public Task RemoveAllProductsAsync(CancellationToken ct)
    {
        return _allProductsCache.RemoveAsync(AllProductsCacheKey, ct);
    }

}