namespace Mango.Web.Models
{
    public class CartDetailsDTO
    {
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }

        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
    }
}
