using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;

namespace HomeSellerServer.StoreService
{
    public class UserStore : IUserStore
    {
        public readonly AppDbContext _appDbContext;

        public UserStore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Users user)
        {
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsByLoginAsync(string login)
        {
            return await _appDbContext.Users.AnyAsync(u => u.Login == login);
        }

        public async Task<bool> UpdateUserAsync(Users updatedUser)
        {
            var existingUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Login == updatedUser.Login);

            if (existingUser == null)
            {
                return false;
            }


            existingUser.HashPassword = updatedUser.HashPassword;
            existingUser.RoleId = updatedUser.RoleId;

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(Users user)
        {
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
            if (existingUser == null || existingUser.isDeleted)
            {
                return false;
            }

            existingUser.isDeleted = true;
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Users?> GetByIdAsync(Guid id)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Users?> GetByLoginAsync(string login)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
        }
    }
}