using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Models;
using System.Security.Claims;

namespace Online_Restaurant.Data
{
    public class AppdbContext : IdentityDbContext<ApplicationUser>
    {
       private readonly IHttpContextAccessor _httpContextAccessor;

        public AppdbContext(IHttpContextAccessor httpContextAccessor, DbContextOptions<AppdbContext> options)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
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
        private void ApplyReportingInformation()
        {
            
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                                ?? "System";

            var currentTime = DateTime.Now;

           
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                     // 3lshan don't override Original Data 
                     // Get entity using query then map entity to all functions but this is better because it is more genaric 
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;

                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUserId;
                }
            }
        }

        public override int SaveChanges()
        {
            ApplyReportingInformation();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyReportingInformation();
            return base.SaveChangesAsync(cancellationToken);
        }



    }
}