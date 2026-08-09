using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Restaurant.Models
{
    public class Ingredient
    {
        [Key]
        public int IngredientId { get; set; }
        public int InventoryId { get; set; }
        public string IngredientName { get; set; } = string.Empty;

        public Inventory Inventory { get; set; } = null!;
        public ICollection<Menu_Ingredient> Menu_Ingredients { get; set; } = new List<Menu_Ingredient>();
    }
}