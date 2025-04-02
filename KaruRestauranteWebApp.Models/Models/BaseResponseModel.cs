namespace KaruRestauranteWebApp.Models.Models
{
    public class BaseResponseModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; } = string.Empty;
        public object? Data { get; set; } = new();
    }
}
