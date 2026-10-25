namespace HomeSeller.CartService.DTOs.Cart;

public class CartRequest
{
    
    public  Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}