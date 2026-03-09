using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartDetails
    {
        [Key]
        public int CartDetailsId { get; set; }

        public int CartHeaderId { get; set; }

        [ForeignKey(nameof(CartHeaderId))]
        public CartHeader CartHeader { get; set; } = default!;

        public int ProductId { get; set; }

        public int Count { get; set; }

        [Required]
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
    }
}
