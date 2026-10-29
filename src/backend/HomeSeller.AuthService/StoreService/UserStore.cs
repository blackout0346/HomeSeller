using System.Text.Json;
using HomeSeller.Shared.CacheHelper;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace HomeSellerServer.StoreService
{
    public class UserStore : IUserStore
    {
        private readonly IDatabase _redis;
        private readonly ILogger<UserStore> _logger;
        private readonly TimeSpan? _cacheExpiry = TimeSpan.FromHours(2);
        private readonly AppDbContext _appDbContext;


        public UserStore(AppDbContext appDbContext, ILogger<UserStore> logger, IConnectionMultiplexer muxer)
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
            await CacheHelper.InvalidateCacheAsync(_redis, _logger, "user:id");
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
            await CacheHelper.InvalidateCacheAsync(_redis, _logger, "user:id");
            return true;
        }

        public async Task<Users?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"user:id:{id}";
            await CacheHelper.GetCacheAsync<Users>(_redis, _logger, cacheKey);
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                await CacheHelper.SetCacheAsync<Users?>(_redis, _logger, user, cacheKey, _cacheExpiry);
            }

            return user;
        }

        public async Task<Users?> GetByLoginAsync(string login)
        {
            var cacheKey = $"user:login:{login}";
            await CacheHelper.GetCacheAsync<Users?>(_redis, _logger, cacheKey);

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user != null)
            {
                await CacheHelper.SetCacheAsync<Users?>(_redis, _logger, user, cacheKey, _cacheExpiry);
            }

            return user;
        }
    }
}