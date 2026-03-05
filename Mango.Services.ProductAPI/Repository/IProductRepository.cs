using Mango.Services.ProductAPI.Models.DTO;

namespace Mango.Services.ProductAPI.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDTO>> GetProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int productId);
        Task<ProductDTO> CreateProductAsync(ProductDTO productDTO);
        Task<ProductDTO?> UpdateProductAsync(ProductDTO productDTO);
        Task<bool> DeleteProductAsync(int productId);
    }
}
