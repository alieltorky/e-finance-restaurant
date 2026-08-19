using Microsoft.AspNetCore.Identity;
using System.Collections;
namespace Online_Restaurant.Models
{
    public class ApplicationUser : IdentityUser
    {
        public String? Address { get; set; }

        // Soft delete flag
        public bool IsActive { get; set; } = true;

        public ICollection<Orders> CustomerOrders { get; set; } = new List<Orders>();
        public ICollection<Orders> DeliveryOrders { get; set; } = new List<Orders>();
     }
}
