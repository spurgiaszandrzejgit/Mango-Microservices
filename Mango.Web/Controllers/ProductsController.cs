using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;
        public ProductsController(IProductService productService,
            IWebHostEnvironment env,
            IFileService fileService)
        {
            _productService = productService;
            _env = env;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductsIndex()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.GetProductsAsync(accessToken);

            if (response == null)
            {
                TempData["error"] = "No response from ProductService";
                return View(new List<ProductDTO>());
            }

            if (!response.IsSuccess)
            {
                TempData["error"] = response.ErrorMessages?.FirstOrDefault() ?? "Error while fetching products";
                return View(new List<ProductDTO>());
            }

            return View(response.Result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ProductCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductCreate(ProductVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string imageUrl = await _fileService.SaveImageAsync(vm.ImageFile);

            var dto = new ProductDTO
            {
                Name = vm.Name,
                CategoryName = vm.CategoryName,
                Description = vm.Description,
                Price = vm.Price,
                ImageUrl = imageUrl
            };

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _productService.CreateProductAsync(dto, accessToken);

            if (response == null || !response.IsSuccess)
                return View(vm);

            return RedirectToAction(nameof(ProductsIndex));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductEdit(int productId)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.GetProductByIdAsync(productId, accessToken);

            if (response == null || !response.IsSuccess || response.Result == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction(nameof(ProductsIndex));
            }

            var product = response.Result;

            var vm = new ProductVM
            {
                ProductId = product.ProductId,
                Name = product.Name,
                CategoryName = product.CategoryName,
                Description = product.Description,
                Price = product.Price,
                ExistingImageUrl = product.ImageUrl
            };

            ViewBag.ImageUrl = product.ImageUrl;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductEdit(int productId, ProductVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.GetProductByIdAsync(productId, accessToken);

            if (response == null || !response.IsSuccess || response.Result == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction(nameof(ProductsIndex));
            }

            var existingProduct = response.Result;
            string imageUrl = vm.ExistingImageUrl;
            

            var newImage = await _fileService.SaveImageAsync(vm.ImageFile);
            if (!string.IsNullOrEmpty(newImage))
            {
                _fileService.DeleteImage(vm.ExistingImageUrl);
                imageUrl = newImage;
            }

            var dto = new ProductDTO
            {
                ProductId = productId,
                Name = vm.Name,
                CategoryName = vm.CategoryName,
                Description = vm.Description,
                Price = vm.Price,
                ImageUrl = imageUrl
            };

            var updateResponse = await _productService.UpdateProductAsync(productId, dto, accessToken);

            if (updateResponse == null || !updateResponse.IsSuccess)
            {
                TempData["error"] = "Update failed";
                return View(vm);
            }

            return RedirectToAction(nameof(ProductEdit), new { productId = vm.ProductId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductDelete(int productId)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.GetProductByIdAsync(productId, accessToken);

            if (response == null || !response.IsSuccess || response.Result == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction(nameof(ProductsIndex));
            }

            var p = response.Result;

            var vm = new ProductVM
            {
                ProductId = p.ProductId,
                Name = p.Name,
                CategoryName = p.CategoryName,
                Description = p.Description,
                Price = p.Price,
                ExistingImageUrl = p.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductDelete(ProductVM vm)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var getResponse = await _productService.GetProductByIdAsync(vm.ProductId, accessToken);
            if (getResponse == null || !getResponse.IsSuccess || getResponse.Result == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction(nameof(ProductsIndex));
            }

            var imageUrl = getResponse.Result.ImageUrl;

            var deleteResponse = await _productService.DeleteProductAsync(vm.ProductId, accessToken);

            if (deleteResponse == null || !deleteResponse.IsSuccess || deleteResponse.Result == false)
            {
                TempData["error"] = "Delete failed";
                return RedirectToAction(nameof(ProductsIndex));
            }

            _fileService.DeleteImage(vm.ExistingImageUrl);

            TempData["success"] = "Product deleted";
            return RedirectToAction(nameof(ProductsIndex));
        }


        [Authorize]
        public async Task<IActionResult> Details(int productId)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["error"] = "Access token is missing. Please login again.";
                return RedirectToAction("Login", "Home");
            }

            var response = await _productService.GetProductByIdAsync(productId, token);

            if (response == null)
            {
                TempData["error"] = "No response from ProductService";
                return View(new ProductDTO());
            }

            if (!response.IsSuccess)
            {
                TempData["error"] = response.ErrorMessages?.FirstOrDefault() ?? "Error while fetching products";
                return View(new ProductDTO());
            }

            return View(response.Result);
        }
    }
}
