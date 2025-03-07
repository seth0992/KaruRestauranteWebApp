using System.ComponentModel.DataAnnotations;

namespace KaruRestauranteWebApp.Models.Models.Orders
{
    public class TableDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El número de mesa es requerido")]
        public int TableNumber { get; set; }

        [Required(ErrorMessage = "La capacidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Status { get; set; } = "Available";

        [StringLength(50, ErrorMessage = "La ubicación no puede exceder los 50 caracteres")]
        public string Location { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
