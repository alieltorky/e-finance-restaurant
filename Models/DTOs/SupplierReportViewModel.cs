namespace Online_Restaurant.ViewModels
{
    public class SupplierReportViewModel
    {
        public List<Online_Restaurant.Models.Inventory> Records { get; set; } = new();
        public List<Online_Restaurant.Models.Supplier> Suppliers { get; set; } = new();
        public int? SelectedSupplierId { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
    }
}