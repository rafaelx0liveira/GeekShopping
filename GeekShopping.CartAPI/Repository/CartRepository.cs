using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly MySQLContext _context;
        private IMapper _mapper;

        public CartRepository(MySQLContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeader = await _context.CartHeaders
                .FirstOrDefaultAsync(ch => ch.UserId == userId);

            if (cartHeader != null)
            {
                _context.CartDetails.RemoveRange(
                    _context.CartDetails.Where(
                            cd => cd.CartHeaderId == cartHeader.Id
                        )
                    );

                _context.CartHeaders.Remove(cartHeader);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            Cart cart = new()
            {
                CartHeader = await _context.CartHeaders.
                FirstOrDefaultAsync(ch => ch.UserId == userId),
            };

            cart.CartDetails = await _context.CartDetails
                .Where(cd => cd.CartHeaderId == cart.CartHeader.Id)
                .Include(cd => cd.Product)
                .ToListAsync();

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFromCart(long cartDetailId)
        {
            try
            {
                CartDetail cartDetail = await _context.CartDetails
                    .FirstOrDefaultAsync(cd => cd.Id == cartDetailId);

                int total = _context.CartDetails
                    .Where(cd => cd.CartHeaderId == cartDetail.CartHeaderId)
                    .Count();

                _context.CartDetails.Remove(cartDetail);

                if(total == 1)
                {
                    var cartHeaderToRemove = await _context.CartHeaders
                        .FirstOrDefaultAsync(ch => ch.Id == cartDetail.CartHeaderId);

                    _context.CartHeaders.Remove(cartHeaderToRemove);

                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO cartVO)
        {
            Cart cart = _mapper.Map<Cart>(cartVO);

            // Check if the cart already exists in the database
            var product = await _context.Products.FirstOrDefaultAsync(
                p => p.Id == cartVO.CartDetails.FirstOrDefault().ProductId);

            if (product == null)
            {
                // If the product does not exist, add it to the database
                _context.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _context.SaveChangesAsync();
            }

            // Check if CartHeader is null
            var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(
                ch => ch.UserId == cart.CartHeader.UserId);

            if (cartHeader == null)
            {
                // Create CartHeader and CartDetail
                _context.CartHeaders.Add(cart.CartHeader);
                await _context.SaveChangesAsync();

                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                cart.CartDetails.FirstOrDefault().Product = null;

                _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _context.SaveChangesAsync();

            } else
            {
                // If CartHeader is not null
                // Check if CartDetail has the same product

                var cartDetail = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    cd => cd.ProductId == cart.CartDetails.FirstOrDefault().ProductId 
                    &&
                    cd.CartHeaderId == cartHeader.Id);

                if (cartDetail == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
                    cart.CartDetails.FirstOrDefault().Product = null;

                    _context.CartDetails.Add(cart.CartDetails.FirstOrDefault());

                    await _context.SaveChangesAsync();
                } else
                {
                    cart.CartDetails.FirstOrDefault().Product = null;

                    cart.CartDetails.FirstOrDefault().Count += cartDetail.Count;
                    cart.CartDetails.FirstOrDefault().Id = cartDetail.Id;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetail.CartHeaderId;

                    _context.CartDetails.Update(cart.CartDetails.FirstOrDefault());

                    await _context.SaveChangesAsync();
                }
            }

            return _mapper.Map<CartVO>(cart);
        }
    }
}
