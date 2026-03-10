using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        [Required]
        public string UserId { get; set; } = default!;
        public string? CouponCode { get; set; }
    }
}
