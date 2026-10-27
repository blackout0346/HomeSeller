using System.Text.Json;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace HomeSellerServer.StoreService
{
    public class UserStore : IUserStore
    {
        private readonly IDatabase _redis;
        private readonly AppDbContext _appDbContext;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

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
            var cacheKey = $"user:login:{login.ToLowerInvariant()}";
            if (await _redis.KeyExistsAsync(cacheKey))
            {
                return true;
            }

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
            await InvalidateCacheAsync(existingUser);
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
            await InvalidateCacheAsync(existingUser);
            return true;
        }

        public async Task<Users?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"user:id:{id}";
            var cacheJson = await _redis.StringGetAsync(cacheKey);
            if (cacheJson.HasValue)
            {
                return JsonSerializer.Deserialize<Users>((string)cacheJson!);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                await SetCacheAsync(user);
            }

            return user;
        }

        public async Task<Users?> GetByLoginAsync(string login)
        {
            var cacheKey = $"user:login:{login}";
            var cacheJson = await _redis.StringGetAsync(cacheKey);
            if (cacheJson.HasValue)
            {
                return JsonSerializer.Deserialize<Users>((string)cacheJson!);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user != null)
            {
                await SetCacheAsync(user);
            }

            return user;
        }

        private async Task SetCacheAsync(Users user)
        {
            var json = JsonSerializer.Serialize(user);
            await _redis.StringSetAsync($"user:id{user.Id}", json, _cacheExpiry);
            await _redis.StringSetAsync($"user:login{user.Login}", json, TimeSpan.FromMinutes(5));
        }

        private async Task InvalidateCacheAsync(Users user)
        {
            await _redis.KeyDeleteAsync($"user:id{user.Id}");
            await _redis.KeyDeleteAsync($"user:login{user.Login}");
        }
    }
}