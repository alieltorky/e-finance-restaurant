using Microsoft.AspNetCore.Identity;

using System.Collections;

namespace Online_Restaurant.Models
{
    public class ApplicationUser : IdentityUser
    {
        public String? Address { get; set; }

        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
     }
}
