namespace HomeSeller.CartService.Module;

public class Cart
{
    public  Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItem> CartItems { get; set; } = new();
}