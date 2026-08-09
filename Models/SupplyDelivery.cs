using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class SupplyDelivery
    {
        [Key]
        public int DeliveryId { get; set; }
        public int SupplierId { get; set; }
        public int InventoryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal QuantityDelivered { get; set; }
        public decimal Cost { get; set; }

        public Supplier Supplier { get; set; } = null!;
        public Inventory Inventory { get; set; } = null!;
    }
}