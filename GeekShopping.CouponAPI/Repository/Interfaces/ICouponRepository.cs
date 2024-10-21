using GeekShopping.CouponAPI.Data.ValueObjects;

namespace GeekShopping.CouponAPI.Repository.Interfaces;

public interface ICouponRepository
{
    Task<CouponVO> GetByCode(string code);
}