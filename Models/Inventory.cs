using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        public string InventoryName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public string Unit { get; set; } 

        

        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<SupplyDelivery> SupplyDeliveries { get; set; } = new List<SupplyDelivery>();
    }
}