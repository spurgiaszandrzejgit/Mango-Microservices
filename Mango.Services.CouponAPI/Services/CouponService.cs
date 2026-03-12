using Grpc.Core;
using Mango.Services.CouponAPI.DbContexts;
using Mango.Services.CouponAPI.Protos;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Services
{
    public class CouponService : CouponProtoService.CouponProtoServiceBase
    {
        private readonly ApplicationDbContext _db;

        public CouponService(ApplicationDbContext db)
        {
            _db = db;
        }

        public override async Task<CouponModel> GetCoupon(GetCouponRequest request, ServerCallContext context)
        {
            var coupon = await _db.Coupons
                .FirstOrDefaultAsync(x => x.CouponCode == request.CouponCode);

            if (coupon == null)
            {
                return new CouponModel();
            }

            return new CouponModel
            {
                CouponId = coupon.CouponId,
                CouponCode = coupon.CouponCode,
                DiscountAmount = coupon.DiscountAmount,
                MinAmount = coupon.MinAmount
            };
        }
    }
}
