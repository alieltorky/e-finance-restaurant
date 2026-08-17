using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int PaymentMethodId { get; set; }
        public int OrderStatusId { get; set; }
        public string MobileNumber {  get; set; }
        public string Address {  get; set; }
        public ApplicationUser User { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public PaymentMethod PaymentMethod { get; set; } = null!;
        public OrderStatus OrderStatus { get; set; } = null!;
    }
}