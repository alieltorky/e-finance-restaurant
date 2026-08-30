namespace Online_Restaurant.ViewModels
{
    public class DeliveryDashboardViewModel
    {
        public List<DeliveryOrderItemVM> AvailableOrders { get; set; } = new();
        public int AvailableTotalCount { get; set; }
        public int AvailablePageNumber { get; set; } = 1;
        public int AvailableTotalPages { get; set; } = 1;

        public List<DeliveryOrderItemVM> MyOrders { get; set; } = new();
        public int MyOrdersTotalCount { get; set; }
        public int MyOrdersPageNumber { get; set; } = 1;
        public int MyOrdersTotalPages { get; set; } = 1;
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