using HomeSellerServer.DTO.Products;
namespace HomeSellerServer.Interface;

public interface IProductService
{
    Task<IEnumerable<ProductsResponse>> GetProductsAsync(string? search = null);
    Task<ProductsResponse?> GetProductByIdAsync(Guid id);
    Task<ProductsResponse> CreateProductAsync(ProductsRequest request);
    Task<ProductsResponse?> UpdateProductsAsync(Guid id, ProductsRequest request);
    Task<bool> DeleteProductsAsync(Guid id);
    
}