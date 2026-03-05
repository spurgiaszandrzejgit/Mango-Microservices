namespace Mango.Services.ShoppingCartAPI.Models.DTO
{
    public class CartDTO
    {
        public CartHeaderDTO CartHeader { get; set; } = new();
        public List<CartDetailsDTO> CartDetails { get; set; } = new();
    }
}
