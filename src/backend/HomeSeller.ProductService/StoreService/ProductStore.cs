using System.Text.Json;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using ProductService.Module;
using StackExchange.Redis;
namespace ProductService.StoreService;

public class ProductStore : IProductStore
{
    private readonly ProductDbContext dbcontext;
    private readonly IDatabase _redis;

    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    public ProductStore(ProductDbContext context, IConnectionMultiplexer muxer)
    {
        _redis = muxer.GetDatabase();
        this.dbcontext = context;
    }

    public async Task<IEnumerable<Products>> GetProducts(string? search = null)
    {
        var cachekey = $"product:Id";
        var cachejson = await _redis.StringGetAsync(cachekey);
        if (!cachejson.HasValue)
        {
            return JsonSerializer.Deserialize<IEnumerable<Products>>((string)cachejson!);
        }
        
        var query = dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.NameProduct, search));
        }

        if (query != null)
        {
            foreach (var product in query )
            {
                
                await SetCacheAsync(product);
            }
        }
        return await query.ToListAsync();
    }

    public async Task<Products?> GetProductsIdAsync(Guid id)
    {
        var cachekey = $"product:Id:{id}";
        var cachejson = await _redis.StringGetAsync(cachekey);
        if (!cachejson.HasValue)
        {
            return JsonSerializer.Deserialize<Products>((string)cachejson!);
        }
        var product = await dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            SetCacheAsync(product);
        }
        return product;
    }

    public async Task<bool> DeleteProductAsync(Guid UserId, Guid productId)
    {
        var product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == UserId);
        if (product != null)
        {
            product.isDeleted = true;
            await dbcontext.SaveChangesAsync();
            return true;
        }

        await InvalidateCacheAsync(product);
        return false;
    }

    public async Task ProductsAddAsync(Guid UserId, Products products)
    {
        var product = await dbcontext.Products.FirstOrDefaultAsync(p => p.UserId == UserId);
        if (product != null)
        {
            products.CreateAt = DateTime.UtcNow;
            await dbcontext.Products.AddAsync(products);
            await dbcontext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Products>> GetProductsByIdsAsync(IEnumerable<Guid> ids)
    {
        var cachekey = $"product:Id:{ids}";
        var cachejson = await _redis.StringGetAsync(cachekey);
        if (!cachejson.HasValue)
        {
            return JsonSerializer.Deserialize<IEnumerable<Products>>((string)cachejson!);
        }

        var products =await dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .Where(p => ids.Contains(p.Id)).ToListAsync();
        
        if (products != null)
        {
            foreach (var product in products)
            {
                await SetCacheAsync(product);
            }
        }
        return products;
    }

    public async Task ProductsUpdateAsync(Guid UserId, Products products)
    {
        
        var existingProduct = await dbcontext.Products
            .Include(p => p.ProductTags)
            .Include(p => p.ImageURLs)
            .FirstOrDefaultAsync(p => p.Id == products.Id && p.UserId == UserId);

        if (existingProduct == null)
            return;


        existingProduct.NameProduct = products.NameProduct;
        existingProduct.Description = products.Description;
        existingProduct.Price = products.Price;
        existingProduct.ConditionId = products.ConditionId;

        if (products.ImageURLs != null)
        {
            
            dbcontext.Set<ProductImage>().RemoveRange(products.ImageURLs);
            existingProduct.ImageURLs = products.ImageURLs;
        }
        if (products.ProductTags != null)
        {
            var newTagIds = products.ProductTags.Select(pt => pt.TagId).ToHashSet();
            var currentTagIds = existingProduct.ProductTags.Select(pt => pt.TagId).ToHashSet();


            var tagsToRemove = existingProduct.ProductTags
                .Where(pt => !newTagIds.Contains(pt.TagId))
                .ToList();
            dbcontext.ProductTags.RemoveRange(tagsToRemove);


            var tagsToAdd = products.ProductTags
                .Where(pt => !currentTagIds.Contains(pt.TagId))
                .ToList();

            foreach (var tag in tagsToAdd)
            {
                existingProduct.ProductTags.Add(tag);
            }
        }

        await dbcontext.SaveChangesAsync();
        await InvalidateCacheAsync(existingProduct);
    }
    private async Task SetCacheAsync(Products products)
    {
        try
        {
            var json = JsonSerializer.Serialize(products);
            await _redis.StringSetAsync($"products:id{products.Id}", json, _cacheExpiry);
            await _redis.StringSetAsync($"products:NameProduct{products.NameProduct}", json, _cacheExpiry);
            await _redis.StringSetAsync($"products:Description{products.Description}", json, _cacheExpiry);
            await _redis.StringSetAsync($"products:ImageURLs{products.ImageURLs}", json, _cacheExpiry);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    private async Task InvalidateCacheAsync(Products products)
    {
        try
        {
            await _redis.KeyDeleteAsync($"products:id{products.Id}");
            await _redis.KeyDeleteAsync($"products:NameProduct{products.NameProduct}");
            await _redis.KeyDeleteAsync($"products:ImageURLs{products.ImageURLs}");
            await _redis.KeyDeleteAsync($"products:Description{products.Description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}