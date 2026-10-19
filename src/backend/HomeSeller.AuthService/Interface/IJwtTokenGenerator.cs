using HomeSellerServer.Module;

namespace HomeSellerServer.Interface;

public interface IJwtTokenGenerator
{
    string GenerateToken(Users user);
}