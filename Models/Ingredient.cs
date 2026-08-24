using System.ComponentModel.DataAnnotations;

namespace Online_Restaurant.Models
{
    public class Ingredient :  BaseEntity
    {
        [Key]
        public int IngredientId { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public ICollection<Menu_Ingredient> Menu_Ingredients { get; set; }
            = new List<Menu_Ingredient>();

        public ICollection<Inventory> Inventories { get; set; }
            = new List<Inventory>();
    }
}