using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Entities.Users
{
    public class UserModel
    {
        public UserModel()
        {
            UserRoles = new List<UserRolModel>();
        }

        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<UserRolModel> UserRoles { get; set; }
    }
}
