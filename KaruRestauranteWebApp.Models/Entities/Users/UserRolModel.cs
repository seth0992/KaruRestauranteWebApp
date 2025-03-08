namespace KaruRestauranteWebApp.Models.Entities.Users
{
    public class UserRolModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }

        public virtual RoleModel Role { get; set; } = null!;
        public virtual UserModel User { get; set; } = null!;
    }
}
