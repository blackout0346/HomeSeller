using HomeSeller.Shared.CacheHelper;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using ProductService.Module;
using StackExchange.Redis;


namespace ProductService.StoreService;

public class ProductStore : IProductStore
{
    private readonly ProductDbContext _dbcontext;
    private readonly IDatabase _redis;
    private readonly ILogger<ProductStore> _logger;
    private readonly TimeSpan? _cacheExpiry = TimeSpan.FromHours(2);

    public ProductStore(ProductDbContext context, IConnectionMultiplexer muxer, ILogger<ProductStore> logger)
    {
        _redis = muxer.GetDatabase();
        _logger = logger;
        this._dbcontext = context;
    }

    public async Task<IEnumerable<Products>> GetProducts(string? search = null)
    {
        var cachekey = "product:Id";
        await CacheHelper.GetCacheAsync<IEnumerable<Products>>(_redis, _logger, cachekey);
        var query = _dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.NameProduct, search));
        }

        if (query != null)
        {
            foreach (var product in query)
            {
                await CacheHelper.SetCacheAsync(_redis, _logger, product, cachekey, _cacheExpiry);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<Products?> GetProductsIdAsync(Guid id)
    {
        var cachekey = $"product:Id:{id}";
        await CacheHelper.GetCacheAsync<Products?>(_redis, _logger, cachekey);

        var product = await _dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            await CacheHelper.SetCacheAsync(_redis, _logger, product, cachekey, _cacheExpiry);
        }

        return product;
    }

    public async Task<bool> DeleteProductAsync(Guid UserId, Guid productId)
    {
        var product = await _dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == UserId);
        if (product != null)
        {
            product.isDeleted = true;
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        await CacheHelper.InvalidateCacheAsync(_redis, _logger, "product:Id");
        return false;
    }

    public async Task ProductsAddAsync(Guid UserId, Products products)
    {
        var product = await _dbcontext.Products.FirstOrDefaultAsync(p => p.UserId == UserId);
        if (product != null)
        {
            products.CreateAt = DateTime.UtcNow;
            await _dbcontext.Products.AddAsync(products);
            await _dbcontext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Products>> GetProductsByIdsAsync(IEnumerable<Guid> ids)
    {
        var cachekey = $"product:Id:{ids}";
        await CacheHelper.GetCacheAsync<IEnumerable<Products>>(_redis, _logger, cachekey);

        var products = await _dbcontext.Products
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
                await CacheHelper.SetCacheAsync(_redis, _logger, product, cachekey, _cacheExpiry);
            }
        }

        return products;
    }

    public async Task ProductsUpdateAsync(Guid UserId, Products products)
    {
        var existingProduct = await _dbcontext.Products
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
            _dbcontext.Set<ProductImage>().RemoveRange(products.ImageURLs);
            existingProduct.ImageURLs = products.ImageURLs;
        }

        if (products.ProductTags != null)
        {
            var newTagIds = products.ProductTags.Select(pt => pt.TagId).ToHashSet();
            var currentTagIds = existingProduct.ProductTags.Select(pt => pt.TagId).ToHashSet();


            var tagsToRemove = existingProduct.ProductTags
                .Where(pt => !newTagIds.Contains(pt.TagId))
                .ToList();
            _dbcontext.ProductTags.RemoveRange(tagsToRemove);


            var tagsToAdd = products.ProductTags
                .Where(pt => !currentTagIds.Contains(pt.TagId))
                .ToList();

            foreach (var tag in tagsToAdd)
            {
                existingProduct.ProductTags.Add(tag);
            }
        }

        await _dbcontext.SaveChangesAsync();
        await CacheHelper.InvalidateCacheAsync(_redis, _logger, "product:Id");
    }
}