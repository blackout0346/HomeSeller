using System.Security.Claims;
using HomeSeller.CartService.DTOs.Cart;
using HomeSeller.CartService.Interfaces;
using HomeSeller.CartService.Module;


namespace HomeSeller.CartService.Services;

public class CartService : ICartService
{

    private readonly ICartStore _cartstore;

    public CartService(ICartStore cartstore)
    {
        _cartstore = cartstore;
    }

    

    public async Task<CartResponse> GetCartByUserIdAsync(Guid userId)
    {
        var cartByUser = await _cartstore.GetCartByUserIdAsync(userId);
        return MapToResponse(cartByUser, userId);
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, CartRequest request)
    {
        var cartitem = new CartItem
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };
        await _cartstore.AddItemAsync(cartitem, userId);
        var updatedCart = await _cartstore.GetCartByUserIdAsync(userId);
        return MapToResponse(updatedCart, userId);
    }

    public async Task<CartResponse> RemoveItemAsync(Guid userId, Guid productId)
    {
        await _cartstore.RemoveItemAsync(productId, userId);

        var updatedCart = await _cartstore.GetCartByUserIdAsync(userId);
        return MapToResponse(updatedCart, userId);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        await _cartstore.ClearCartAsync(userId);
    }

    private static CartResponse MapToResponse(Cart? cart, Guid userId)
    {
        if (cart == null)
        {
            return new CartResponse
            {
                Id = Guid.Empty,
                UserId = userId,
                CartItems = new List<CartItemResponse>()
            };
        }

        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(i => new CartItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };
    }
    
}