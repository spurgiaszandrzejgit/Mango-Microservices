using Mango.Services.ShoppingCartAPI.Protos;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICouponGrpcService
    {
        Task<CouponModel> GetCoupon(string couponCode);
    }
}
