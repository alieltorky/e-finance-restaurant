//DTOs : that define the data transfered form local storage into controller
namespace Online_Restaurant.Models.DTOs
{
    public class CheckoutRequest
    {
        public List<CartItemRequest> Items { get; set; } = new();
        public int PaymentMethodId { get; set; }
    }

    public class CartItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}