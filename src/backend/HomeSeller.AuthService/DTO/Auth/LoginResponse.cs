using System;
using System.Collections.Generic;
using System.Text;

namespace HomeSellerServer.DTO
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
