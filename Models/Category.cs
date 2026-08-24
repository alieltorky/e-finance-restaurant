using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Restaurant.Models
{
    public class Category : BaseEntity
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public ICollection<Menu_Item> MenuItems { get; set; } = new List<Menu_Item>();
    }
}