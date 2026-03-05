using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface IProductService
    {
        Task<ResponseDTO<IEnumerable<ProductDTO>>> GetProductsAsync();
        Task<ResponseDTO<ProductDTO>> GetProductByIdAsync(int id);
        Task<ResponseDTO<ProductDTO>> CreateProductAsync(ProductDTO productDTO);
        Task<ResponseDTO<ProductDTO>> UpdateProductAsync(int id, ProductDTO productDTO);
        Task<ResponseDTO<bool>> DeleteProductAsync(int id);
    }
}
