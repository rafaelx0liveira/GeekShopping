using AutoMapper;
using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model.Context;
using GeekShopping.CouponAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Repository;

public class CouponRepository(MySQLContext context, IMapper mapper) : ICouponRepository
{
    private readonly MySQLContext _context = context;
    private IMapper _mapper = mapper;

    public async Task<CouponVO> GetByCode(string code)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        return _mapper.Map<CouponVO>(coupon);
    }
}