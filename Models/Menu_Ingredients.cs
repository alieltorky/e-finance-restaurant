using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class Menu_Ingredient
    {
        [Key]
        public int MenuIngredientId { get; set; }
        public int MenuItemId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }

        public Menu_Item Menu_Item { get; set; } = null!;
        public Ingredient Ingredient { get; set; } = null!;
    }
}