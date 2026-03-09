using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;

        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO<CartDTO>> GetCartAsync(string userId, string token = "")
        {
            return await _baseService.SendAsync<CartDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPIBase + $"/api/cart?userId={userId}",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<CartDTO>> UpsertCartAsync(CartDTO cartDto, string token = "")
        {
            return await _baseService.SendAsync<CartDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + "/api/cart/upsert",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<CartDTO>> SetItemCountAsync(int productId, int count, string userId, string token = "")
        {
            return await _baseService.SendAsync<CartDTO>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = new { Count = count },
                Url = SD.ShoppingCartAPIBase + $"/api/cart/items/{productId}?userId={userId}",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<bool>> RemoveItemAsync(int productId, string userId, string token = "")
        {
            return await _baseService.SendAsync<bool>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ShoppingCartAPIBase + $"/api/cart/items/{productId}?userId={userId}",
                AccessToken = token
            });
        }

        public async Task<ResponseDTO<bool>> ClearCartAsync(string userId, string token = "")
        {
            return await _baseService.SendAsync<bool>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ShoppingCartAPIBase + $"/api/cart/clear?userId={userId}",
                AccessToken = token
            });
        }
    }
}