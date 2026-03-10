using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Repository;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CartRepository(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CartDTO> GetCartByUserId(string userId)
    {
        var header = await _db.CartHeaders.AsNoTracking()
            .FirstOrDefaultAsync(h => h.UserId == userId);

        if (header == null) return new CartDTO();

        var details = await _db.CartDetails.AsNoTracking()
            .Where(d => d.CartHeaderId == header.CartHeaderId)
            .ToListAsync();

        var cartDto = new CartDTO
        {
            CartHeader = _mapper.Map<CartHeaderDTO>(header),
            CartDetails = _mapper.Map<List<CartDetailsDTO>>(details)
        };

        cartDto.CartHeader.OrderTotal = details.Sum(item => item.Price * item.Count);

        return cartDto;
    }

    // ✅ add/increase (upsert)
    public async Task<CartDTO> UpsertCart(CartDTO cartDTO)
    {
        if (cartDTO.CartHeader == null || cartDTO.CartDetails == null || cartDTO.CartDetails.Count == 0)
            throw new ArgumentException("CartDTO must contain CartHeader and at least one CartDetails item.");

        var userId = cartDTO.CartHeader.UserId;
        var incoming = cartDTO.CartDetails[0];

        await using var tx = await _db.Database.BeginTransactionAsync();

        var header = await _db.CartHeaders
            .FirstOrDefaultAsync(h => h.UserId == userId);

        if (header == null)
        {
            header = new CartHeader
            {
                UserId = userId,
                CouponCode = cartDTO.CartHeader.CouponCode
            };
            _db.CartHeaders.Add(header);
            await _db.SaveChangesAsync();
        }

        var item = await _db.CartDetails
            .FirstOrDefaultAsync(d => d.CartHeaderId == header.CartHeaderId && d.ProductId == incoming.ProductId);

        if (item == null)
        {
            item = new CartDetails
            {
                CartHeaderId = header.CartHeaderId,
                ProductId = incoming.ProductId,
                Count = incoming.Count,

                // snapshot
                ProductName = incoming.ProductName,
                Price = incoming.Price,
                ImageUrl = incoming.ImageUrl,
                CategoryName = incoming.CategoryName
            };
            _db.CartDetails.Add(item);
        }
        else
        {
            item.Count += incoming.Count;

            // updte snapshot after add
            item.ProductName = incoming.ProductName;
            item.Price = incoming.Price;
            item.ImageUrl = incoming.ImageUrl;
            item.CategoryName = incoming.CategoryName;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await GetCartByUserId(userId);
    }

    // ✅ set count
    public async Task<CartDTO> SetItemCount(string userId, int productId, int count)
    {
        var header = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == userId);
        if (header == null) return new CartDTO();

        var item = await _db.CartDetails
            .FirstOrDefaultAsync(d => d.CartHeaderId == header.CartHeaderId && d.ProductId == productId);

        if (item == null)
            return await GetCartByUserId(userId);

        if (count <= 0)
        {
            _db.CartDetails.Remove(item);
            await _db.SaveChangesAsync();

            // if basket is empty — remove header
            var any = await _db.CartDetails.AnyAsync(d => d.CartHeaderId == header.CartHeaderId);
            if (!any)
            {
                _db.CartHeaders.Remove(header);
                await _db.SaveChangesAsync();
                return new CartDTO();
            }

            return await GetCartByUserId(userId);
        }

        item.Count = count;
        await _db.SaveChangesAsync();

        return await GetCartByUserId(userId);
    }

    public async Task<bool> RemoveFromCart(string userId, int productId)
    {
        var header = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == userId);
        if (header == null) return true;

        var item = await _db.CartDetails
            .FirstOrDefaultAsync(d => d.CartHeaderId == header.CartHeaderId && d.ProductId == productId);

        if (item == null) return true;

        _db.CartDetails.Remove(item);
        await _db.SaveChangesAsync();

        var any = await _db.CartDetails.AnyAsync(d => d.CartHeaderId == header.CartHeaderId);
        if (!any)
        {
            _db.CartHeaders.Remove(header);
            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> ClearCart(string userId)
    {
        var header = await _db.CartHeaders
            .FirstOrDefaultAsync(h => h.UserId == userId);

        if (header == null) return true;

        var items = await _db.CartDetails
            .Where(d => d.CartHeaderId == header.CartHeaderId)
            .ToListAsync();

        _db.CartDetails.RemoveRange(items);
        _db.CartHeaders.Remove(header);

        await _db.SaveChangesAsync();
        return true;
    }
}