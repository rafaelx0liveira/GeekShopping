using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController(ICartRepository repository) : ControllerBase
{
    private ICartRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    [HttpGet("GetByUserId/{userId}")]
    public async Task<ActionResult<CartVO>> GetByUserId(string userId)
    {
        var cart = await _repository.GetByUserId(userId);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("AddItem")]
    public async Task<ActionResult<CartVO>> AddItem(CartVO vo)
    {
        var cart = await _repository.SaveOrUpdate(vo);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPut("Update")]
    public async Task<ActionResult<CartVO>> Update(CartVO vo)
    {
        var cart = await _repository.SaveOrUpdate(vo);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpDelete("RemoveItem/{id}")]
    public async Task<ActionResult<CartVO>> RemoveItem(int id)
    {
        var status = await _repository.RemoveItem(id);
        if (!status) return BadRequest();
        return Ok(status);
    }

    [HttpPost("ApplyCoupon")]
    public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO vo)
    {
        var status = await _repository.ApplyCoupon(vo.CartHeader!.UserId!, vo.CartHeader.CouponCode!);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpDelete("RemoveCoupon/{userId}")]
    public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
    {
        var status = await _repository.RemoveCoupon(userId);
        if (!status) return NotFound();
        return Ok(status);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
    {
        var cart = await _repository.GetByUserId(vo.UserId!);
        if (cart == null) return NotFound();
        vo.ListCartDetail = cart.ListCartDetail;
        vo.DateTime = DateTime.Now;

        //TASK RabbitMQ logic comes here!!!

        return Ok(vo);
    }
}