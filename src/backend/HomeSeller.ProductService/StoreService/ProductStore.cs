
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using ProductService.Module;



namespace ProductService.StoreService;

public class ProductStore : IProductStore
{
    private readonly ProductDbContext _dbcontext;
    public ProductStore(ProductDbContext context)
    {

        this._dbcontext = context;
    }

    public async Task<IEnumerable<Products>> GetProducts(string? search = null)
    {

        
        var query = _dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .OrderBy(p => p.Id)
            .Take(20)
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim().ToLower()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.NameProduct, searchTerm));
        }
        return query;
    }

    public async Task<Products?> GetProductsIdAsync(Guid id)
    {

        var product = await _dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);


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
    
        var products = await _dbcontext.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Take(20)
            .Include(p => p.Condition)
            .Include(p => p.ImageURLs)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .Where(p => ids.Contains(p.Id)).ToListAsync();

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
    }
}