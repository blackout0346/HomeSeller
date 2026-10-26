using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HomeSellerServer.StoreService
{
    public class UserStore : IUserStore
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _appDbContext;

        public UserStore(AppDbContext appDbContext, IMemoryCache cache)
        {
            _cache = cache;
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
            _cache.TryGetValue(id, out Users user);
            _cache.Set(id, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(1)));
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Users?> GetByLoginAsync(string login)
        {
            _cache.TryGetValue(login, out Users user);
            _cache.Set(login, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(1)));
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);

        }

        private void UserCache(Users user)
        {
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Size = 1
            };
            _cache.Set($"user:id:{user.Id}", user, options);
            _cache.Set($"user:login:{user.Login}", user, options);
        }

        private void clearCache(Users user)
        {
            _cache.Remove($"user:id:{user.Id}");
            _cache.Remove($"user:login:{user.Login}");
        }
    }
}