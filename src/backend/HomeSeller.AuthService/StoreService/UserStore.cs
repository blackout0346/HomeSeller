using System.Text.Json;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace HomeSellerServer.StoreService
{
    public class UserStore : IUserStore
    {
        private readonly IDatabase _redis;
        private readonly AppDbContext _appDbContext;

        public UserStore(AppDbContext appDbContext, IConnectionMultiplexer muxer)
        {
            _redis = muxer.GetDatabase();
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

        private async Task SetCacheAsync(Users user)
        {
            var json = JsonSerializer.Serialize(user);
            await _redis.StringSetAsync($"user:id{user.Id}", json, TimeSpan.FromMinutes(5));
            await _redis.StringSetAsync($"user:login{user.Login}", json, TimeSpan.FromMinutes(5));
            

        }

        private async Task InvalidateCacheAsync(Users user)
        {
            await _redis.KeyDeleteAsync($"user:id{user.Id}");
            await _redis.KeyDeleteAsync($"user:login{user.Login}");
        }
    }
}