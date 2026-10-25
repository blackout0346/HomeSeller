namespace HomeSeller.CartService.DTOs.Cart;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> CartItems { get; set; } = new(); 

}