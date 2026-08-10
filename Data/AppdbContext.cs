using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Models;
namespace Online_Restaurant.Data

{
    public class AppdbContext : DbContext
    {
        public AppdbContext(DbContextOptions<AppdbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Menu_Item> MenuItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Menu_Ingredient> MenuIngredients { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplyDelivery> SupplyDeliveries { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }

    }
}
