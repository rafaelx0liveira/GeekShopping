using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository;

public class CartRepository(MySQLContext context, IMapper mapper) : ICartRepository
{
    private readonly MySQLContext _context = context;
    private IMapper _mapper = mapper;

    public async Task<bool> ApplyCoupon(string userId, string couponCode)
    {
        var header = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
        if (header != null)
        {
            header.CouponCode = couponCode;
            _context.CartHeaders.Update(header);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> RemoveCoupon(string userId)
    {
        var header = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);
        if (header != null)
        {
            header.CouponCode = "";
            _context.CartHeaders.Update(header);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> Clear(string userId)
    {
        var cartHeader = await _context.CartHeaders
                    .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cartHeader != null)
        {
            _context.CartDetails
                .RemoveRange(
                _context.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id));
            _context.CartHeaders.Remove(cartHeader);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<CartVO> GetByUserId(string userId)
    {
        Cart cart = new()
        {
            CartHeader = await _context.CartHeaders
                .FirstOrDefaultAsync(c => c.UserId == userId),
        };
        cart.ListCartDetail = _context.CartDetails
            .Where(c => c.CartHeaderId == cart.CartHeader!.Id)
                .Include(c => c.Product);
        return _mapper.Map<CartVO>(cart);
    }

    public async Task<bool> RemoveItem(long cartDetailsId)
    {
        try
        {
            CartDetail? cartDetail = await _context.CartDetails.FirstOrDefaultAsync(c => c.Id == cartDetailsId);

            int total = _context.CartDetails
                .Where(c => c.CartHeaderId == cartDetail!.CartHeaderId).Count();

            _context.CartDetails.Remove(cartDetail!);

            if (total == 1)
            {
                var cartHeaderToRemove = await _context.CartHeaders
                    .FirstOrDefaultAsync(c => c.Id == cartDetail!.CartHeaderId);
                _context.CartHeaders.Remove(cartHeaderToRemove!);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<CartVO> SaveOrUpdate(CartVO vo)
    {
        Cart cart = _mapper.Map<Cart>(vo);
        //Checks if the product is already saved in the database if it does not exist then save
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == vo.ListCartDetail!.FirstOrDefault()!.ProductId);

        if (product == null)
        {
            _context.Products.Add(cart.ListCartDetail!.FirstOrDefault()!.Product!);
            await _context.SaveChangesAsync();
        }

        //Check if CartHeader is null

        var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(
            c => c.UserId == cart.CartHeader!.UserId);

        if (cartHeader == null)
        {
            //Create CartHeader and CartDetails
            _context.CartHeaders.Add(cart.CartHeader!);
            await _context.SaveChangesAsync();
            cart.ListCartDetail!.FirstOrDefault()!.CartHeaderId = cart.CartHeader!.Id;
            cart.ListCartDetail!.FirstOrDefault()!.Product = null;
            _context.CartDetails.Add(cart.ListCartDetail!.FirstOrDefault()!);
            await _context.SaveChangesAsync();
        }
        else
        {
            //If CartHeader is not null
            //Check if CartDetails has same product
            var cartDetail = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                p => p.ProductId == cart.ListCartDetail!.FirstOrDefault()!.ProductId &&
                p.CartHeaderId == cartHeader.Id);

            if (cartDetail == null)
            {
                //Create CartDetails
                cart.ListCartDetail!.FirstOrDefault()!.CartHeaderId = cartHeader!.Id;
                cart.ListCartDetail!.FirstOrDefault()!.Product = null;
                _context.CartDetails.Add(cart.ListCartDetail!.FirstOrDefault()!);
                await _context.SaveChangesAsync();
            }
            else
            {
                //Update product count and CartDetails
                cart.ListCartDetail!.FirstOrDefault()!.Product = null;
                cart.ListCartDetail!.FirstOrDefault()!.Count += cartDetail.Count;
                cart.ListCartDetail!.FirstOrDefault()!.Id = cartDetail.Id;
                cart.ListCartDetail!.FirstOrDefault()!.CartHeaderId = cartDetail.CartHeaderId;
                _context.CartDetails.Update(cart.ListCartDetail!.FirstOrDefault()!);
                await _context.SaveChangesAsync();
            }
        }
        return _mapper.Map<CartVO>(cart);
    }
}