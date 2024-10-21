using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.CartAPI.Repository.Interfaces;

public interface ICartRepository
{
    Task<CartVO> GetByUserId(string userId);
    Task<CartVO> SaveOrUpdate(CartVO cartVO);
    Task<bool> RemoveItem(long cartDetailId);
    Task<bool> ApplyCoupon(string userId, string couponCode);
    Task<bool> RemoveCoupon(string userId);
    Task<bool> Clear(string userId);
}