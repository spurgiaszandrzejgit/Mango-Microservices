using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    // [Authorize] // включи, когда подключишь JWT111
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        // В реале userId берут из JWT (sub / nameidentifier),
        // но чтобы тебе было удобно тестировать — оставлю fallback query-параметр.
        private string GetUserIdOrThrow(string? fallbackUserId = null)
        {
            // 1) Из JWT
            var userId =
                User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User?.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(userId))
                return userId;

            // 2) Fallback для локального теста без авторизации
            if (!string.IsNullOrWhiteSpace(fallbackUserId))
                return fallbackUserId;

            throw new UnauthorizedAccessException("UserId not found in token. Provide userId explicitly for testing.");
        }

        // GET api/cart?userId=abc   (userId опционален, если есть JWT)
        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart([FromQuery] string? userId = null)
        {
            var uid = GetUserIdOrThrow(userId);
            var cart = await _cartRepository.GetCartByUserId(uid);
            return Ok(cart);
        }

        // POST api/cart/upsert
        // Body: CartDTO (обычно с 1 item в CartDetails)
        [HttpPost("upsert")]
        public async Task<ActionResult<CartDTO>> Upsert([FromBody] CartDTO cartDto)
        {
            if (cartDto == null)
                return BadRequest("CartDTO is required.");

            // userId лучше не доверять с клиента, если есть JWT
            // но для теста оставим: если токен есть — перезапишем.
            var uidFromToken =
                User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User?.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(uidFromToken))
                cartDto.CartHeader.UserId = uidFromToken;

            var result = await _cartRepository.UpsertCart(cartDto);
            return Ok(result);
        }

        // PUT api/cart/items/{productId}
        // Body: { "count": 3 }
        public class SetCountRequest
        {
            public int Count { get; set; }
        }

        [HttpPut("items/{productId:int}")]
        public async Task<ActionResult<CartDTO>> SetCount(int productId, [FromBody] SetCountRequest req, [FromQuery] string? userId = null)
        {
            var uid = GetUserIdOrThrow(userId);

            var cart = await _cartRepository.SetItemCount(uid, productId, req.Count);
            return Ok(cart);
        }

        // DELETE api/cart/items/{productId}
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId, [FromQuery] string? userId = null)
        {
            var uid = GetUserIdOrThrow(userId);

            var ok = await _cartRepository.RemoveFromCart(uid, productId);
            if (!ok) return NotFound();

            return NoContent();
        }

        // DELETE api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> Clear([FromQuery] string? userId = null)
        {
            var uid = GetUserIdOrThrow(userId);

            await _cartRepository.ClearCart(uid);
            return NoContent();
        }
    }
}