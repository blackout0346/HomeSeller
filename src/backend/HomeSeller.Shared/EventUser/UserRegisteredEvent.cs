namespace HomeSeller.Shared.EventUser;

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Login { get; set; } = string.Empty;
}

