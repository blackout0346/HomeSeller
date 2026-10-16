namespace HomeSellerServer.Module;

public class Role
{
    public Guid  Id { get; set; }
    public string RoleName { get; set; }
    public ICollection<Users> Users { get; set; } = new List<Users>();
}