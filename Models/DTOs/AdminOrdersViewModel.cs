namespace Online_Restaurant.ViewModels
{
    public class AdminOrdersViewModel
    {
        public List<Online_Restaurant.Models.Orders> Orders { get; set; } = new();
        public List<Online_Restaurant.Models.OrderStatus> AllStatuses { get; set; } = new();
        public List<DeliveryStaffOption> DeliveryStaff { get; set; } = new();

        // Current filter values - kept here so the form fields stay filled in after a search
        public string? PhoneNumber { get; set; }
        public int? OrderId { get; set; }
        public string? DeliveryManId { get; set; }

        // Paging
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class DeliveryStaffOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}