using HomeSeller.Shared.CacheHelper;
using HomeSeller.Shared.EventUser;
using HomeSellerServer.DTO;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using MassTransit;
using StackExchange.Redis;

namespace HomeSellerServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHashPassword _hashPassword;
        private readonly IUserStore _userStore;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<AuthService> _logger;
        private readonly IDatabase _redis;
        private readonly TimeSpan? _cacheExpiry = TimeSpan.FromMinutes(5);

        private readonly IJwtTokenGenerator _tokenService;

        public AuthService(IHashPassword hashPassword, IUserStore userStore, IPublishEndpoint publishEndpoint,
            IJwtTokenGenerator tokenService, IConnectionMultiplexer muxer, ILogger<AuthService> logger)
        {
            _redis = muxer.GetDatabase();
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _hashPassword = hashPassword;
            _userStore = userStore;
            _tokenService = tokenService;
        }

        async Task<RegisterResponse> IAuthService.RegisterAsync(RegisterRequest registerRequest)
        {
            var exists = await _userStore.ExistsByLoginAsync(registerRequest.Login);
            if (exists)
            {
                throw new Exception("User already exists.");
            }

            var hashpassword = await _hashPassword.HashPasswordAsync(registerRequest.Password);

            var user = new Users
            {
                Login = registerRequest.Login,
                HashPassword = hashpassword,
                RoleId = AppDbContext.BuyerRoleId
            };
            await _userStore.AddAsync(user);
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Login = user.Login,
            });
            return new RegisterResponse
            {
                Login = user.Login,
                UserId = user.Id,
                Token = _tokenService.GenerateToken(user)
            };
        }

        async Task<LoginResponse> IAuthService.LoginAsync(LoginRequest loginRequest)
        {
            var cachekey = "user:id";
            var cachequery = await CacheHelper.GetCacheAsync<LoginResponse>(_redis, _logger, cachekey);
            if (cachequery != null)
            {
                return cachequery;
            }
            var user = await _userStore.GetByLoginAsync(loginRequest.Login);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var hashpaswword = await _hashPassword.VerifyPasswordAsync(loginRequest.Password, user.HashPassword);

            if (!hashpaswword)
            {
                throw new Exception("Invalid login or password.");
            }

            await _publishEndpoint.Publish(new UserLoginEvent()
            {
                UserId = user.Id,
                Login = loginRequest.Login,
            });
            await CacheHelper.SetCacheAsync(_redis, _logger, user , cachekey, _cacheExpiry );
            return new LoginResponse
            {
                Login = user.Login,
                UserId = user.Id,
                Token = _tokenService.GenerateToken(user)
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userStore.GetByLoginAsync(request.Login);
            if (user == null)
            {
                throw new Exception("User not found.");
            }


            var isCurrentPasswordValid =
                await _hashPassword.VerifyPasswordAsync(request.CurrentPassword, user.HashPassword);
            if (!isCurrentPasswordValid)
            {
                throw new Exception("Invalid current password.");
            }


            user.HashPassword = await _hashPassword.HashPasswordAsync(request.NewPassword);


            await _userStore.UpdateUserAsync(user);
            await _publishEndpoint.Publish(new UserChangePasswordEvent()
            {
                UserId = user.Id,
                Login = user.Login,
            });
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userStore.GetByIdAsync(userId);
            if (user == null || user.isDeleted)
            {
                return false;
            }

            var deleted = await _userStore.DeleteUserAsync(user);
            if (!deleted)
            {
                return false;
            }

            await _publishEndpoint.Publish(new UserDeletedEvent
            {
                UserId = user.Id,
            });
            await CacheHelper.InvalidateCacheAsync(_redis, _logger, "user:id");
            return true;
        }
    }
}