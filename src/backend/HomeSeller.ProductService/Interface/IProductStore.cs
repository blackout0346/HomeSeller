using System;
using System.Threading.Tasks;
using HomeSellerServer.Module;
namespace HomeSellerServer.Interface;

public interface IProductStore
{
    public Task<IEnumerable<Products>>  GetProducts(string? search=null);
    public Task<Products?> GetProductsIdAsync(Guid id);
    public Task DeleteProductAsync(Guid id);
    public Task ProductsAddAsync(Products products);
    
    public Task ProductsUpdateAsync(Products products);
    
}