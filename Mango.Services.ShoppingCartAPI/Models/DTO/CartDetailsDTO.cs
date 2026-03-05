using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models.DTO
{
    public class CartDetailsDTO
    {
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }

        public string ProductName { get; set; } = default!;
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
    }
}
