using HomeSeller.CartService.DTOs.Cart;
using HomeSeller.CartService.Module;

namespace HomeSeller.CartService.Interfaces;

public interface ICartStore
{
    public Task AddItemAsync(CartItem cartItem, Guid userId);
    public Task<bool> RemoveItemAsync(Guid productId, Guid userId);
    public Task<Cart?> GetCartByUserIdAsync(Guid userId);
    public Task ClearCartAsync(Guid userId);
}