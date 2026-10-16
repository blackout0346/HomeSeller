using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;

namespace HomeSellerServer.StoreService;

public class ProductStore : IProductStore
{
    private readonly AppDbContext dbcontext;

    public ProductStore(AppDbContext context)
    {
        this.dbcontext = context;
    }

    public async Task<IEnumerable<Products>> GetProducts(string? search = null)
    {
        var query = dbcontext.Products.Include(p => p.Condition)
            .Include(p => p.User)
            .Include(p => p.ProductTags).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.NameProduct, search));
        }

        return await query.ToListAsync();
    }

    public async Task<Products?> GetProductsIdAsync(Guid id)
    {
        return await dbcontext.Products
            .AsNoTracking()
            .Include(p => p.Condition)
            .Include(p => p.User)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            product.isDeleted = true;
            await dbcontext.SaveChangesAsync();
        }
    }

    public async Task ProductsAddAsync(Products products)
    {
        products.CreateAt = DateTime.UtcNow;
        await dbcontext.Products.AddAsync(products);
        await dbcontext.SaveChangesAsync();
    }

    public async Task ProductsUpdateAsync(Products products)
    {
        var existingProduct = await dbcontext.Products
            .Include(p => p.ProductTags)
            .FirstOrDefaultAsync(p => p.Id == products.Id);

        if (existingProduct == null)
            return;


        existingProduct.NameProduct = products.NameProduct;
        existingProduct.Description = products.Description;
        existingProduct.Price = products.Price;
        existingProduct.ConditionId = products.ConditionId;


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
    }
}