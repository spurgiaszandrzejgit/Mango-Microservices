using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface IProductService
    {
        Task<ResponseDTO<IEnumerable<ProductDTO>>> GetProductsAsync(string token);
        Task<ResponseDTO<ProductDTO>> GetProductByIdAsync(int id, string token);
        Task<ResponseDTO<ProductDTO>> CreateProductAsync(ProductDTO productDTO, string token);
        Task<ResponseDTO<ProductDTO>> UpdateProductAsync(int id, ProductDTO productDTO, string token);
        Task<ResponseDTO<bool>> DeleteProductAsync(int id, string token);
    }
}
