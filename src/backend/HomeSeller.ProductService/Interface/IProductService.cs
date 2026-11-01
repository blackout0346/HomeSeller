using ProductService.DTO.Products;

namespace HomeSellerServer.Interface;

public interface IProductService
{
    Task<IEnumerable<ProductsResponse>> GetProductsAsync(string? search = null);
    Task<ProductsResponse?> GetProductByIdAsync(Guid productid);
    Task<ProductsResponse> CreateProductAsync(Guid userId, ProductsRequest request);
    Task<ProductsResponse?> UpdateProductsAsync(Guid userId, Guid productid, ProductsRequest request);
    Task<bool> DeleteProductsAsync(Guid userId, Guid productid);
}