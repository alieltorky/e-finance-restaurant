using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class OrderDetail : BaseEntity
    {
        [Key]
        public int DetailId { get; set; }
        public int OrderId { get; set; }
        public int Menu_ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public Orders Order { get; set; } = null!;
        public Menu_Item Menu_Item { get; set; } = null!;
    }
}