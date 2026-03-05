using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;

        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO<ProductDTO>> CreateProductAsync(ProductDTO productDTO, string token)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = productDTO,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<bool>> DeleteProductAsync(int id, string token)
        {
            return await this._baseService.SendAsync<bool>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<ProductDTO>> GetProductByIdAsync(int id, string token)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<IEnumerable<ProductDTO>>> GetProductsAsync(string token)
        {
            return await this._baseService.SendAsync<IEnumerable<ProductDTO>>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<ProductDTO>> UpdateProductAsync(int id, ProductDTO productDTO, string token)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDTO,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = token
            });
        }
    }
}
