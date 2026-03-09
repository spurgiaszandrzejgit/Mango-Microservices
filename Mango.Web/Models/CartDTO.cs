namespace Mango.Web.Models
{
    public class CartDTO
    {
        public CartHeaderDTO CartHeader { get; set; } = new();
        public List<CartDetailsDTO> CartDetails { get; set; } = new();
    }
}
