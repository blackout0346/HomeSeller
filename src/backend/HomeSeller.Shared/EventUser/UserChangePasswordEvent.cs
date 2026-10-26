namespace HomeSeller.Shared.EventUser;

public class UserChangePasswordEvent
{
    public Guid UserId { get; set; }
    public string Login { get; set; }
}