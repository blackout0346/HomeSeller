using Microsoft.EntityFrameworkCore;

namespace HomeSellerServer.Module;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Condition> Condition { get; set; } 
    public DbSet<Tags> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("SellShop");
        var transportRootId   = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var electronicsRootId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var business360RootId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        modelBuilder.Entity<Role>().HasData(
            new Role 
            { 
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                RoleName = "Seller" 
            },
            new Role 
            { 
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), 
                RoleName = "Buyer" 
            }
        );

        modelBuilder.Entity<Condition>().HasData(
            new Condition
            { 
                Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), 
                NameCondition = "Новое" 
            },
            new Condition
            { 
                Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), 
                NameCondition = "Б/У " 
            },
   
            new Condition
            { 
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), 
                NameCondition = "Требует ремонта / На запчасти" 
            }
        );


        modelBuilder.Entity<Tags>(entity =>
        {
            entity.HasOne(t => t.ParentTag)
                .WithMany(t => t.SubTags)
                .HasForeignKey(t => t.ParentTagId)
                .OnDelete(DeleteBehavior.Restrict); 
        });
        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasQueryFilter(u => !u.isDeleted);

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasQueryFilter(p => !p.isDeleted && !p.User.isDeleted);

            entity.Property(p => p.Price)
                .HasColumnType("numeric(18,2)");

            entity.HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Condition)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ConditionId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

  
        modelBuilder.Entity<ProductTag>(entity =>
        {
         
            entity.HasQueryFilter(pt => !pt.Product.isDeleted && !pt.Product.User.isDeleted);

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