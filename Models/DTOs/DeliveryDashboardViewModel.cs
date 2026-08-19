namespace Online_Restaurant.ViewModels
{
    public class DeliveryDashboardViewModel
    {
        public List<DeliveryOrderItemVM> AvailableOrders { get; set; } = new();
        public List<DeliveryOrderItemVM> MyOrders { get; set; } = new();
    }

    public class DeliveryOrderItemVM
    {
        public int OrderId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public decimal Price { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public int OrderStatusId { get; set; }
    }
}