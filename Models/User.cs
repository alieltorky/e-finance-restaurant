using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } 
        public string UserName { get; set; } 
        public string Email { get; set; }
        public string Password { get; set; } 
        public string PhoneNumber { get; set; } 
        public string Role { get; set; }
        public string Address { get; set; }

        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
    }
}