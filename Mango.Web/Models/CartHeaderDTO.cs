namespace Mango.Web.Models
{
    public class CartHeaderDTO
    {
        public int CartHeaderId { get; set; }
        public string UserId { get; set; } = default!;
        public string? CouponCode { get; set; }
        public double OrderTotal { get; set; }
    }
}
