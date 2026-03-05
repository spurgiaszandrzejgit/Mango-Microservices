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

        public async Task<ResponseDTO<ProductDTO>> CreateProductAsync(ProductDTO productDTO)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = productDTO,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }

        public async Task<ResponseDTO<bool>> DeleteProductAsync(int id)
        {
            return await this._baseService.SendAsync<bool>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = ""
            });
        }

        public async Task<ResponseDTO<ProductDTO>> GetProductByIdAsync(int id)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = ""
            });
        }

        public async Task<ResponseDTO<IEnumerable<ProductDTO>>> GetProductsAsync()
        {
            return await this._baseService.SendAsync<IEnumerable<ProductDTO>>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }

        public async Task<ResponseDTO<ProductDTO>> UpdateProductAsync(int id, ProductDTO productDTO)
        {
            return await this._baseService.SendAsync<ProductDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDTO,
                Url = SD.ProductAPIBase + $"/api/products/{id}",
                AccessToken = ""
            });
        }
    }
}
