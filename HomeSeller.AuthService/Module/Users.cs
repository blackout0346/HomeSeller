namespace HomeSellerServer.Module;

public class Users
{
    public Guid  Id { get; set; }
    public string Login { get; set; }
    public string HashPassword { get; set; }
    public Role Role { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public ICollection<Products> Products { get; set; } = new List<Products>();
    public bool isDeleted { get; set; } = false;
}