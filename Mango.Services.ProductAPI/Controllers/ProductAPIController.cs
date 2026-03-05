using Mango.Services.ProductAPI.Models.DTO;
using Mango.Services.ProductAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductAPIController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductAPIController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<IEnumerable<ProductDTO>>>> Get()
        {
            try
            {
                var products = await _productRepository.GetProductsAsync();
                return Ok(new ResponseDTO<IEnumerable<ProductDTO>> { Result = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<IEnumerable<ProductDTO>>
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> GetById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new ResponseDTO<ProductDTO>
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string> { $"Product with id={id} not found." }
                    });
                }

                return Ok(new ResponseDTO<ProductDTO> { Result = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<ProductDTO>
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> Create([FromBody] ProductDTO productDTO)
        {
            try
            {
                var created = await _productRepository.CreateProductAsync(productDTO);

                return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, new ResponseDTO<ProductDTO>
                {
                    Result = created,
                    DisplayMessage = "Product created."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<ProductDTO>
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> Update(int id, [FromBody] ProductDTO productDTO)
        {
            try
            {
                if (productDTO.ProductId != id)
                {
                    return BadRequest(new ResponseDTO<ProductDTO>
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string>
                        {
                            $"Route id ({id}) does not match ProductId ({productDTO.ProductId}) in request body."
                        }
                    });
                }

                var updated = await _productRepository.UpdateProductAsync(productDTO);

                if (updated == null)
                {
                    return NotFound(new ResponseDTO<ProductDTO>
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string> { $"Product with id={id} not found." }
                    });
                }

                return Ok(new ResponseDTO<ProductDTO>
                {
                    Result = updated,
                    DisplayMessage = "Product updated."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<ProductDTO>
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<ResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var deleted = await _productRepository.DeleteProductAsync(id);

                if (!deleted)
                {
                    return NotFound(new ResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Result = false,
                        ErrorMessages = new List<string> { $"Product with id={id} not found." }
                    });
                }

                return Ok(new ResponseDTO<bool>
                {
                    Result = true,
                    DisplayMessage = "Product deleted."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Result = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

    }
}
