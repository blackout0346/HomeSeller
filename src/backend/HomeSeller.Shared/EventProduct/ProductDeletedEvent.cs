namespace HomeSeller.Shared.EventProduct;

public class ProductDeletedEvent
{
    public Guid ProductId { get; set; }

    public Guid UserId { get; set; }

    public DateTime DeleteAt { get; set; } = DateTime.UtcNow;
}