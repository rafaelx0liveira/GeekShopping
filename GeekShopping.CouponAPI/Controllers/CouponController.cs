using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class CouponController(ICouponRepository repository) : ControllerBase
{
    private readonly ICouponRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    [HttpGet("{code}")]
    public async Task<ActionResult<CouponVO>> GetByCode(string code)
    {
        var coupon = await _repository.GetByCode(code);
        if (coupon == null) return NotFound();
        return Ok(coupon);
    }
}