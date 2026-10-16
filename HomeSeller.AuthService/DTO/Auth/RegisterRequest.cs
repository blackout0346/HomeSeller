using System;
using System.Collections.Generic;
using System.Text;

namespace HomeSellerServer.DTO
{
    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;   
        public string Password { get; set; } = string.Empty;
    }
}
