namespace HomeSellerServer.Module;

public class ProductTag
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Products Product { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tags Tag { get; set; } = null!;
}