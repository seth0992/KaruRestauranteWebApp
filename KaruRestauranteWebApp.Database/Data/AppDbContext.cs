using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using KaruRestauranteWebApp.Models.Entities.Restaurant;
using KaruRestauranteWebApp.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace KaruRestauranteWebApp.Database.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshTokenModel>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRolModel>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRolModel>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboDetailModel>()
      .HasOne(cd => cd.Combo)
      .WithMany(f => f.ComboItems)
      .HasForeignKey(cd => cd.ComboID)
      .OnDelete(DeleteBehavior.Restrict); // Evita problemas de eliminación circular

            modelBuilder.Entity<ComboDetailModel>()
                .HasOne(cd => cd.Item)
                .WithMany()
                .HasForeignKey(cd => cd.ItemID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshTokenModel>(entity =>
            {
                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(500)  // Asegúrate que esto coincida con tu columna SQL
                    .IsUnicode(true);   // Para nvarchar                   
            });

            modelBuilder.Entity<ProductInventoryModel>()
         .HasOne(pi => pi.FastFoodItem)
         .WithOne()
         .HasForeignKey<ProductInventoryModel>(pi => pi.FastFoodItemID)
         .OnDelete(DeleteBehavior.Cascade);
        }


        //public DbSet<ProductModel> Products { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<UserRolModel> UserRoles { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ComboDetailModel> ComboDetails { get; set; }
        public DbSet<IngredientModel> Ingredients { get; set; }
        public DbSet<FastFoodItemModel> FastFoodItems { get; set; }
        public DbSet<ItemIngredientModel> ItemIngredients { get; set; }
        public DbSet<InventoryTransactionModel> InventoryTransactions { get; set; }
        public DbSet<ProductInventoryModel> ProductInventory { get; set; }

    }
}
