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

            modelBuilder.Entity<RefreshTokenModel>(entity =>
            {
                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(500)  // Asegúrate que esto coincida con tu columna SQL
                    .IsUnicode(true);   // Para nvarchar                   
            });



            // Configuración FastFoodItem
            modelBuilder.Entity<FastFoodItemModel>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(f => f.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración ProductInventory (relación uno a uno)
            modelBuilder.Entity<ProductInventoryModel>()
                .HasOne(pi => pi.FastFoodItem)
                .WithOne(f => f.Inventory)
                .HasForeignKey<ProductInventoryModel>(pi => pi.FastFoodItemID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración ItemIngredient
            modelBuilder.Entity<ItemIngredientModel>()
                .HasOne(ii => ii.FastFoodItem)
                .WithMany(f => f.Ingredients)
                .HasForeignKey(ii => ii.FastFoodItemID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemIngredientModel>()
                .HasOne(ii => ii.Ingredient)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.IngredientID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración Combo
            modelBuilder.Entity<ComboModel>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Combo)
                .HasForeignKey(ci => ci.ComboID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración ComboItem
            modelBuilder.Entity<ComboItemModel>()
                .HasOne(ci => ci.FastFoodItem)
                .WithMany()
                .HasForeignKey(ci => ci.FastFoodItemID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración InventoryTransaction
            modelBuilder.Entity<InventoryTransactionModel>()
                .HasOne(it => it.Ingredient)
                .WithMany(i => i.Transactions)
                .HasForeignKey(it => it.IngredientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransactionModel>()
                .HasOne(it => it.User)
                .WithMany()
                .HasForeignKey(it => it.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuraciones de propiedades
            modelBuilder.Entity<FastFoodItemModel>()
                .Property(f => f.SellingPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ProductInventoryModel>()
                .Property(pi => pi.PurchasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<IngredientModel>()
                .Property(i => i.PurchasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ComboModel>()
                .Property(c => c.RegularPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ComboModel>()
                .Property(c => c.SellingPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ComboModel>()
                       .Property(c => c.DiscountPercentage)
                       .HasPrecision(5, 2);

            modelBuilder.Entity<FastFoodItemModel>()
    .HasOne(f => f.Category)
    .WithMany(c => c.Items)
    .HasForeignKey(f => f.CategoryID)
    .OnDelete(DeleteBehavior.Restrict);
        }


        //public DbSet<ProductModel> Products { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<UserRolModel> UserRoles { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        //public DbSet<ComboDetailModel> ComboDetails { get; set; }
        public DbSet<IngredientModel> Ingredients { get; set; }
        public DbSet<FastFoodItemModel> FastFoodItems { get; set; }
        public DbSet<ItemIngredientModel> ItemIngredients { get; set; }
        public DbSet<InventoryTransactionModel> InventoryTransactions { get; set; }
        public DbSet<ProductInventoryModel> ProductInventory { get; set; }   
        public DbSet<ComboModel> Combos { get; set; }
        public DbSet<ComboItemModel> ComboItems { get; set; }
        public DbSet<ProductTypeModel> ProductTypes { get; set; }

    }
}
