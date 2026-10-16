using HomeSellerServer.Module;

namespace HomeSellerServer.DTO.Products;

public class ProductsResponse
{
    public Guid Id { get; set; }
    public string NameProduct { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreateAt { get; set; }

    
    public Guid ConditionId { get; set; }
    public string NameCondition { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public string SellerLogin { get; set; } = string.Empty;


    public List<string> Tags { get; set; } = new();
}