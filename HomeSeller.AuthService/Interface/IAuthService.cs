
using System;
using System.Collections.Generic;
using System.Text;
using HomeSellerServer.DTO;

namespace HomeSellerServer.Module
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest);

        public Task<bool> DeleteUserAsync(Guid userId);
    }
}
