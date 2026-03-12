using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface ICartService
    {
        Task<ResponseDTO<CartDTO>> GetCartAsync(string userId, string token = "");
        Task<ResponseDTO<CartDTO>> UpsertCartAsync(CartDTO cartDto, string token = "");
        Task<ResponseDTO<CartDTO>> SetItemCountAsync(int productId, int count, string userId, string token = "");
        Task<ResponseDTO<bool>> RemoveItemAsync(int productId, string userId, string token = "");
        Task<ResponseDTO<bool>> ClearCartAsync(string userId, string token = "");
        Task<ResponseDTO<bool>> ApplyCouponAsync(string userId, string couponCode, string token);
    }
}
