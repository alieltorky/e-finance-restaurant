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


       
    }
}