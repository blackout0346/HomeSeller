
using System;
using System.Collections.Generic;
using System.Text;
using HomeSellerServer.Module;
namespace HomeSellerServer.Interface
{
    public interface IUserStore
    {
        Task<Users?> GetByLoginAsync(string login);
        Task<Users?> GetByIdAsync(Guid id);
        Task AddAsync(Users user);
        Task<bool> ExistsByLoginAsync(string login);
        public Task<bool> UpdateUserAsync(Users updatedUser);
        
        public Task<bool> DeleteUserAsync(Users user);
    }
}
