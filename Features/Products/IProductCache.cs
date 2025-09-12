namespace VerticalSliceArchitecture.Features.Products;

public interface IProductCache
{
    Task<GetProductById.Response?> GetProductAsync(int productId, CancellationToken ct);
    Task SetProductAsync(GetProductById.Response product, TimeSpan duration, CancellationToken ct);

    // GetAllProducts.Response
    Task<GetAllProducts.Response?> GetAllProductsAsync(CancellationToken ct);
    Task SetAllProductsAsync(GetAllProducts.Response products, TimeSpan duration, CancellationToken ct);
}
