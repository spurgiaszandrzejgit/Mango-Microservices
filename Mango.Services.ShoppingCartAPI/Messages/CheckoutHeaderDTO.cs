using Mango.Services.ShoppingCartAPI.Models.DTO;

namespace Mango.Services.ShoppingCartAPI.Messages
{
    public class CheckoutHeaderDTO
    {
        public int CartHeaderId { get; set; }
        public string UserId { get; set; } = default!;
        public string? CouponCode { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal Discount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime PickupDateTime { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CardNumber { get; set; }
        public string? CVV { get; set; }
        public string? ExpiryMonthYear { get; set; }
        public int CartTotalItems { get; set; }
        public List<CartDetailsDTO> CartDetails { get; set; } = new();
    }
}
