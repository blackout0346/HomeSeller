using System.Text.Json;
using HomeSeller.CartService.Interfaces;
using HomeSeller.CartService.Module;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
namespace HomeSeller.CartService.StoreServices;

public class CartStore : ICartStore
{
    CartDbContext _cartDbContext;
    private readonly IDatabase _redis;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    public CartStore(CartDbContext context, IConnectionMultiplexer muxer)
    {
        _redis = muxer.GetDatabase();
        _cartDbContext = context;
    }

    public async Task AddItemAsync(CartItem cartItem, Guid userId)
    {
        var cart = await _cartDbContext.Cart
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            await _cartDbContext.Cart.AddAsync(cart);
        }
        var existingItem = cart.CartItems
            .FirstOrDefault(i => i.ProductId == cartItem.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += cartItem.Quantity;
        }
        else
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity
            };
            await  _cartDbContext.CartItem.AddAsync(newItem);
        }
        await _cartDbContext.SaveChangesAsync();
    }

    public async Task<bool> RemoveItemAsync(Guid productId, Guid userId)
    {
        
     var cart = await  _cartDbContext.Cart.Include(c=>c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);

     if (cart == null)
     {
         return false;
     }
     var item = await  _cartDbContext.CartItem.FirstOrDefaultAsync(c => c.ProductId == productId);
     if(item != null)
     {
         cart.CartItems.Remove(item);
         await  _cartDbContext.SaveChangesAsync();
        await InvalidateCacheAsync(cart);
         return true;
     }
     
     return false;
    }



    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        var cacheKey = $"cart:id:{userId}";
        var cacheJson =await _redis.StringGetAsync(cacheKey);
        if (!cacheJson.HasValue)
        {
            return JsonSerializer.Deserialize<Cart>((string)cacheJson!);
        }
        
       var cart = await _cartDbContext.Cart.Include(c=>c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
       if (cart != null)
       {
           await SetCacheAsync(cart);
       }
       return cart;
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await  _cartDbContext.Cart.Include(c=>c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any())
        {
            return;
        }
        _cartDbContext.RemoveRange(cart.CartItems);
        
        await  _cartDbContext.SaveChangesAsync();
        await InvalidateCacheAsync(cart);
    }
    private async Task SetCacheAsync(Cart cart)
    {
        try
        {
           
            var json = JsonSerializer.Serialize(cart);
            await _redis.StringSetAsync($"cart:id{cart.Id}", json, _cacheExpiry);
            await _redis.StringSetAsync($"cart:CartItems{cart.CartItems}", json, _cacheExpiry);
         
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    private async Task InvalidateCacheAsync(Cart cart)
    {
        try
        {
            await _redis.KeyDeleteAsync($"cart:id{cart.Id}");
            await _redis.KeyDeleteAsync($"cart:CartItems{cart.CartItems}");
        
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}