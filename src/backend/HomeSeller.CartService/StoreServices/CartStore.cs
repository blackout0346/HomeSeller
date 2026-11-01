using HomeSeller.CartService.Interfaces;
using HomeSeller.CartService.Module;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using HomeSeller.Shared.CacheHelper;

namespace HomeSeller.CartService.StoreServices;

public class CartStore : ICartStore
{
    CartDbContext _cartDbContext;


    public CartStore(CartDbContext context)
    {

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
            await _cartDbContext.CartItem.AddAsync(newItem);
        }

        await _cartDbContext.SaveChangesAsync();
    }

    public async Task<bool> RemoveItemAsync(Guid productId, Guid userId)
    {
        var cart = await _cartDbContext.Cart.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return false;
        }

        var item = await _cartDbContext.CartItem.FirstOrDefaultAsync(c => c.ProductId == productId);
        if (item != null)
        {
            cart.CartItems.Remove(item);
            await _cartDbContext.SaveChangesAsync();
          
            return true;
        }

        return false;
    }


    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
     
        var cart = await _cartDbContext.Cart.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
      

        return cart;
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await _cartDbContext.Cart.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any())
        {
            return;
        }

        _cartDbContext.RemoveRange(cart.CartItems);

        await _cartDbContext.SaveChangesAsync();
      
    }
}