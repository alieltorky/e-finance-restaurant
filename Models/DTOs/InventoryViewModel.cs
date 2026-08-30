namespace Online_Restaurant.ViewModels
{
    public class InventoryIndexViewModel
    {
        public List<Online_Restaurant.Models.Inventory> InventoryRecords { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}