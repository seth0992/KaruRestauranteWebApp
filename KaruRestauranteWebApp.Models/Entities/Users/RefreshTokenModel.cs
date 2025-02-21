using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Users
{
    public class RefreshTokenModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string RefreshToken { get; set; } = string.Empty;

        public virtual UserModel User { get; set; }
    }
}
