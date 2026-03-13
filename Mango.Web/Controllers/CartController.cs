using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            {
                TempData["error"] = "Please login again.";
                return RedirectToAction("Login", "Home");
            }

            var response = await _cartService.GetCartAsync(userId, token);

            if (response != null && response.IsSuccess && response.Result != null)
            {
                return View(response.Result);
            }

            return View(new CartDTO());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int count)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(userId))
            {
                await _cartService.SetItemCountAsync(productId, count, userId, token);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(userId))
            {
                await _cartService.RemoveItemAsync(productId, userId, token);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            {
                TempData["error"] = "Please login again.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _cartService.ApplyCouponAsync(userId, couponCode, token);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = $"Coupon '{couponCode}' applied.";
            }
            else
            {
                TempData["error"] = response?.DisplayMessage ?? "Error applying coupon.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            {
                TempData["error"] = "Please login again.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _cartService.RemoveCouponAsync(userId, token);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon removed.";
            }
            else
            {
                TempData["error"] = response?.DisplayMessage ?? "Error removing coupon.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.FirstOrDefault(u => u.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            {
                TempData["error"] = "Please login again.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _cartService.GetCartAsync(userId, token);

            if (response == null || !response.IsSuccess || response.Result == null)
            {
                TempData["error"] = "Cart not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Result);
        }
    }
}