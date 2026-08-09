using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }
        public string CompanyName { get; set; } 
        public string ContactPerson { get; set; } 
        public string Phone { get; set; } 
        public string Email { get; set; } 
        public string Address { get; set; } 

        public ICollection<SupplyDelivery> SupplyDeliveries { get; set; } = new List<SupplyDelivery>();
    }
}