using VerticalSliceArchitecture.Services.Caching;

namespace VerticalSliceArchitecture.Features.Products;

public class ProductCacheService : IProductCache
{
    // Tekil ürünler için önbellek servisi
    private readonly ICacheService<GetProductById.Response> _productCache;
    // Tüm ürünler için önbellek servisi
    private readonly ICacheService<GetAllProducts.Response> _allProductsCache;

    private const string ProductCacheKeyPrefix = "product:";
    private const string AllProductsCacheKey = "products:all";

    public ProductCacheService(
        ICacheService<GetProductById.Response> productCache,
        ICacheService<GetAllProducts.Response> allProductsCache)
    {
        _productCache = productCache;
        _allProductsCache = allProductsCache;
    }

    public Task<GetProductById.Response?> GetProductAsync(int productId, CancellationToken ct)
    {
        string cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        return _productCache.GetAsync(cacheKey, ct);
    }

    public Task SetProductAsync(GetProductById.Response product, TimeSpan duration, CancellationToken ct)
    {
        string cacheKey = $"{ProductCacheKeyPrefix}{product.Id}";
        return _productCache.SetAsync(cacheKey, product, duration, ct);
    }

    public Task<GetAllProducts.Response?> GetAllProductsAsync(CancellationToken ct)
    {
        return _allProductsCache.GetAsync(AllProductsCacheKey, ct);
    }

    public Task SetAllProductsAsync(GetAllProducts.Response products, TimeSpan duration, CancellationToken ct)
    {
        return _allProductsCache.SetAsync(AllProductsCacheKey, products, duration, ct);
    }
}