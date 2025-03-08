using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.User
{
    public class CreateUserDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public List<int> RoleIds { get; set; } = new();
    }
}
