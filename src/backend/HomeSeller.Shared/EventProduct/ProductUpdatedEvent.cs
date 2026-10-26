namespace HomeSeller.Shared.EventProduct;

public class ProductUpdatedEvent
{
    public Guid ProductId { get; set; }

    public Guid UserId { get; set; }

    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
}