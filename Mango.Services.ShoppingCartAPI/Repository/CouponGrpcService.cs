using Mango.Services.ShoppingCartAPI.Protos;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CouponGrpcService : ICouponGrpcService
    {
        private readonly CouponProtoService.CouponProtoServiceClient _client;

        public CouponGrpcService(CouponProtoService.CouponProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<CouponModel> GetCoupon(string couponCode)
        {
            return await _client.GetCouponAsync(new GetCouponRequest
            {
                CouponCode = couponCode
            });
        }
    }
}
