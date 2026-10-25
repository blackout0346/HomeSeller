using HomeSeller.CartService.DTOs.Cart;

namespace HomeSeller.CartService.Interfaces;

public interface ICartService
{
    public Task<CartResponse> GetCartByUserIdAsync(Guid userId);
    public Task<CartResponse> AddItemAsync(Guid userId, CartRequest request);
    public Task<CartResponse> RemoveItemAsync(Guid userId, Guid productId);
    public Task ClearCartAsync(Guid userId);
}