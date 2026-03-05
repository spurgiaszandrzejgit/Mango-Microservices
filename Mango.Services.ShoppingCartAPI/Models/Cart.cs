using Mango.Services.ShoppingCartAPI.Models.DTO;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class Cart
    {
        public CartHeaderDTO CartHeader { get; set; } = new();
        public List<CartDetailsDTO> CartDetails { get; set; } = new();
    }
}
