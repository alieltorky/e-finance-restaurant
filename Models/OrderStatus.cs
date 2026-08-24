using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class OrderStatus : BaseEntity
    {
        [Key]
        public int OrderStatusId { get; set; }

        public string StatusName { get; set; } = null!;

        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
    }
}
