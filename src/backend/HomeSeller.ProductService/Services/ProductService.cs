using System.Diagnostics;
using HomeSeller.Shared.CacheHelper;
using HomeSeller.Shared.EventProduct;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using ProductService.Module;
using MassTransit;
using ProductService.DTO.Products;
using StackExchange.Redis;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly IProductStore _productStore;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductService> _logger;
    private readonly IDatabase _redis;
    private readonly TimeSpan? _cacheExpiry = TimeSpan.FromHours(1);

    public ProductService(IProductStore productStore, IPublishEndpoint publishEndpoint, ILogger<ProductService> logger,
        IConnectionMultiplexer muxer)
    {
        _redis = muxer.GetDatabase();
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _productStore = productStore;
    }


    public async Task<IEnumerable<ProductsResponse>> GetProductsAsync(string? search = null)
    {
        var cachekey = "products:All";
        var timer = Stopwatch.StartNew();

        var cachequery = await CacheHelper.GetCacheAsync<IEnumerable<ProductsResponse>>(_redis, _logger, cachekey);
        if (cachequery != null)
        {
            _logger.LogInformation($"get cache - {timer.ElapsedMilliseconds}");
            timer.Stop();
            return cachequery;
        }

        var products = await _productStore.GetProducts(search);

        var productsresponse = products.Select(MapToResponse).ToList();

        await CacheHelper.SetCacheAsync(_redis, _logger, productsresponse, cachekey, _cacheExpiry);

        timer.Stop();
        _logger.LogInformation($"get no cache - {timer.ElapsedMilliseconds}");
        return productsresponse;
    }


    public async Task<ProductsResponse?> GetProductByIdAsync(Guid id)
    {
        var cachekey = $"products:id{id}";
        var cachequery = await CacheHelper.GetCacheAsync<ProductsResponse?>(_redis, _logger, cachekey);
        if (cachequery != null)
        {
            return cachequery;
        }

        var product = await _productStore.GetProductsIdAsync(id);
        var productresponse = MapToResponse(product);
        await CacheHelper.SetCacheAsync(_redis, _logger, productresponse, cachekey, _cacheExpiry);
        return productresponse;
    }

    public async Task<ProductsResponse> CreateProductAsync(Guid userId, ProductsRequest request)
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
            ImageURLs = request.ImageURLs.Where(img => !string.IsNullOrWhiteSpace(img)).Select(img => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = newProductId,
                ImageURL = img.Trim()
            }).ToList() ?? new List<ProductImage>(),

            ProductTags = request.TagIds?.Select(tagId => new ProductTag
            {
                Id = Guid.NewGuid(),
                ProductId = newProductId,
                TagId = tagId
            }).ToList() ?? new List<ProductTag>()
        };

        await _productStore.ProductsAddAsync(userId, product);
        await _publishEndpoint.Publish(new ProductCreatedEvent()
        {
            ProductId = newProductId,
            UserId = request.UserId,
            CreateAt = DateTime.UtcNow,
        });

        var createdProduct = await _productStore.GetProductsIdAsync(product.Id);
        return MapToResponse(createdProduct ?? product);
    }

    public async Task<ProductsResponse?> UpdateProductsAsync(Guid userId, Guid productid, ProductsRequest request)
    {
        var existing = await _productStore.GetProductsIdAsync(productid);
        if (existing == null)
        {
            return null;
        }

        var productToUpdate = new Products
        {
            Id = productid,
            NameProduct = request.NameProduct,
            Description = request.Description,
            Price = request.Price,
            ConditionId = request.ConditionId,
            UserId = existing.UserId,
            ImageURLs = request.ImageURLs.Where(img => !string.IsNullOrWhiteSpace(img)).Select(img => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productid,
                ImageURL = img.Trim()
            }).ToList() ?? new List<ProductImage>(),

            ProductTags = request.TagIds?.Select(tagId => new ProductTag
            {
                Id = Guid.NewGuid(),
                ProductId = productid,
                TagId = tagId
            }).ToList() ?? new List<ProductTag>()
        };

        await _productStore.ProductsUpdateAsync(userId, productToUpdate);

        await _publishEndpoint.Publish(new ProductUpdatedEvent()
        {
            ProductId = productid,
            UserId = request.UserId,
            UpdateAt = DateTime.UtcNow,
        });
        var updatedProduct = await _productStore.GetProductsIdAsync(productid);
        await CacheHelper.InvalidateCacheAsync(_redis, _logger, "products:All");
        return updatedProduct == null ? null : MapToResponse(updatedProduct);
    }

    public async Task<bool> DeleteProductsAsync(Guid userId, Guid productid)
    {
        var existing = await _productStore.GetProductsIdAsync(productid);
        if (existing == null)
        {
            return false;
        }

        await _publishEndpoint.Publish(new ProductDeletedEvent()
        {
            ProductId = productid,
            UserId = userId,
            DeleteAt = DateTime.UtcNow,
        });
        await _productStore.DeleteProductAsync(userId, productid);
        await CacheHelper.InvalidateCacheAsync(_redis, _logger, "products:All");
        await CacheHelper.InvalidateCacheAsync(_redis, _logger, "products:id");
        return true;
    }
    
    private static ProductsResponse MapToResponse(Products products) => new()
    {
        Id = products.Id,
        UserId = products.UserId,
        CreateAt = products.CreateAt,
        Description = products.Description,
        Price = products.Price,
        ImageURLs = products.ImageURLs?.Select(img => img.ImageURL).ToList(),
        NameProduct = products.NameProduct,
        ConditionId = products.ConditionId,
        NameCondition = products.Condition?.NameCondition ?? string.Empty,
        Tags = products.ProductTags?
            .Where(pt => pt.Tag != null)
            .Select(pt => pt.Tag.NameTag)
            .ToList() ?? new List<string>()
    };
}