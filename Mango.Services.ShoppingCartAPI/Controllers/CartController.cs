using Mango.Services.ProductAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    // [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponGrpcService _couponGrpcService;

        public CartController(ICartRepository cartRepository, ICouponGrpcService couponGrpcService)
        {
            _cartRepository = cartRepository;
            _couponGrpcService = couponGrpcService;
        }

        private string GetUserIdOrThrow(string? fallbackUserId = null)
        {
            var userId =
                User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User?.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(userId))
                return userId;

            if (!string.IsNullOrWhiteSpace(fallbackUserId))
                return fallbackUserId;

            throw new UnauthorizedAccessException("UserId not found in token. Provide userId explicitly for testing.");
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> GetCart([FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<CartDTO>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                var cart = await _cartRepository.GetCartByUserId(uid);

                response.Result = cart;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error getting cart.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpPost("upsert")]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> Upsert([FromBody] CartDTO cartDto)
        {
            var response = new ResponseDTO<CartDTO>();

            try
            {
                if (cartDto == null)
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "CartDTO is required.";
                    response.ErrorMessages = new List<string> { "Request body is null." };
                    return BadRequest(response);
                }

                var uidFromToken =
                    User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User?.FindFirstValue("sub");

                if (!string.IsNullOrWhiteSpace(uidFromToken))
                    cartDto.CartHeader.UserId = uidFromToken;

                var result = await _cartRepository.UpsertCart(cartDto);
                response.Result = result;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error upserting cart.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        public class SetCountRequest
        {
            public int Count { get; set; }
        }

        [HttpPut("items/{productId:int}")]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> SetCount(
            int productId,
            [FromBody] SetCountRequest req,
            [FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<CartDTO>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                var cart = await _cartRepository.SetItemCount(uid, productId, req.Count);

                response.Result = cart;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error updating item count.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<ActionResult<ResponseDTO<bool>>> RemoveItem(int productId, [FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<bool>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                var ok = await _cartRepository.RemoveFromCart(uid, productId);

                if (!ok)
                {
                    response.IsSuccess = false;
                    response.Result = false;
                    response.DisplayMessage = "Item not found.";
                    response.ErrorMessages = new List<string> { $"Product with id={productId} not found in cart." };
                    return NotFound(response);
                }

                response.Result = true;
                response.DisplayMessage = "Item removed successfully.";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = false;
                response.DisplayMessage = "Error removing item.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<ResponseDTO<bool>>> Clear([FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<bool>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                await _cartRepository.ClearCart(uid);

                response.Result = true;
                response.DisplayMessage = "Cart cleared successfully.";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = false;
                response.DisplayMessage = "Error clearing cart.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpGet("coupon/{couponCode}")]
        public async Task<ActionResult<ResponseDTO<CouponDTO>>> GetCoupon(string couponCode)
        {
            var response = new ResponseDTO<CouponDTO>();

            try
            {
                var coupon = await _couponGrpcService.GetCoupon(couponCode);

                response.Result = new CouponDTO
                {
                    CouponId = coupon.CouponId,
                    CouponCode = coupon.CouponCode,
                    DiscountAmount = (decimal)coupon.DiscountAmount,
                    MinAmount = coupon.MinAmount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error getting coupon from CouponAPI via gRPC.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<ResponseDTO<bool>>> ApplyCoupon([FromQuery] string couponCode, [FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<bool>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                var result = await _cartRepository.ApplyCoupon(uid, couponCode);

                response.Result = result;

                if (!result)
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "Cart not found.";
                    return NotFound(response);
                }

                response.DisplayMessage = "Coupon applied successfully.";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error applying coupon.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }

        [HttpPost("remove-coupon")]
        public async Task<ActionResult<ResponseDTO<bool>>> RemoveCoupon([FromQuery] string? userId = null)
        {
            var response = new ResponseDTO<bool>();

            try
            {
                var uid = GetUserIdOrThrow(userId);
                var result = await _cartRepository.RemoveCoupon(uid);

                response.Result = result;

                if (!result)
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "Cart not found.";
                    return NotFound(response);
                }

                response.DisplayMessage = "Coupon removed successfully.";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Error removing coupon.";
                response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest(response);
            }
        }
    }
}