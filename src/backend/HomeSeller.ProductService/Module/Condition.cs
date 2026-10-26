using HomeSellerServer.Module;

namespace ProductService.Module;

public class Condition
{
    public Guid  Id{get;set;}
    public string NameCondition{get;set;}
    public ICollection<Products> Products { get; set; } = new List<Products>();
}