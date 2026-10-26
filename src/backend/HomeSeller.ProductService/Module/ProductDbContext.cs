using Microsoft.EntityFrameworkCore;
using ProductService.Module;

namespace HomeSellerServer.Module;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }


    public DbSet<Products> Products { get; set; }
    public DbSet<Condition> Condition { get; set; } 
    public DbSet<Tags> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("SellShop.Product");
       

        modelBuilder.Entity<Tags>(entity =>
        {
            entity.HasOne(t => t.ParentTag)
                .WithMany(t => t.SubTags)
                .HasForeignKey(t => t.ParentTagId)
                .OnDelete(DeleteBehavior.Restrict); 
        });
  

        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasQueryFilter(p => !p.isDeleted);

            entity.Property(p => p.Price)
                .HasColumnType("numeric(18,2)");

            entity.HasOne(p => p.Condition)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ConditionId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

  
        modelBuilder.Entity<ProductTag>(entity =>
        {
         
            entity.HasQueryFilter(pt => !pt.Product.isDeleted );

            entity.HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pt => new { pt.ProductId, pt.TagId })
                .IsUnique();
        });
    }
}