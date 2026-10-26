namespace HomeSeller.Shared.EventUser;

public class UserLoginEvent
{
    public Guid UserId { get; set; }
    public string Login { get; set; }
}