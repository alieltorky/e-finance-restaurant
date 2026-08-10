using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        public string MethodName { get; set; } = null!;

        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
    }
}