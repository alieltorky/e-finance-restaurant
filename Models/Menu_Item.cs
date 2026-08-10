using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class Menu_Item
    {
        [Key]
        public int MenuItemId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public bool Available { get; set; }
        public int CategoryId { get; set; }

        public ICollection<OrderDetail> OrderDetail { get; set; } = new List<OrderDetail>();
        public ICollection<Menu_Ingredient> Menu_Ingredients { get; set; } = new List<Menu_Ingredient>();
        public Category Category { get; set; } = null!;
    }
}