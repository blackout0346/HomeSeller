using System;
using System.Threading.Tasks;
using HomeSellerServer.Module;
using ProductService.Module;

namespace HomeSellerServer.Interface;

public interface IProductStore
{
    public Task<IEnumerable<Products>>  GetProducts(string? search=null);
    public Task<Products?> GetProductsIdAsync(Guid id);
    public Task<bool> DeleteProductAsync(Guid UserId,Guid productId);
    public Task ProductsAddAsync(Guid UserId,Products products);
    public Task<IEnumerable<Products>> GetProductsByIdsAsync(IEnumerable<Guid> ids);
    public Task ProductsUpdateAsync(Guid UserId,Products products);
    
}