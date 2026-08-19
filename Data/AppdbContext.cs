using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Online_Restaurant.Models;

namespace Online_Restaurant.Data
{
    public class AppdbContext : IdentityDbContext<ApplicationUser>
    {
        public AppdbContext(DbContextOptions<AppdbContext> options)
            : base(options)
        {
        }


        public DbSet<Orders> Orders { get; set; }
        public DbSet<Menu_Item> MenuItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Menu_Ingredient> MenuIngredients { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // this is used to Base function do its work 

            // Customer to Orders relationship
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.User)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery man to Orders relationship
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.DeliveryMan)
                .WithMany(u => u.DeliveryOrders)
                .HasForeignKey(o => o.DeliveryManId)
                .OnDelete(DeleteBehavior.Restrict);

            // the delete if restricted for the sake of not loosing the data 
        }



    }
}