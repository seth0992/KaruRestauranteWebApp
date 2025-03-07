using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.User
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
