using Mango.Services.ShoppingCartAPI.Models.DTO;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICartRepository
    {
        Task<CartDTO> GetCartByUserId(string userId);
        Task<CartDTO> UpsertCart(CartDTO cartDTO);      // add/increase item
        Task<CartDTO> SetItemCount(string userId, int productId, int count); // set count
        Task<bool> RemoveFromCart(string userId, int productId);
        Task<bool> ClearCart(string userId);
        Task<bool> ApplyCoupon(string userId, string couponCode);
        Task<bool> RemoveCoupon(string userId);
    }
}
