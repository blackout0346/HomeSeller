namespace HomeSellerServer.DTO;

public class ChangePasswordRequest
{
    public string Login { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}