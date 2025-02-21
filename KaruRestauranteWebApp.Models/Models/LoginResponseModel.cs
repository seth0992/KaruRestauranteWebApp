using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public long TokenExpired { get; set; }
        public string? RefreshToken { get; set; }
    }
}
