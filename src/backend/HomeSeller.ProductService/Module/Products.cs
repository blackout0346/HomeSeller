
namespace HomeSellerServer.Module;

public class Products
{
   
    public Guid Id { get; set; }
    public string NameProduct { get; set; }
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    public Guid ConditionId { get; set; }
    public Condition Condition{get;set;} = null!;
    public Guid UserId {get;set;}
  
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public bool isDeleted { get; set; } = false;
}