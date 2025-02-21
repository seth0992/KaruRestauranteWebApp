using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Users
{
    public class RoleModel
    {
        public int ID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
