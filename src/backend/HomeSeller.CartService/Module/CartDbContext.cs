using Microsoft.EntityFrameworkCore;

namespace HomeSeller.CartService.Module;

public class CartDbContext : DbContext
{
    public DbSet<Cart> Cart { get; set; }

    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("SellShop.Cart");
        modelBuilder.Entity<CartItem>(options =>
            options.HasOne(p => p.Cart).WithMany(p => p.CartItems).HasForeignKey(p => p.CartId));
      
    }
}