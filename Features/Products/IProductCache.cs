namespace VerticalSliceArchitecture.Features.Products;
using static VerticalSliceArchitecture.Features.Products.Contracts;

public interface IProductCache
{
    Task<ProductDto?> GetProductAsync(int productId, CancellationToken ct);
    Task SetProductAsync(ProductDto product, TimeSpan duration, CancellationToken ct);

    // GetAllProducts.Response
    Task<ProductListDto?> GetAllProductsAsync(CancellationToken ct);
    Task SetAllProductsAsync(ProductListDto products, TimeSpan duration, CancellationToken ct);

    Task RemoveProductAsync(int productId, CancellationToken ct);
    Task RemoveAllProductsAsync(CancellationToken ct);
}
