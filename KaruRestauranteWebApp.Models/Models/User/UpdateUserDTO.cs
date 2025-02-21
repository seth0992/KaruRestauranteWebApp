using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaruRestauranteWebApp.Models.Models.User
{
    public class UpdateUserDTO
    {
        [Required]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;  // Lo mantenemos pero será solo lectura en el frontend

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public List<int> RoleIds { get; set; } = new();
    }
}
