using HomeSellerServer.DTO.Products;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;

namespace HomeSellerServer.Service;

public class ProductService : IProductService
{

    private readonly IProductStore _productStore;

    public ProductService(IProductStore productStore)
    {
        _productStore = productStore;
    }


    public async Task<IEnumerable<ProductsResponse>> GetProductsAsync(string? search = null)
    {
        var products = await _productStore.GetProducts(search);
        return products.Select(MapToResponse);
    }

  
    public async Task<ProductsResponse?> GetProductByIdAsync(Guid id)
    {
        var product = await _productStore.GetProductsIdAsync(id);
        return product == null ? null : MapToResponse(product);
    }


    public async Task<ProductsResponse> CreateProductAsync(ProductsRequest request)
    {
        var newProductId = Guid.NewGuid();

        var product = new Products
        {
            Id = newProductId,
            NameProduct = request.NameProduct,
            Description = request.Description,
            Price = request.Price,
            ConditionId = request.ConditionId,
            UserId = request.UserId,
            CreateAt = DateTime.UtcNow,
            ProductTags = request.TagIds?.Select(tagId => new ProductTag
            {
                Id = Guid.NewGuid(),
                ProductId = newProductId,
                TagId = tagId
            }).ToList() ?? new List<ProductTag>()
        };

        await _productStore.ProductsAddAsync(product);

     
        var createdProduct = await _productStore.GetProductsIdAsync(product.Id);
        return MapToResponse(createdProduct ?? product);
    }


    public async Task<ProductsResponse?> UpdateProductsAsync(Guid id, ProductsRequest request)
    {

        var existing = await _productStore.GetProductsIdAsync(id);
        if (existing == null)
        {
            return null; 
        }

        var productToUpdate = new Products
        {
            Id = id,
            NameProduct = request.NameProduct,
            Description = request.Description,
            Price = request.Price,
            ConditionId = request.ConditionId,
            UserId = existing.UserId, 
            ProductTags = request.TagIds?.Select(tagId => new ProductTag
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                TagId = tagId
            }).ToList() ?? new List<ProductTag>()
        };

        await _productStore.ProductsUpdateAsync(productToUpdate);

     
        var updatedProduct = await _productStore.GetProductsIdAsync(id);
        return updatedProduct == null ? null : MapToResponse(updatedProduct);
    }

  
    public async Task<bool> DeleteProductsAsync(Guid id)
    {
        var existing = await _productStore.GetProductsIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        await _productStore.DeleteProductAsync(id);
        return true;
    }

   
    private static ProductsResponse MapToResponse(Products products) => new()
    {
        Id = products.Id,
        UserId = products.UserId,
        CreateAt = products.CreateAt,
        Description = products.Description,
        Price = products.Price,
        NameProduct = products.NameProduct,
        ConditionId = products.ConditionId,
        NameCondition = products.Condition?.NameCondition ?? string.Empty,
        Tags = products.ProductTags?
            .Where(pt => pt.Tag != null)
            .Select(pt => pt.Tag.NameTag)
            .ToList() ?? new List<string>()
    };
}