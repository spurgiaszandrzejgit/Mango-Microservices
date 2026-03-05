using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ShoppingCartAPI.Models.DTO
{
    public class CartHeaderDTO
    {
        public int CartHeaderId { get; set; }
        public string UserId { get; set; } = default!;
        public string? CouponCode { get; set; }
    }
}
