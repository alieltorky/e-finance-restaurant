using System.ComponentModel.DataAnnotations;

namespace Online_Restaurant.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public int IngredientId { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal Cost { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public Ingredient Ingredient { get; set; } = null!;
    }
}