using System;
using System.Collections.Generic;
using System.Text;

namespace HomeSellerServer.Interface
{
    public interface IHashPassword
    {
        public Task<string> HashPasswordAsync(string password);

        public Task<bool> VerifyPasswordAsync(string password, string hashedPassword);

    }
}
