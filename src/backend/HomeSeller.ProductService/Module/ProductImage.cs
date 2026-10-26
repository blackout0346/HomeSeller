namespace ProductService.Module;

public class ProductImage
{
    public Guid Id { get; set; }
    public string ImageURL { get; set; }
    public Guid ProductId{ get; set; }
    public Products Products { get; set; } = null!;
}