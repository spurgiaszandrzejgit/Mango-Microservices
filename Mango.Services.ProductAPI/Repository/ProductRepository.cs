using AutoMapper;
using Mango.Services.ProductAPI.DbContexts;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public ProductRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductDTO productDTO)
        {
            var product = _mapper.Map<Product>(productDTO);

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null) return false;

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int productId)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            return product == null ? null : _mapper.Map<ProductDTO>(product);
        }


        public async Task<IEnumerable<ProductDTO>> GetProductsAsync()
        {
            var products = await _db.Products
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductDTO>>(products);
        }

        public async Task<ProductDTO?> UpdateProductAsync(ProductDTO productDTO)
        {
            var existing = await _db.Products
                .FirstOrDefaultAsync(x => x.ProductId == productDTO.ProductId);

            if (existing == null) return null;

            _mapper.Map(productDTO, existing);

            await _db.SaveChangesAsync();

            return _mapper.Map<ProductDTO>(existing);
        }
    }
}
