using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Module;

public class ProductImage
{
    public Guid Id { get; set; }
    public string ImageURL { get; set; }
    public Guid ProductId{ get; set; }
    [ForeignKey(nameof(ProductId))]
    public Products Products { get; set; } = null!;
}