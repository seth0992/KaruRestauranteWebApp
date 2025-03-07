using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Entities.Restaurant
{
    public class ProductTypeModel
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
