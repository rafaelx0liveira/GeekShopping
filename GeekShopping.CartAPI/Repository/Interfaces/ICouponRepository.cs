using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.CartAPI.Repository.Interfaces;

public interface ICouponRepository
{
    Task<CouponVO?> GetByCode(string code, string accessToken);
}