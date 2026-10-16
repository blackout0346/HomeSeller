
using System;
using System.Collections.Generic;
using System.Text;
using HomeSellerServer.DTO;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;


namespace HomeSellerServer.Service
{
    public class AuthService : IAuthService
    {
   
        private readonly IHashPassword _hashPassword;
        private readonly IUserStore _userStore;

        public AuthService(IHashPassword hashPassword, IUserStore userStore)
        {
        
            _hashPassword = hashPassword;
            _userStore = userStore;
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
                HashPassword = hashpassword
            };
            await _userStore.AddAsync(user);

            return new RegisterResponse
            {
                Login = user.Login,
                UserId = user.Id,
       
            };

        }
        async Task<LoginResponse> IAuthService.LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userStore.GetByLoginAsync(loginRequest.Login);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            var hashpaswword =await _hashPassword.VerifyPasswordAsync(loginRequest.Password, user.HashPassword);

            if(!hashpaswword)
            {
                throw new Exception("Invalid login or password.");
            }

            return new LoginResponse
            {
                Login = user.Login,
                UserId = user.Id,
             
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userStore.GetByLoginAsync(request.Login);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

     
            var isCurrentPasswordValid = await _hashPassword.VerifyPasswordAsync(request.CurrentPassword, user.HashPassword);
            if (!isCurrentPasswordValid)
            {
                throw new Exception("Invalid current password.");
            }

     
            user.HashPassword = await _hashPassword.HashPasswordAsync(request.NewPassword);


            await _userStore.UpdateUserAsync(user);

            return true;
        }
    }
}
