namespace ProductService.Module;

public class Tags
{
    public Guid Id { get; set; }
    public string NameTag { get; set; } = string.Empty;

   
    public Guid? ParentTagId { get; set; }

   
    public Tags? ParentTag { get; set; }

  
    public ICollection<Tags> SubTags { get; set; } = new List<Tags>();

   
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}